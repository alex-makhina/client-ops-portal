# =====================================================================
# normalize-osm.ps1 — запускает scripts/normalize-osm.sql против PostgreSQL.
#
# Запуск из корня проекта (после того, как миграции создали address_objects):
#     powershell -ExecutionPolicy Bypass -File .\scripts\normalize-osm.ps1
#
# Заполняет таблицу address_objects из сырых OSM-таблиц planet_osm_*.
# UUID генерируется как uuid_generate_v5(namespace, 'type:' || osm_id::text).
# =====================================================================
[CmdletBinding()]
param(
    [string]$PgHost     = $env:PGHOST,
    [string]$PgPort     = $env:PGPORT,
    [string]$PgUser     = $env:PGUSER,
    [string]$PgDatabase = $env:PGDATABASE,
    [string]$PgPassword = $env:PGPASSWORD
)

$ErrorActionPreference = "Stop"
$ScriptDir  = $PSScriptRoot
$ProjectDir = Split-Path -Parent $ScriptDir
$SqlFile    = Join-Path $ScriptDir "normalize-osm.sql"

if (-not (Test-Path $SqlFile)) {
    Write-Error "Файл normalize-osm.sql не найден: $SqlFile"
    exit 1
}

# Defaults (должны совпадать с docker-compose.yml)
if ([string]::IsNullOrWhiteSpace($PgHost))     { $PgHost = "localhost" }
if ([string]::IsNullOrWhiteSpace($PgPort))     { $PgPort = "5432" }
if ([string]::IsNullOrWhiteSpace($PgUser))     { $PgUser = "addr" }
if ([string]::IsNullOrWhiteSpace($PgDatabase)) { $PgDatabase = "addresses" }
if ([string]::IsNullOrWhiteSpace($PgPassword)) { $PgPassword = "addr_secret" }

$env:PGPASSWORD = $PgPassword

Write-Host "==> Нормализация OSM -> address_objects" -ForegroundColor Cyan
Write-Host "    SQL файл: $SqlFile"
Write-Host "    База:     $PgDatabase @ ${PgHost}:$PgPort"
Write-Host ""

# Сначала проверяем, что таблица address_objects существует
# (создаётся миграцией EF Core при первом старте Api).
Write-Host "==> Проверка таблицы address_objects ..." -ForegroundColor Cyan
$tableCheckSql = "SELECT to_regclass('public.address_objects');"
$psqlCmd = Get-Command psql -ErrorAction SilentlyContinue

if ($psqlCmd) {
    $result = & psql --host=$PgHost --port=$PgPort --username=$PgUser --dbname=$PgDatabase -tAc $tableCheckSql 2>$null
    $tableExists = ($result -match 'address_objects')
}
else {
    $result = docker exec -i addr-postgres psql -U $PgUser -d $PgDatabase -tAc $tableCheckSql 2>$null
    $tableExists = ($result -match 'address_objects')
}

if (-not $tableExists) {
    Write-Error @"
Таблица address_objects не найдена в базе $PgDatabase.
Сначала запустите Api, чтобы EF Core применил миграции:

    dotnet run --project .\src\AddressValidator.Api

Дождитесь сообщения 'Now listening on: http://localhost:5000', нажмите Ctrl+C,
затем повторите этот скрипт.
"@
    exit 1
}
Write-Host "    address_objects: OK" -ForegroundColor Green

# Запуск normalize-osm.sql
Write-Host ""
Write-Host "==> Запуск normalize-osm.sql ..." -ForegroundColor Cyan

# Передаём -v ON_ERROR_STOP=1 чтобы psql упал при первой ошибке, а не молча
# пропускал (предыдущая версия скрипта из-за этого скрыла сломанный DO-блок).
#
# Если psql установлен на хосте — запускаем напрямую с --file (psql читает UTF-8).
# Если нет — копируем .sql в контейнер через docker cp (бинарно, без перекодировки
# PowerShell'ом) и запускаем psql -f внутри контейнера. Прямой пайп
# Get-Content | docker exec ломает кириллицу в \echo (PowerShell перекодирует
# строку в консольную кодировку cp1251/cp866 при передаче через stdin).
if ($psqlCmd) {
    & psql --host=$PgHost --port=$PgPort --username=$PgUser --dbname=$PgDatabase -v ON_ERROR_STOP=1 --file=$SqlFile
    if ($LASTEXITCODE -ne 0) {
        Write-Error "normalize-osm.sql завершился с ошибкой (exit code $LASTEXITCODE). Смотрите вывод выше."
        exit 1
    }
}
else {
    Write-Host "psql не найден на хосте — копирую SQL в контейнер и выполняю там ..." -ForegroundColor Yellow
    docker cp $SqlFile addr-postgres:/tmp/normalize-osm.sql
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Не удалось скопировать $SqlFile в контейнер addr-postgres"
        exit 1
    }
    docker exec addr-postgres psql -U $PgUser -d $PgDatabase -v ON_ERROR_STOP=1 -f /tmp/normalize-osm.sql
    if ($LASTEXITCODE -ne 0) {
        Write-Error "normalize-osm.sql завершился с ошибкой (exit code $LASTEXITCODE). Смотрите вывод выше."
        exit 1
    }
}

Write-Host ""
Write-Host "==> Готово." -ForegroundColor Green
Write-Host ""
Write-Host "Проверьте результат:"
Write-Host "  docker exec addr-postgres psql -U addr -d addresses -c 'SELECT type, COUNT(*) FROM address_objects GROUP BY type ORDER BY type;'"
Write-Host "  docker exec addr-postgres psql -U addr -d addresses -c 'SELECT * FROM v_city_stats LIMIT 5;'"
Write-Host ""
Write-Host "Получить UUID здания для теста API:"
Write-Host "  docker exec addr-postgres psql -U addr -d addresses -c 'SELECT id, full_path FROM address_objects WHERE type=''building'' LIMIT 3;'"
Write-Host ""
Write-Host "Теперь можно запустить Api и сделать GET /api/addresses/{id}:"
Write-Host "  dotnet run --project .\src\AddressValidator.Api"
