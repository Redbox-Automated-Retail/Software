# Check-Literals [-Commit <rev> | -Range <a..b> | (working tree)] [-Path src]
# A rename must not change any string. Compares the string literals on removed and added lines of a diff as
# multisets; interpolated strings ($"...") are compared with their {holes} blanked out, so renamed placeholders don't
# count but changed text does. Prints each differing literal; exit code 1 when anything differs.
param([string] $Commit, [string] $Range, [string[]] $Path = @('src'))
$diff = if ($Commit) { git show -U0 --format= $Commit -- @Path } elseif ($Range) { git diff -U0 $Range -- @Path } else { git diff -U0 -- @Path }
function Get-Literals([string] $line) {
    $out = @()
    # Interpolated (optionally verbatim) strings: blank each {hole}, keep the literal text.
    foreach ($m in [regex]::Matches($line, '\$@?"(?:[^"\\]|\\.)*"')) { $out += ([regex]::Replace($m.Value, '\{[^{}]*(\{[^{}]*\}[^{}]*)*\}', '{}')) }
    $rest = [regex]::Replace($line, '\$@?"(?:[^"\\]|\\.)*"', '')
    # Leave comments out (a rename may touch words in them).
    $rest = [regex]::Replace($rest, '//.*$', '')
    foreach ($m in [regex]::Matches($rest, '(?<![\$])@?"(?:[^"\\]|\\.)*"')) { $out += $m.Value }
    $out
}
$minus = New-Object System.Collections.Generic.List[string]; $plus = New-Object System.Collections.Generic.List[string]
foreach ($l in $diff) {
    if ($l -match '^-(?!--)') { foreach ($x in Get-Literals $l.Substring(1)) { $minus.Add($x) } }
    elseif ($l -match '^\+(?!\+\+)') { foreach ($x in Get-Literals $l.Substring(1)) { $plus.Add($x) } }
}
$counts = @{}
foreach ($x in $minus) { $counts[$x] = 1 + [int]$counts[$x] }
foreach ($x in $plus) { $counts[$x] = [int]$counts[$x] - 1 }
$bad = $counts.GetEnumerator() | Where-Object { $_.Value -ne 0 }
foreach ($b in $bad) { "{0} {1}" -f $(if ($b.Value -gt 0) { 'removed' } else { 'added  ' }), $b.Key }
if ($bad) { exit 1 }
