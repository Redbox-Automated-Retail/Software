# Test-Nullable <project name or .csproj> [...] [-Enable]
# Turns nullable reference types on in each project (only there: references that aren't annotated yet stay
# oblivious, as in the real build), rebuilds it in its own artifacts folder and prints that project's distinct
# nullable warnings (CS86xx/CS87xx). The project keeps <Nullable>enable</Nullable> only with -Enable and no warnings
# left; otherwise its .csproj is put back as it was.
param([Parameter(Mandatory)] [string[]] $Project, [switch] $Enable, [string] $Artifacts = "E:\temp\rb-modernize\a-nullable",
    [string[]] $Property = @())   # extra MSBuild properties, Name=Value (KioskServices: RedboxEngine181Refs=<folder>)
$msbuild = "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe"
$root = Resolve-Path (Join-Path $PSScriptRoot "..\..")
foreach ($p in $Project) {
    $proj = if ($p -like "*.csproj") { Get-Item $p } else { Get-ChildItem (Join-Path $root "src"), (Join-Path $root "tests") -Recurse -Filter "$p.csproj" | Select-Object -First 1 }
    if (-not $proj) { "not found: $p"; continue }
    $original = [IO.File]::ReadAllText($proj.FullName)
    if ($original -notmatch "<Nullable>enable</Nullable>") {
        [IO.File]::WriteAllText($proj.FullName, [regex]::Replace($original, "(<PropertyGroup>(\r?\n)([ \t]*))", '$1<Nullable>enable</Nullable>$2$3', 1))
    }
    $out = & $msbuild $proj.FullName -restore -t:Rebuild -nologo -v:q -p:Configuration=Release "-p:ArtifactsPath=$Artifacts" @($Property | ForEach-Object { "-p:$_" }) 2>&1 | ForEach-Object { "$_" }
    $errors = @($out | Where-Object { $_ -match ": error " })
    $warnings = @($out | Where-Object { $_ -match "warning CS8[67]\d\d" -and $_.Contains("[" + $proj.FullName) } | ForEach-Object { ($_ -replace [regex]::Escape("$root\"), "") -replace " \[.*$", "" } | Sort-Object -Unique)
    "== {0}: {1} nullable warning(s), {2} error(s)" -f $proj.BaseName, $warnings.Count, $errors.Count
    $errors | Select-Object -First 5
    $warnings
    if ($Enable -and $warnings.Count -eq 0 -and $errors.Count -eq 0) {
        if ($original -notmatch "<Nullable>enable</Nullable>") { "   enabled in $($proj.Name)" }
    } else {
        [IO.File]::WriteAllText($proj.FullName, $original)
    }
}
