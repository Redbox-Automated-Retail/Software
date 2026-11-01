# Rename-InMethod -Path <file> -Method <text of the member's declaration line> -Map @{ old = 'new'; ... }
# Whole-word renames inside one member (its declaration line up to the next member at one tab of indentation).
param([Parameter(Mandatory)] [string] $Path, [Parameter(Mandatory)] [string] $Method, [Parameter(Mandatory)] [System.Collections.IDictionary] $Map)
$full = (Resolve-Path $Path).Path
$bytes = [IO.File]::ReadAllBytes($full); $bom = $bytes.Length -ge 3 -and $bytes[0] -eq 0xEF
$text = [IO.File]::ReadAllText($full); $nl = if ($text.Contains("`r`n")) { "`r`n" } else { "`n" }; $text = $text.Replace("`r`n", "`n")
$start = $text.IndexOf($Method); if ($start -lt 0) { throw "member not found: $Method" }
if ($text.IndexOf($Method, $start + 1) -ge 0) { throw "member text is not unique: $Method" }
$m = [regex]::Match($text.Substring($start + $Method.Length), '\n\t(public|private|protected|internal|static|\[|#|\})'); $end = if ($m.Success) { $start + $Method.Length + $m.Index } else { $text.Length }
$body = $text.Substring($start, $end - $start)
foreach ($k in $Map.Keys) {
    if ([regex]::IsMatch($body, "\b$($Map[$k])\b")) { throw "$($Map[$k]) is already used in $Method" }
    if (-not [regex]::IsMatch($body, "\b$k\b")) { throw "$k not found in $Method" }
}
foreach ($k in $Map.Keys) { $body = [regex]::Replace($body, "\b$k\b", $Map[$k]) }
$text = $text.Substring(0, $start) + $body + $text.Substring($end)
[IO.File]::WriteAllText($full, $text.Replace("`n", $nl), (New-Object Text.UTF8Encoding($bom)))
"renamed $($Map.Count) in $($Method.Trim())"
