<#
.SYNOPSIS
  Build Redbox.KioskEngine.API.Parity for net48 (Web API self-host) and net10.0-windows (ASP.NET Core), run both and
  compare their answers. The net48 side runs Web API in memory (HttpServer as the HttpClient handler: same routes,
  formatters and controllers, no HTTP.sys listener, which would need a URL reservation); the .NET 10 side runs the
  real APIService on Kestrel at the given port.
#>
param([string] $Out = (Join-Path ([IO.Path]::GetTempPath()) "rb-api-parity"), [int] $Port = 19002)
$ErrorActionPreference = "Stop"
$msbuild = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -products * -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1
$proj = Join-Path $PSScriptRoot "Redbox.KioskEngine.API.Parity.csproj"
$results = @{}
# One build for both targets (setting TargetFramework globally would also force it on .NET Standard references).
& $msbuild $proj -restore -nologo -v:q -clp:ErrorsOnly -p:Configuration=Release "-p:ArtifactsPath=$(Join-Path $Out 'artifacts')" | Out-Host
if ($LASTEXITCODE) { throw "build failed" }
foreach ($tfm in "net48", "net10.0-windows") {
    $dir = Join-Path $Out "artifacts\bin\Redbox.KioskEngine.API.Parity\release_$tfm"
    $results[$tfm] = & (Join-Path $dir "Redbox.KioskEngine.API.Parity.exe") $Port
    Set-Content -Path (Join-Path $Out "$tfm.txt") -Value $results[$tfm] -Encoding UTF8
}
$diff = Compare-Object $results["net48"] $results["net10.0-windows"] -SyncWindow 0
$cases = @($results["net48"] | Where-Object { $_ -like "### *" }).Count
if ($diff) {
    $diff | Format-Table -AutoSize -Wrap | Out-String -Width 300 | Write-Host
    Write-Host "DIFFERENT ($cases requests; outputs in $Out)"
    exit 1
}
Write-Host "IDENTICAL ($cases requests; outputs in $Out)"
