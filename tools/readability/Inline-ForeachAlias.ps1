# Inline-ForeachAlias -Files <paths>
# The decompiler turns "foreach (var x in Expr())" over an array into
#     T[] array = Expr();
#     foreach (T x in array)
# When the alias is used nowhere else in its member, put the expression back into the foreach. The expression is still
# evaluated exactly once, before the loop, so behaviour is unchanged.
param([string[]] $Files)
$total = 0
$decl = [regex]'(?m)^(?<ind>\t+)[\w.<>,\[\]? ]+\[\] (?<name>array\d*|\w+Array\d*) = (?<expr>[^\r\n;]+);\n\k<ind>foreach \((?<var>[^\r\n]+?) in \k<name>\)\n'
foreach ($f in $Files) {
    $bytes = [IO.File]::ReadAllBytes($f); $bom = $bytes.Length -ge 3 -and $bytes[0] -eq 0xEF
    $text = [IO.File]::ReadAllText($f); $nl = if ($text.Contains("`r`n")) { "`r`n" } else { "`n" }; $text = $text.Replace("`r`n", "`n")
    $count = 0
    while ($true) {
        $done = $true
        foreach ($m in $decl.Matches($text)) {
            $name = $m.Groups['name'].Value; $ind = $m.Groups['ind'].Value
            # The alias's scope ends at the first closing brace one level out.
            $after = $text.Substring($m.Index + $m.Length)
            $end = [regex]::Match($after, "\n$($ind.Substring(1))\}")
            $scope = if ($end.Success) { $after.Substring(0, $end.Index) } else { $after }
            if ([regex]::IsMatch($scope, "\b$name\b")) { continue }
            $replacement = "$ind" + "foreach ($($m.Groups['var'].Value) in $($m.Groups['expr'].Value))`n"
            $text = $text.Substring(0, $m.Index) + $replacement + $after
            $count++; $done = $false
            break
        }
        if ($done) { break }
    }
    if ($count) {
        [IO.File]::WriteAllText($f, $text.Replace("`n", $nl), (New-Object Text.UTF8Encoding($bom)))
        $total += $count
    }
}
"inlined $total"
