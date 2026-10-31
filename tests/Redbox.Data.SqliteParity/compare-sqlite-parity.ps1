<#
.SYNOPSIS
  Build Redbox.Data.SqliteParity for net48 (VistaDB) and net10.0-windows (SQLite), run both and compare their output.
.PARAMETER Samples
  Optional folder with copies of real kiosk VistaDB files (Local.data, cache.data, profile.data, message.data,
  HALData.vdb3, HALCounters.vdb3) for the conversion checks. Never point it at a kiosk's live folders.
.PARAMETER Out
  Build output and work folder (default: a temp folder). Exit code 0 when both outputs match.
#>
param([string] $Samples, [string] $Out = (Join-Path ([IO.Path]::GetTempPath()) "rb-sqlite-parity"))
$ErrorActionPreference = "Stop"
$msbuild = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -products * -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1
$root = Resolve-Path (Join-Path $PSScriptRoot "..\..")
$tool = Join-Path $Out "tool"
& $msbuild (Join-Path $root "src\Tools\vistadb-to-sqlite\vistadb-to-sqlite.csproj") -restore -nologo -v:q -clp:ErrorsOnly -p:Configuration=Release "-p:OutDir=$tool\" | Out-Host
if ($LASTEXITCODE) { throw "build of vistadb-to-sqlite failed" }
$env:REDBOX_VISTADB_TO_SQLITE = Join-Path $tool "vistadb-to-sqlite.exe"
$proj = Join-Path $PSScriptRoot "Redbox.Data.SqliteParity.csproj"
# Both targets in one build (a global TargetFramework would also apply to the netstandard2.0 references).
$artifacts = Join-Path $Out "artifacts"
& $msbuild $proj -restore -nologo -v:q -clp:ErrorsOnly -p:Configuration=Release "-p:ArtifactsPath=$artifacts" | Out-Host
if ($LASTEXITCODE) { throw "build failed" }
$results = @{}
foreach ($tfm in "net48", "net10.0-windows") {
    $dir = Join-Path $artifacts "bin\Redbox.Data.SqliteParity\release_$tfm"
    $runArgs = @((Join-Path $Out "work-$tfm"))
    if ($Samples) { $runArgs += (Resolve-Path $Samples).Path }
    $results[$tfm] = & (Join-Path $dir "Redbox.Data.SqliteParity.exe") @runArgs
    Set-Content -Path (Join-Path $Out "$tfm.txt") -Value $results[$tfm] -Encoding UTF8
}
$a = @($results["net48"] | Where-Object { $_ -notlike "#*" })
$b = @($results["net10.0-windows"] | Where-Object { $_ -notlike "#*" })
$results["net10.0-windows"] | Where-Object { $_ -like "#*" } | Write-Host
$diff = Compare-Object $a $b -SyncWindow 0
$checks = @($a | Where-Object { $_ -notlike "===*" }).Count
if ($diff) {
    $diff | Format-Table -AutoSize -Wrap | Out-String -Width 300 | Write-Host
    Write-Host "DIFFERENT ($checks results; outputs in $Out)"
    exit 1
}
Write-Host "IDENTICAL ($checks results; outputs in $Out)"
