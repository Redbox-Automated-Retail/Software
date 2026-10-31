<#
.SYNOPSIS
  Build one deployable component (build/components.json) and package it as a release zip for the backend.
.DESCRIPTION
  Builds the component's projects with MSBuild (Release, deterministic, the release version stamped on its main
  assemblies by build/Release.targets), stages the output as the kiosk lays it out, and writes
  <Out>\<component>-<version>.zip:

    release.json   Format, Component, Version, Kind, Repository/TargetRoot or FileSet/Destination, Source
                   (git commit, ref, dirty), Built, Files {relative path: SHA-1}
    files\...      the staged output, relative to TargetRoot (repository) or Destination (file set)

  Publish it with the backend (docs/releases.md there): `admin release-publish <zip>` or
  `scripts/rb-release.py publish <zip>`. Publishing does not serve it to any kiosk.

  Needs Visual Studio 2022+ or Build Tools (MSBuild with the .NET desktop workload), .NET Framework 3.5 for HAL,
  and the .NET SDK; NuGet packages are restored from nuget.org.
.PARAMETER Component
  A key of build/components.json (kioskclient, updateclient, device-service, kop-update-manager, kop-v2-engine,
  kop-hal-install, kop-ms-hal-tester, kop-tools, kioskclient-fileset).
.PARAMETER Version
  Numeric a.b.c.d. Or give -Tag.
.PARAMETER Tag
  A git tag <component>/v<version> (e.g. kioskclient/v1.0.208.0): sets Component and Version.
.EXAMPLE
  .\build\release.ps1 -Component kioskclient -Version 1.0.208.0
  .\build\release.ps1 -Tag kioskclient/v1.0.208.0 -Out E:\temp\releases
#>
param(
    [string] $Component,
    [string] $Version,
    [string] $Tag,
    [string] $Out,
    [string] $Configuration = "Release",
    [string] $RestoreSources,          # NuGet sources instead of nuget.org (e.g. a local folder feed); ";"-separated
    [string] $Map = (Join-Path $PSScriptRoot "components.json"),
    # Extra MSBuild properties, Name=Value. kop-v2-engine needs RedboxEngine181Refs=<folder> (the engine 1.81.0.27
    # assemblies that Redbox.KioskServices.* build against; src/KioskServices/Engine181.targets).
    [string[]] $Property = @(),
    [switch] $AllowDirty
)
$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
if (-not $Out) { $Out = Join-Path $root "out\release" }

if ($Tag) {
    if ($Tag -notmatch '^(?:refs/tags/)?([a-z0-9-]+)/v(\d+\.\d+\.\d+\.\d+)$') { throw "tag '$Tag' is not <component>/v<a.b.c.d>" }
    $Component, $Version = $Matches[1], $Matches[2]
}
if (-not $Component -or -not $Version) { throw "give -Component and -Version, or -Tag" }
if ($Version -notmatch '^\d+\.\d+\.\d+\.\d+$' -or ($Version.Split('.') | Where-Object { [int64]$_ -gt 65534 })) {
    throw "version '$Version' must be a.b.c.d with parts up to 65534 (it becomes the file version)"
}
$catalog = Get-Content -Raw $Map | ConvertFrom-Json
$comp = $catalog.components.$Component
if (-not $comp) {
    $why = $catalog.notBuiltHere.$Component
    if ($why) { throw "$Component is not built from this repository: $why" }
    throw "unknown component '$Component'; see build/components.json"
}

function Invoke-Git { $r = & git.exe -C $root @args; if ($LASTEXITCODE) { throw "git $args failed" }; $r }
$commit = Invoke-Git rev-parse HEAD
$dirty = [bool](Invoke-Git status --porcelain --untracked-files=no)
if ($dirty -and -not $AllowDirty) { throw "the working tree has uncommitted changes (commit them, or -AllowDirty for a test build)" }
$ref = $Tag
if (-not $ref) {   # PowerShell 5.1 turns git's stderr into a terminating error under "Stop"
    $ErrorActionPreference = "Continue"
    $ref = & git.exe -C $root describe --tags --exact-match 2>$null
    $ErrorActionPreference = "Stop"
    $ref = if ($ref) { [string]$ref } else { $null }
}

$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
$msbuild = & $vswhere -latest -prerelease -products * -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1
if (-not $msbuild) { throw "MSBuild not found (install Visual Studio Build Tools with the .NET desktop workload)" }

$work = Join-Path $Out "$Component-$Version"
$stage = Join-Path $work "stage"
Remove-Item $work -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force $stage | Out-Null

$common = @("-nologo", "-v:minimal", "-restore", "-p:Configuration=$Configuration", "-p:ContinuousIntegrationBuild=true",
            "-p:RedboxReleaseVersion=$Version", "-p:RedboxReleaseStamp=$(@($comp.stamp) -join '|')",
            "-p:RedboxReleaseCommit=$($commit.Substring(0, 12))")
if ($RestoreSources) { $common += "-p:RestoreSources=$($RestoreSources.Replace(';', '%3B'))" }
foreach ($prop in $Property) { $common += "-p:$prop" }
foreach ($p in $comp.projects) {
    $proj = Join-Path $root $p.path
    $dest = if ($p.to) { Join-Path $stage $p.to } else { $stage }
    New-Item -ItemType Directory -Force $dest | Out-Null
    Write-Host "== $($p.path) -> $(if ($p.to) { $p.to } else { '.' })"
    if ($p.publish) {
        # As shipped: framework-dependent, win7-x86, with the apphost .exe. No web.config: the kiosk keeps its own
        # (the SDK's would drop hostingModel="InProcess").
        & $msbuild $proj -t:Publish @common "-p:RuntimeIdentifier=win7-x86" "-p:SelfContained=false" "-p:PublishDir=$dest\" `
            "-p:IsTransformWebConfigDisabled=true"
    } else {
        $extra = @()   # not "$extra = if ...": PowerShell unwraps a one-item array, and @extra would splat its characters
        if ($p.merged) { $extra += "-p:ILRepack=true" }
        & $msbuild $proj @common @extra "-p:OutDir=$dest\"
    }
    if ($LASTEXITCODE) { throw "build of $($p.path) failed" }
    if ($p.merged) {
        $merged = Join-Path $dest "merged"
        if (-not (Test-Path $merged)) { throw "$($p.path): no merged output (ILRepack)" }
        Copy-Item (Join-Path $merged "*") $dest -Force
        Remove-Item $merged -Recurse -Force
    }
}

# What the kiosk does not need.
Get-ChildItem $stage -Recurse -File -Include *.pdb | Remove-Item
$files = Get-ChildItem $stage -Recurse -File | Sort-Object FullName
if ($comp.include) {
    $files = $files | Where-Object { $comp.include -contains $_.Name }
    if (@($files).Count -ne @($comp.include).Count) { throw "not every file in 'include' was built: $($comp.include -join ', ')" }
}
$manifest = [ordered]@{}
foreach ($f in $files) {
    $rel = $f.FullName.Substring($stage.Length + 1).Replace('\', '/')
    $manifest[$rel] = (Get-FileHash $f.FullName -Algorithm SHA1).Hash
}
$meta = [ordered]@{
    Format = 1; Component = $Component; Version = $Version; Kind = $comp.kind
    Source = [ordered]@{ Repository = "Redbox-Automated-Retail/Software"; Commit = $commit; Ref = $ref; Dirty = $dirty }
    Built = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ"); Files = $manifest
}
if ($comp.kind -eq "fileset") {
    $meta.FileSet = $comp.fileSet; $meta.Destination = $comp.destination
} else {
    $meta.Repository = $comp.repository; $meta.TargetRoot = $comp.targetRoot
    if ($comp.newFiles) { $meta.NewFiles = @($comp.newFiles) }
}

# Entries sorted, forward slashes, fixed timestamps: the zip depends only on the build output.
Add-Type -AssemblyName System.IO.Compression, System.IO.Compression.FileSystem
$zipPath = Join-Path $Out "$Component-$Version.zip"
Remove-Item $zipPath -ErrorAction SilentlyContinue
$zip = [IO.Compression.ZipFile]::Open($zipPath, [IO.Compression.ZipArchiveMode]::Create)
try {
    $stamp = [DateTimeOffset]::new(2020, 1, 1, 0, 0, 0, [TimeSpan]::Zero)
    $e = $zip.CreateEntry("release.json"); $e.LastWriteTime = $stamp
    $w = New-Object IO.StreamWriter($e.Open(), (New-Object Text.UTF8Encoding($false)))
    $w.Write(($meta | ConvertTo-Json -Depth 6)); $w.Dispose()
    foreach ($f in $files) {
        $rel = $f.FullName.Substring($stage.Length + 1).Replace('\', '/')
        $e = $zip.CreateEntry("files/$rel", [IO.Compression.CompressionLevel]::Optimal); $e.LastWriteTime = $stamp
        $s = $e.Open(); $in = [IO.File]::OpenRead($f.FullName)
        try { $in.CopyTo($s) } finally { $in.Dispose(); $s.Dispose() }
    }
} finally { $zip.Dispose() }
Write-Host "$zipPath ($(@($files).Count) files, $Component $Version from $($commit.Substring(0, 12))$(if ($dirty) { ', DIRTY' }))"
$zipPath
