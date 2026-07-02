# =====================================================================
# download-osm.ps1 — скачивает .osm.pbf выгрузку Минска с bbbike.org.
#
# Запуск из корня проекта:
#     powershell -ExecutionPolicy Bypass -File .\scripts\download-osm.ps1
#
# Файл сохраняется в: data\minsk.osm.pbf (~70 МБ).
# =====================================================================
[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"
$ProjectDir = Split-Path -Parent $PSScriptRoot
$DataDir    = Join-Path $ProjectDir "data"
$OutFile    = Join-Path $DataDir "minsk.osm.pbf"

# bbbike.org предоставляет отдельные выгрузки по городам.
# Geofabrik не выделяет Минск отдельно (только вся Беларусь ~1.2 ГБ).
$Url = "https://download.bbbike.org/osm/bbbike/Minsk/Minsk.osm.pbf"

if (-not (Test-Path $DataDir)) {
    New-Item -ItemType Directory -Path $DataDir -Force | Out-Null
}

Write-Host "==> Скачиваю выгрузку OSM для Минска ..." -ForegroundColor Cyan
Write-Host "    Источник:   $Url"
Write-Host "    Файл:       $OutFile"
Write-Host ""

# Preferred: curl (есть в Windows 10/11 как curl.exe)
$curl = Get-Command curl.exe -ErrorAction SilentlyContinue
if ($curl) {
    & curl.exe -L --fail --retry 3 --continue-at - -o $OutFile $Url
    if ($LASTEXITCODE -ne 0) {
        Write-Error "curl завершился с ошибкой (exit code $LASTEXITCODE)"
        exit 1
    }
}
elseif (Get-Command Invoke-WebRequest -ErrorAction SilentlyContinue) {
    # Fallback на PowerShell-ный Invoke-WebRequest (медленнее, но работает)
    Invoke-WebRequest -Uri $Url -OutFile $OutFile -UseBasicParsing
}
else {
    Write-Error "Ни curl, ни Invoke-WebRequest не доступны."
    exit 1
}

$size = (Get-Item $OutFile).Length
Write-Host ""
Write-Host "==> Готово. Размер файла: $([math]::Round($size / 1MB, 2)) МБ" -ForegroundColor Green
Write-Host ""
Write-Host "Следующий шаг:"
Write-Host "  .\scripts\import-osm.ps1 -PbfFile $OutFile"
