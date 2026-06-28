param(
    [string]$Version = "1.3",
    [string]$MpvSource = "$env:USERPROFILE\Desktop\mpv-omniphony-v0.4.1-2-windows-x86_64",
    [string]$StudioSource = "C:\Program Files\Omniphony Studio"
)

$ErrorActionPreference = "Stop"
$root = Split-Path $PSScriptRoot -Parent
$dotnet = Join-Path $root "work\.dotnet\dotnet.exe"
$project = Join-Path $root "src\OmniphonyLauncher\OmniphonyLauncher.csproj"
$publish = Join-Path $root "work\portable-publish-$Version"
$package = Join-Path $root "outputs\OmniPlayer-Portable-v$Version"
$zip = "$package.zip"

if (!(Test-Path -LiteralPath $dotnet)) { throw "Local .NET SDK not found: $dotnet" }
if (!(Test-Path -LiteralPath (Join-Path $MpvSource "mpv.exe"))) { throw "mpv runtime not found: $MpvSource" }
if (!(Test-Path -LiteralPath (Join-Path $StudioSource "omniphony-studio.exe"))) { throw "Studio runtime not found: $StudioSource" }
if ((Test-Path -LiteralPath $package) -or (Test-Path -LiteralPath $zip)) { throw "Output already exists; choose a new version or remove the generated output: $package" }

& $dotnet publish $project -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=false -o $publish
if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed" }

New-Item -ItemType Directory -Path $package | Out-Null
New-Item -ItemType Directory -Path (Join-Path $package "runtime\mpv") | Out-Null
New-Item -ItemType Directory -Path (Join-Path $package "runtime\studio") | Out-Null
New-Item -ItemType Directory -Path (Join-Path $package "legal") | Out-Null

Copy-Item -LiteralPath (Join-Path $publish "OmniPlayer.exe") -Destination (Join-Path $package "OmniPlayer.exe")
Get-ChildItem -LiteralPath $MpvSource -File | Where-Object { $_.Name -notlike "mpv-shot*" } | Copy-Item -Destination (Join-Path $package "runtime\mpv")
Copy-Item -LiteralPath (Join-Path $StudioSource "omniphony-studio.exe") -Destination (Join-Path $package "runtime\studio\omniphony-studio.exe")
Copy-Item -LiteralPath (Join-Path $StudioSource "orender.exe") -Destination (Join-Path $package "runtime\studio\orender.exe")
Copy-Item -LiteralPath (Join-Path $StudioSource "harletty_bridge.dll") -Destination (Join-Path $package "runtime\studio\harletty_bridge.dll")
if (Test-Path -LiteralPath (Join-Path $StudioSource "_up_")) { Copy-Item -LiteralPath (Join-Path $StudioSource "_up_") -Destination (Join-Path $package "runtime\studio\_up_") -Recurse }

Copy-Item -Path (Join-Path $root "legal\*") -Destination (Join-Path $package "legal")
Copy-Item -LiteralPath (Join-Path $root "docs\便携版使用说明.md") -Destination (Join-Path $package "使用说明.md")

Compress-Archive -LiteralPath $package -DestinationPath $zip -CompressionLevel Optimal
Write-Output "Portable package: $package"
Write-Output "Archive: $zip"
