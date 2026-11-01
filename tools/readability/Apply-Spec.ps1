# Apply-Spec -Files <paths> -Spec @{ '<declaration text>' = @{ old = 'new' } }
# Runs Rename-Member for every declaration found in every file (declarations are matched after a tab, so nesting
# depth doesn't matter); prints one line per attempt.
param([string[]] $Files, [System.Collections.IDictionary] $Spec, [string[]] $AllowExisting = @())
$rename = Join-Path $PSScriptRoot 'Rename-Member.ps1'
foreach ($f in $Files) {
    $short = $f.Substring($f.IndexOf('\src\') + 5)
    foreach ($d in $Spec.Keys) {
        if (-not ([IO.File]::ReadAllText($f)).Contains("`t$d")) { continue }
        $name = ($d -split '\(')[0] -split ' ' | Select-Object -Last 1
        try { $null = & $rename -Path $f -Method "`t$d" -Map $Spec[$d] -AllowExisting $AllowExisting -SkipMissing; "ok    $name  $short" }
        catch { "skip  $name  $short  ($($_.Exception.Message))" }
    }
}
