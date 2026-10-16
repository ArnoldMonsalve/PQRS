<#
  Construye el instalador de PQRS:
    1. Publica la aplicación autocontenida (incluye .NET, no hay que instalarlo en los equipos).
    2. Compila installer\PQRS.iss con Inno Setup.
  El instalador queda en installer\Output\PQRS-Setup-<versión>.exe

  Uso:   powershell -ExecutionPolicy Bypass -File installer\build-installer.ps1
  La versión se toma de <Version> en src\PQRS.Desktop\PQRS.Desktop.csproj
#>
$ErrorActionPreference = 'Stop'

$raiz    = Split-Path $PSScriptRoot -Parent
$proyecto = Join-Path $raiz 'src\PQRS.Desktop\PQRS.Desktop.csproj'
$publish = Join-Path $PSScriptRoot 'publish'

# Versión (del .csproj)
$version = ([xml](Get-Content $proyecto)).Project.PropertyGroup.Version | Where-Object { $_ } | Select-Object -First 1
if (-not $version) { $version = '1.0.0' }
Write-Host "Versión: $version"

# 1. Publicar
if (Test-Path $publish) { Remove-Item $publish -Recurse -Force }
Write-Host 'Publicando la aplicación...'
dotnet publish $proyecto -c Release -r win-x64 --self-contained true -o $publish
if ($LASTEXITCODE -ne 0) { throw 'Falló dotnet publish.' }

# 2. Localizar Inno Setup
$candidatos = @(
    "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
    "$env:ProgramFiles\Inno Setup 6\ISCC.exe",
    "$env:LOCALAPPDATA\Programs\Inno Setup 6\ISCC.exe"
)
$iscc = $candidatos | Where-Object { Test-Path $_ } | Select-Object -First 1
if (-not $iscc) {
    $cmd = Get-Command ISCC.exe -ErrorAction SilentlyContinue
    if ($cmd) { $iscc = $cmd.Source }
}
if (-not $iscc) { throw 'No se encontró Inno Setup 6. Instálelo (winget install JRSoftware.InnoSetup) y vuelva a ejecutar.' }

# 3. Compilar el instalador
Write-Host 'Compilando el instalador...'
& $iscc "/DAppVersion=$version" (Join-Path $PSScriptRoot 'PQRS.iss')
if ($LASTEXITCODE -ne 0) { throw 'Falló la compilación del instalador.' }

Write-Host ''
Write-Host "Listo: $(Join-Path $PSScriptRoot "Output\PQRS-Setup-$version.exe")"
