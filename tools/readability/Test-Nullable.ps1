# Test-Nullable <project name or .csproj> [...] [-Enable]
# Builds each project with nullable reference types on (its own ArtifactsPath, so it never touches the main build)
# and prints the distinct nullable warnings (CS86xx/CS87xx) of that project only. With -Enable, adds
# <Nullable>enable</Nullable> to the project's first PropertyGroup when it has no warnings left.
param([Parameter(Mandatory)] [string[]] $Project, [switch] $Enable, [string] $Artifacts = "E:\temp\rb-modernize\a-nullable")
$msbuild = "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe"
$root = Resolve-Path (Join-Path $PSScriptRoot "..\..")
foreach ($p in $Project) {
    $proj = if ($p -like "*.csproj") { Get-Item $p } else { Get-ChildItem (Join-Path $root "src"), (Join-Path $root "tests") -Recurse -Filter "$p.csproj" | Select-Object -First 1 }
    if (-not $proj) { "not found: $p"; continue }
    $out = & $msbuild $proj.FullName -restore -t:Rebuild -nologo -v:q -p:Configuration=Release -p:Nullable=enable "-p:ArtifactsPath=$Artifacts" 2>&1
    $errors = @($out | Select-String ": error ")
    $warnings = @($out | ForEach-Object { "$_" } | Where-Object { $_ -match "warning CS8[67]\d\d" -and $_.Contains("[" + $proj.FullName) } | ForEach-Object { ($_ -replace [regex]::Escape("$root\"), "") -replace " \[.*$", "" } | Sort-Object -Unique)
    "== {0}: {1} nullable warning(s), {2} error(s)" -f $proj.BaseName, $warnings.Count, $errors.Count
    $errors | Select-Object -First 5 | ForEach-Object Line
    $warnings
    if ($Enable -and $warnings.Count -eq 0 -and $errors.Count -eq 0) {
        $text = [IO.File]::ReadAllText($proj.FullName)
        if ($text -notmatch "<Nullable>") {
            $text = [regex]::Replace($text, "(<PropertyGroup>(\r?\n)([ \t]*))", '$1<Nullable>enable</Nullable>$2$3', 1)
            [IO.File]::WriteAllText($proj.FullName, $text)
            "   enabled in $($proj.Name)"
        }
    }
}
