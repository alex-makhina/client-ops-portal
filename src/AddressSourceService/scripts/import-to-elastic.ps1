# =====================================================================
# import-to-elastic.ps1 — запускает AddressValidator.Importer для индексации
#                         данных из PostgreSQL в Elasticsearch.
#
# Запуск из корня проекта:
#     powershell -ExecutionPolicy Bypass -File .\scripts\import-to-elastic.ps1
#
# Предварительно:
#   1. Docker: docker compose up -d   (должны работать postgres + elasticsearch)
#   2. В PostgreSQL уже загружены данные (normalize-osm.ps1 отработал)
#   3. Запустите Api хотя бы один раз, чтобы миграции применились
#      (это не строго обязательно для импорта, но рекомендуется).
# =====================================================================
[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"
$ProjectDir = Split-Path -Parent $PSScriptRoot
$ImporterProject = Join-Path $ProjectDir "src\AddressValidator.Importer\AddressValidator.Importer.csproj"

if (-not (Test-Path $ImporterProject)) {
    Write-Error "Проект Importer не найден: $ImporterProject"
    exit 1
}

# CWD = корень проекта. Важно для MappingFile (путь "config/elasticsearch/...").
# Importer читает appsettings.json из AppContext.BaseDirectory (bin/...), а
# MappingFile грузит через File.ReadAllText — для этого нужен CWD = корень solution.
Push-Location $ProjectDir
try {
    Write-Host "==> Рабочая директория: $(Get-Location)" -ForegroundColor DarkGray

    Write-Host "==> Проверка доступности Elasticsearch ..." -ForegroundColor Cyan
    try {
        $esHealth = Invoke-RestMethod -Uri "http://localhost:9200/_cluster/health" -UseBasicParsing -TimeoutSec 5
        Write-Host "    ES status: $($esHealth.status)" -ForegroundColor Green
    } catch {
        Write-Error "Elasticsearch недоступен на http://localhost:9200. Запустите: docker compose up -d elasticsearch"
        exit 1
    }

    Write-Host ""
    Write-Host "==> Запуск AddressValidator.Importer ..." -ForegroundColor Cyan
    Write-Host "    Это может занять 1-3 минуты в зависимости от объёма данных."
    Write-Host ""

    dotnet run --project $ImporterProject --no-launch-profile
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Importer завершился с ошибкой (exit code $LASTEXITCODE)"
        exit 1
    }

    Write-Host ""
    Write-Host "==> Готово." -ForegroundColor Green
    Write-Host ""
    Write-Host "Проверьте количество документов в индексе:"
    Write-Host "  curl http://localhost:9200/addresses/_count"
    Write-Host ""
    Write-Host "Пример запроса автодополнения:"
    Write-Host '  curl "http://localhost:5000/api/addresses/suggest?query=минск+незави&limit=10"'
    Write-Host ""
    Write-Host "Теперь можно запустить Api и тестировать /api/addresses/suggest:"
    Write-Host "  dotnet run --project .\src\AddressValidator.Api"
}
finally {
    Pop-Location
}
