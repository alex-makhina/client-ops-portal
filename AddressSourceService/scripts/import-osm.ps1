# =====================================================================
# import-osm.ps1 — загружает .osm.pbf в PostgreSQL через osm2pgsql.
#
# Создаёт сырые таблицы planet_osm_point, planet_osm_line, planet_osm_polygon
# со всеми OSM-тегами в колонке tags (hstore).
#
# Запуск из корня проекта:
#     powershell -ExecutionPolicy Bypass -File .\scripts\import-osm.ps1 -PbfFile .\data\minsk.osm.pbf
#
# Если osm2pgsql не установлен на хосте — скрипт установит его в контейнер
# addr-postgres через apt-get (одноразово, ~30 сек).
#
# ВАЖНО: Этот скрипт НЕ создаёт таблицу address_objects. Запустите
# `dotnet run --project .\src\AddressValidator.Api` один раз, чтобы EF Core
# применил миграции (создаст таблицу), а затем `.\scripts\normalize-osm.ps1`
# для заполнения address_objects из planet_osm_*.
# =====================================================================
[CmdletBinding()]
param(
    [string]$PbfFile = "",
    [string]$PgHost     = $env:PGHOST,
    [string]$PgPort     = $env:PGPORT,
    [string]$PgUser     = $env:PGUSER,
    [string]$PgDatabase = $env:PGDATABASE,
    [string]$PgPassword = $env:PGPASSWORD
)

$ErrorActionPreference = "Stop"
$ProjectDir = Split-Path -Parent $PSScriptRoot

if ([string]::IsNullOrWhiteSpace($PbfFile)) {
    $PbfFile = Join-Path $ProjectDir "data\minsk.osm.pbf"
}

if (-not (Test-Path $PbfFile)) {
    Write-Error "Файл выгрузки не найден: $PbfFile`nСначала запустите: .\scripts\download-osm.ps1"
    exit 1
}

# Defaults (должны совпадать с docker-compose.yml)
if ([string]::IsNullOrWhiteSpace($PgHost))     { $PgHost = "localhost" }
if ([string]::IsNullOrWhiteSpace($PgPort))     { $PgPort = "5432" }
if ([string]::IsNullOrWhiteSpace($PgUser))     { $PgUser = "addr" }
if ([string]::IsNullOrWhiteSpace($PgDatabase)) { $PgDatabase = "addresses" }
if ([string]::IsNullOrWhiteSpace($PgPassword)) { $PgPassword = "addr_secret" }

$env:PGPASSWORD = $PgPassword

Write-Host "==> Импорт OSM в PostgreSQL (через osm2pgsql)" -ForegroundColor Cyan
Write-Host "    Файл:     $PbfFile"
Write-Host "    База:     $PgDatabase @ ${PgHost}:$PgPort"
Write-Host ""

# 1. Определяем, как запускать osm2pgsql: локально или через docker exec
$osm2pgsqlCmd = Get-Command osm2pgsql -ErrorAction SilentlyContinue
if ($osm2pgsqlCmd) {
    $osm2pgsqlExe       = "osm2pgsql"
    $osm2pgsqlPrefixArgs = @()
    Write-Host "==> Использую локальный osm2pgsql: $($osm2pgsqlCmd.Source)" -ForegroundColor Green
}
else {
    # Проверяем, запущен ли контейнер addr-postgres
    $containerRunning = $false
    try {
        $containers = docker ps --format '{{.Names}}' 2>$null
        if ($containers -match 'addr-postgres') {
            $containerRunning = $true
        }
    } catch {
        Write-Warning "Не удалось проверить контейнеры Docker: $($_.Exception.Message)"
    }

    if (-not $containerRunning) {
        Write-Error "osm2pgsql не найден на хосте, и контейнер addr-postgres не запущен.`nСначала: docker compose up -d postgres"
        exit 1
    }

    Write-Host "==> osm2pgsql не найден на хосте, устанавливаю в контейнер addr-postgres ..." -ForegroundColor Yellow
    docker exec -i addr-postgres bash -c "apt-get update -qq && apt-get install -y -qq osm2pgsql" | Out-Null

    # Копируем .pbf в контейнер (osm2pgsql внутри контейнера не видит файлы хоста)
    Write-Host "==> Копирую $PbfFile в контейнер ..." -ForegroundColor Yellow
    docker cp $PbfFile addr-postgres:/tmp/import.osm.pbf
    $PbfFile = "/tmp/import.osm.pbf"

    $osm2pgsqlExe       = "docker"
    $osm2pgsqlPrefixArgs = @("exec", "-i", "addr-postgres", "osm2pgsql")
}

# 2. Убедимся, что расширение hstore установлено (osm2pgsql --hstore его требует).
# Миграции EF Core тоже его создают, но на случай если osm2pgsql запускается ДО
# первого старта Api — дублируем CREATE EXTENSION IF NOT EXISTS (idempotent).
Write-Host "==> Проверка расширения hstore в PostgreSQL ..." -ForegroundColor Cyan
$hstoreSql = "CREATE EXTENSION IF NOT EXISTS hstore;"
$psqlCmd = Get-Command psql -ErrorAction SilentlyContinue
if ($psqlCmd) {
    & psql --host=$PgHost --port=$PgPort --username=$PgUser --dbname=$PgDatabase -c $hstoreSql | Out-Null
} else {
    docker exec -i addr-postgres psql -U $PgUser -d $PgDatabase -c $hstoreSql | Out-Null
}
if ($LASTEXITCODE -ne 0) {
    Write-Error "Не удалось установить расширение hstore"
    exit 1
}
Write-Host "    hstore: OK" -ForegroundColor Green

# 3. Запуск osm2pgsql
Write-Host "==> Запуск osm2pgsql ..." -ForegroundColor Cyan
Write-Host "    --create     (создаёт таблицы planet_osm_*)"
Write-Host "    --slim       (использует промежуточные таблицы для экономии RAM)"
Write-Host "    --hstore     (теги OSM в колонке tags типа hstore)"
Write-Host "    --latlong    (координаты в WGS84 — как в OSM)"
Write-Host ""

$allArgs = @()
$allArgs += $osm2pgsqlPrefixArgs
$allArgs += @(
    "--create",
    "--slim",
    "--hstore",
    "--latlong",
    "--number-processes=2",
    "--host=$PgHost",
    "--port=$PgPort",
    "--username=$PgUser",
    "--database=$PgDatabase",
    $PbfFile
)

& $osm2pgsqlExe @allArgs
if ($LASTEXITCODE -ne 0) {
    Write-Error "osm2pgsql завершился с ошибкой (exit code $LASTEXITCODE)"
    exit 1
}

Write-Host ""
Write-Host "==> osm2pgsql завершил загрузку." -ForegroundColor Green
Write-Host ""
Write-Host "Проверьте сырые таблицы OSM:"
Write-Host "  docker exec addr-postgres psql -U addr -d addresses -c `"SELECT COUNT(*) FROM planet_osm_point;`""
Write-Host "  docker exec addr-postgres psql -U addr -d addresses -c `"SELECT COUNT(*) FROM planet_osm_line;`""
Write-Host "  docker exec addr-postgres psql -U addr -d addresses -c `"SELECT COUNT(*) FROM planet_osm_polygon;`""
Write-Host ""
Write-Host "Следующие шаги:"
Write-Host "  1. dotnet run --project .\src\AddressValidator.Api   (применит миграции, создаст address_objects)"
Write-Host "     Нажмите Ctrl+C после сообщения 'Now listening on: http://localhost:5000'"
Write-Host "  2. .\scripts\normalize-osm.ps1                       (заполнит address_objects из planet_osm_*)"
Write-Host "  3. dotnet run --project .\src\AddressValidator.Api   (теперь GET /api/addresses/{id} работает)"
