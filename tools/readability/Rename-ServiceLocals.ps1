# Rename-ServiceLocals -Path <file>: "IFooService serviceN = ServiceLocator.Instance.GetService<IFooService>();" -> fooService,
# within the rest of the enclosing method (up to the next member at one tab of indentation). Skips a name already used there.
param([Parameter(Mandatory)] [string] $Path)
$full = (Resolve-Path $Path).Path
$bytes = [IO.File]::ReadAllBytes($full); $bom = $bytes.Length -ge 3 -and $bytes[0] -eq 0xEF
$text = [IO.File]::ReadAllText($full); $nl = if ($text.Contains("`r`n")) { "`r`n" } else { "`n" }; $text = $text.Replace("`r`n", "`n")
$changed = 0
while ($true) {
    $m = [regex]::Match($text, '\b(I(\w+?)Service)\s+(service\d+)\s*=\s*ServiceLocator\.Instance\.GetService<\1>\(\)')
    $done = $true
    foreach ($m in [regex]::Matches($text, '\b(I(\w+?)Service)\s+(service\d+)\s*=\s*ServiceLocator\.Instance\.GetService<\1>\(\)')) {
        $old = $m.Groups[3].Value; $new = $m.Groups[2].Value.Substring(0,1).ToLower() + $m.Groups[2].Value.Substring(1) + "Service"
        $start = $m.Index
        $methodStart = $text.LastIndexOf("`n`t", $start); while ($methodStart -gt 0 -and $text[$methodStart + 2] -eq "`t") { $methodStart = $text.LastIndexOf("`n`t", $methodStart - 1) }
        $endMatch = [regex]::Match($text.Substring($start), '\n\t(public|private|protected|internal|static|\[|#)'); $end = if ($endMatch.Success) { $start + $endMatch.Index } else { $text.Length }
        $scope = $text.Substring($methodStart, $end - $methodStart)
        if ([regex]::IsMatch($scope, "\b$new\b")) { Write-Host "skip $old -> $new (name in use)"; continue }
        $renamed = [regex]::Replace($scope, "\b$old\b", $new)
        $text = $text.Substring(0, $methodStart) + $renamed + $text.Substring($end); $changed++; $done = $false; break
    }
    if ($done) { break }
}
[IO.File]::WriteAllText($full, $text.Replace("`n", $nl), (New-Object Text.UTF8Encoding($bom)))
"$Path : $changed renamed"
