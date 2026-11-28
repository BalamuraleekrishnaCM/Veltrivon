#Requires -Version 7.0
[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$root = Split-Path $PSScriptRoot
$tag = (Get-Content -LiteralPath "$PSScriptRoot/version.txt" -Raw).Trim()
if ($tag -notmatch '^v(\d+\.\d+\.\d+)-preview\.\d+$') {
    throw 'Use a preview release tag such as v0.1.0-preview.1.'
}
$version = $tag.Substring(1)
$packageRoot = Join-Path $root "artifacts/release/$tag"
$assets = Join-Path $packageRoot 'assets'
$bundle = Join-Path $packageRoot 'bundle'
$xr = Join-Path $bundle 'XR'
$tools = Join-Path $bundle 'Tools'
$batch = Join-Path $packageRoot 'batch'
if (Test-Path -LiteralPath $packageRoot) {
    throw "Output already exists at $packageRoot. Use a fresh checkout or move that output aside."
}
$null = New-Item -ItemType Directory -Path $assets,$bundle,$xr,$tools,$batch -Force

function Invoke-Checked {
    param([string]$Executable, [string[]]$Arguments)
    & $Executable @Arguments
    if ($LASTEXITCODE -ne 0) { throw "$Executable exited with code $LASTEXITCODE." }
}

Invoke-Checked dotnet @('publish', "$root/src/Veltrivon.App/Veltrivon.App.csproj", '-c', 'Release', '-r', 'win-x64',
    '--self-contained', 'true', '-o', $bundle, "-p:Version=$version")
Invoke-Checked dotnet @('publish', "$root/xr-dev-manager/XrDevManager.csproj", '-c', 'Release', '-r', 'win-x64',
    '--self-contained', 'true', '-o', $xr, "-p:Version=$version", '-p:PublishSingleFile=true',
    '-p:IncludeNativeLibrariesForSelfExtract=true', '-p:DebugType=embedded')
Copy-Item -LiteralPath "$root/xr-dev-manager/LICENSE","$root/xr-dev-manager/README.md" -Destination $xr
Copy-Item -LiteralPath "$root/README.md" -Destination $bundle
$xrExe = Join-Path $xr 'xr-dev-manager.exe'
Invoke-Checked $xrExe @('--help')
Copy-Item -LiteralPath $xrExe -Destination (Join-Path $assets 'XR-Dev-Manager-win-x64.exe')

$toolExe = Join-Path $tools 'Veltrivon-Tools-win-x64.exe'
Push-Location "$root/installer"
try {
    Invoke-Checked go @('test', './...')
    Invoke-Checked go @('build', '-trimpath', '-o', $toolExe, '.')
} finally {
    Pop-Location
}
'7' | & $toolExe
if ($LASTEXITCODE -ne 0) { throw 'Tool installer exit test failed.' }
Copy-Item -LiteralPath $toolExe -Destination $assets
Copy-Item -LiteralPath "$root/installer/Veltrivon-Install.bat","$root/installer/catalog.json" -Destination $batch
Compress-Archive -Path "$batch/*" -DestinationPath "$assets/Veltrivon-Batch-Installer.zip"
Compress-Archive -Path "$bundle/*" -DestinationPath "$assets/Veltrivon-Portable-win-x64.zip"

$compiler = Get-Command ISCC.exe -CommandType Application -ErrorAction SilentlyContinue | Select-Object -First 1
$compilerPath = if ($compiler) { $compiler.Source } else {
    Join-Path ${env:ProgramFiles(x86)} 'Inno Setup 6/ISCC.exe'
}
if (!(Test-Path -LiteralPath $compilerPath)) { throw 'Inno Setup 6 compiler is required.' }
Invoke-Checked $compilerPath @("/DAppVersion=$version", "/DBundleDir=$bundle", "/DAssetDir=$assets", "$PSScriptRoot/Veltrivon.iss")

$smokePath = Join-Path $packageRoot 'installed-test'
$setup = Join-Path $assets 'Veltrivon-Setup-win-x64.exe'
$setupArgs = '/VERYSILENT /SUPPRESSMSGBOXES /NORESTART /NOICONS /DIR="' + $smokePath + '"'
$installer = Start-Process -FilePath $setup -ArgumentList $setupArgs -PassThru -Wait
if ($installer.ExitCode -ne 0 -or !(Test-Path -LiteralPath (Join-Path $smokePath 'Veltrivon.App.exe'))) {
    throw 'Setup installation test failed.'
}
Invoke-Checked (Join-Path $smokePath 'XR/xr-dev-manager.exe') @('--help')
$uninstaller = Start-Process -FilePath (Join-Path $smokePath 'unins000.exe') -ArgumentList '/VERYSILENT /SUPPRESSMSGBOXES /NORESTART' -PassThru -Wait
if ($uninstaller.ExitCode -ne 0 -or (Test-Path -LiteralPath (Join-Path $smokePath 'Veltrivon.App.exe'))) {
    throw 'Setup uninstall test failed.'
}

$hashes = Get-ChildItem -LiteralPath $assets -File | Sort-Object Name | ForEach-Object {
    $hash = (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
    "$hash  $($_.Name)"
}
$hashes | Set-Content -LiteralPath "$assets/SHA256SUMS.txt" -Encoding ascii
