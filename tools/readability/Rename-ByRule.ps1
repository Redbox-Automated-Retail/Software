# Rename-ByRule -Files <paths> -Rules @( @{ Name='theme'; Pattern='ITheme (?<old>obj\d*) = .*CurrentTheme' }, ... ) [-WhatIf]
# For every member (method, accessor block, constructor, lambda-free scan by declaration line + closing brace at the
# same indentation) in each file: when a declaration matches a rule's Pattern (group 1 = the decompiler name), rename
# that name throughout the member to the rule's Name, unless the new name already occurs in the member. One rule per
# decompiler name per member; the first matching rule wins. Prints a count per file.
param([string[]] $Files, [object[]] $Rules, [switch] $WhatIf)
$total = 0
foreach ($f in $Files) {
    $bytes = [IO.File]::ReadAllBytes($f); $bom = $bytes.Length -ge 3 -and $bytes[0] -eq 0xEF
    $text = [IO.File]::ReadAllText($f); $nl = if ($text.Contains("`r`n")) { "`r`n" } else { "`n" }; $text = $text.Replace("`r`n", "`n")
    $lines = $text.Split("`n")
    $changed = 0
    # Member starts: a line indented by tabs that declares a method/ctor/property/accessor and is followed by "{" on the
    # next line at the same indentation.
    $i = 0
    $out = New-Object System.Collections.Generic.List[string]
    while ($i -lt $lines.Length) {
        $line = $lines[$i]
        $m = [regex]::Match($line, '^(\t+)(public|private|protected|internal|static|override|virtual|async|get|set|add|remove)\b')
        if ($m.Success -and $line -notmatch '\b(class|struct|interface|enum|record|namespace)\b' -and $i + 1 -lt $lines.Length -and $lines[$i + 1] -eq "$($m.Groups[1].Value){") {
            $indent = $m.Groups[1].Value
            $j = $i + 2
            while ($j -lt $lines.Length -and $lines[$j] -ne "$indent}") { $j++ }
            if ($j -ge $lines.Length) { $out.Add($line); $i++; continue }
            $body = ($lines[$i..$j]) -join "`n"
            $map = @{}
            foreach ($r in $Rules) {
                foreach ($dm in [regex]::Matches($body, $r.Pattern)) {
                    $old = $dm.Groups['old'].Value
                    $new = $r.Name
                    # {n}: camelCase of the "n" group (a leading interface I dropped), e.g. IKernelService -> kernelService.
                    if ($new.Contains('{p}')) { $n = $dm.Groups['n'].Value -creplace '^I(?=[A-Z])', ''; $n = $n.Substring(0, 1).ToLower() + $n.Substring(1); $n = if ($n -cmatch '(Data|Details|Info|Information|Metadata|Status|Settings|Criteria|For\w+)$') { $n + 'List' } elseif ($n -match '(s|x|ch|sh)$') { $n + 'es' } elseif ($n -match '[^aeiou]y$') { $n.Substring(0, $n.Length - 1) + 'ies' } else { $n + 's' }; $new = $new.Replace('{p}', $n) }
                    if ($new.Contains('{n}')) { $n = $dm.Groups['n'].Value -creplace '^I(?=[A-Z])', ''; $new = $new.Replace('{n}', $n.Substring(0, 1).ToLower() + $n.Substring(1)) }
                    if ($new -eq $old -or $map.ContainsKey($old) -or $map.Values -contains $new) { continue }
                    if ([regex]::IsMatch($body, "\b$([regex]::Escape($new))\b")) { continue }
                    # Only rename a name declared once in the member (a reused decompiler name may mean two things).
                    if ([regex]::Matches($body, "(?:[\w>\]]\s+|out var\s+)$old\s*(?:=|;|\))").Count -ne 1) { continue }
                    # Leave it when the name also appears inside a string literal.
                    if ([regex]::Matches($body, '"(?:[^"\\\n]|\\.)*"') | Where-Object { $_.Value -match "\b$old\b" }) { continue }
                    $map[$old] = $new
                }
            }
            if ($map.Count -gt 0) {
                $rx = '\b(' + ((@($map.Keys) | Sort-Object Length -Descending | ForEach-Object { [regex]::Escape($_) }) -join '|') + ')\b'
                $body = [regex]::Replace($body, $rx, { param($x) $map[$x.Value] })
                $changed += $map.Count
            }
            foreach ($b in $body.Split("`n")) { $out.Add($b) }
            $i = $j + 1
            continue
        }
        $out.Add($line); $i++
    }
    if ($changed -gt 0) {
        $total += $changed
        if (-not $WhatIf) { [IO.File]::WriteAllText($f, ($out -join "`n").Replace("`n", $nl), (New-Object Text.UTF8Encoding($bom))) }
        "$changed  $($f.Substring($f.IndexOf('\src\') + 5))"
    }
}
"total: $total"
