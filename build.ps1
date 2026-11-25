<#
    Roost build script (Windows or Linux with the .NET 8 SDK).

    Examples:
      pwsh ./build.ps1                      # self-contained single exe -> dist/Roost.exe (no .NET needed to run)
      pwsh ./build.ps1 -FrameworkDependent  # small build -> dist-fd/ (needs .NET 8 Desktop Runtime)
#>
param([switch]$FrameworkDependent)

$ErrorActionPreference = "Stop"
$root = $PSScriptRoot
$proj = Join-Path $root "src/Roost/Roost.csproj"
$env:DOTNET_CLI_TELEMETRY_OPTOUT = "1"
$env:DOTNET_NOLOGO = "1"

if ($FrameworkDependent) {
    $out = Join-Path $root "dist-fd"
    dotnet publish $proj -c Release -r win-x64 --self-contained false `
        -p:PublishSingleFile=true -p:DebugType=none -o $out
} else {
    $out = Join-Path $root "dist"
    dotnet publish $proj -c Release -r win-x64 --self-contained true `
        -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:DebugType=none -o $out
}

Write-Host "Output in $out" -ForegroundColor Green
Get-ChildItem $out -Filter *.exe | Select-Object Name, Length | Format-Table | Out-String | Write-Host
