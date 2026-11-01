# Rename-Member -Path <file> -Method <exact declaration text, may include leading tabs> -Map @{ old = 'new' } [-Occurrence n]
# Simultaneous whole-word renames inside one member, which may be nested at any depth: from its declaration line to the
# closing brace at the declaration's indentation. Refuses when a new name (that isn't itself being renamed away) is
# already used in the member, and refuses 'value' in a member with a setter.
param([Parameter(Mandatory)] [string] $Path, [Parameter(Mandatory)] [string] $Method,
      [Parameter(Mandatory)] [System.Collections.IDictionary] $Map, [int] $Occurrence = 1, [string[]] $AllowExisting = @(), [switch] $SkipMissing)
$full = (Resolve-Path $Path).Path
$bytes = [IO.File]::ReadAllBytes($full); $bom = $bytes.Length -ge 3 -and $bytes[0] -eq 0xEF
$text = [IO.File]::ReadAllText($full); $nl = if ($text.Contains("`r`n")) { "`r`n" } else { "`n" }; $text = $text.Replace("`r`n", "`n")
$start = -1
for ($n = 0; $n -lt $Occurrence; $n++) { $start = $text.IndexOf($Method, $start + 1); if ($start -lt 0) { throw "member not found: $Method" } }
$lineStart = $text.LastIndexOf("`n", $start) + 1
$indent = ([regex]::Match($text.Substring($lineStart), '^\t*')).Value
$close = [regex]::Match($text.Substring($start), "\n$indent\}(\n|$)")
if (-not $close.Success) { throw "end of member not found: $Method" }
$end = $start + $close.Index + $close.Length
$body = $text.Substring($start, $end - $start)
$Map = [ordered]@{} + $Map
foreach ($k in @($Map.Keys)) {
    if ($SkipMissing -and -not [regex]::IsMatch($body, "\b$k\b")) { $Map.Remove($k); continue }
    if ($k -eq 'value' -and [regex]::IsMatch($body, '\n\t+set\b')) { throw "value is the setter keyword in $Method" }
    if (-not [regex]::IsMatch($body, "\b$k\b")) { throw "$k not found in $Method" }
}
foreach ($v in @($Map.Values | Sort-Object -Unique)) {
    if (-not $Map.Contains($v) -and $AllowExisting -notcontains $v -and [regex]::IsMatch($body, "\b$v\b")) { throw "$v is already used in $Method" }
}
if ($Map.Count -eq 0) { "nothing to rename in $($Method.Trim())"; return }
$rx = '\b(' + ((@($Map.Keys) | Sort-Object Length -Descending | ForEach-Object { [regex]::Escape($_) }) -join '|') + ')\b'
$body = [regex]::Replace($body, $rx, { param($m) $Map[$m.Value] })
$text = $text.Substring(0, $start) + $body + $text.Substring($end)
[IO.File]::WriteAllText($full, $text.Replace("`n", $nl), (New-Object Text.UTF8Encoding($bom)))
"renamed $(@($Map.Keys).Count) in $($Method.Trim())"
