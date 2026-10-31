<#
.SYNOPSIS
  Build Redbox.Core.JsonParity for net48 and net10.0-windows, run both and compare their output.
.PARAMETER Out
  Build output folder (default: a temp folder). Exit code 0 when both outputs are identical.
#>
param([string] $Out = (Join-Path ([IO.Path]::GetTempPath()) "rb-json-parity"))
$ErrorActionPreference = "Stop"
$msbuild = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -products * -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1
$proj = Join-Path $PSScriptRoot "Redbox.Core.JsonParity.csproj"
$results = @{}
foreach ($tfm in "net48", "net10.0-windows") {
    $dir = Join-Path $Out $tfm
    & $msbuild $proj -restore -nologo -v:q -clp:ErrorsOnly -p:Configuration=Release "-p:TargetFramework=$tfm" "-p:OutDir=$dir\" | Out-Host
    if ($LASTEXITCODE) { throw "build for $tfm failed" }
    $results[$tfm] = & (Join-Path $dir "Redbox.Core.JsonParity.exe")
    Set-Content -Path (Join-Path $Out "$tfm.txt") -Value $results[$tfm] -Encoding UTF8
}
$diff = Compare-Object $results["net48"] $results["net10.0-windows"] -SyncWindow 0
$cases = @($results["net48"] | Where-Object { $_ -like "### *" }).Count
if ($diff) {
    $diff | Format-Table -AutoSize -Wrap | Out-String -Width 300 | Write-Host
    Write-Host "DIFFERENT ($cases cases; outputs in $Out)"
    exit 1
}
Write-Host "IDENTICAL ($cases cases; outputs in $Out)"
