"""Quote markup-extension values that ILSpy's BAML decompiler left unquoted.

ILSpy writes e.g. {Binding Path=Margin, FallbackValue=8,0,8,0} - the XAML compiler then splits the value at
its commas (MC3041/MC3042). This re-joins such values and quotes them: FallbackValue='8,0,8,0'.

usage: python fix_xaml_markup.py <dir> [<dir> ...]
"""
import os
import re
import sys

ATTR = re.compile(r'(\s[\w:.]+=")(\{[^"]*\})(")')
NAMED = re.compile(r'^\s*[A-Za-z_][\w.]*\s*=')


def split_top(s):
    """split on commas that are not inside {} or quotes"""
    parts, depth, quote, cur = [], 0, None, ''
    for ch in s:
        if quote:
            cur += ch
            if ch == quote:
                quote = None
            continue
        if ch == "'":
            quote = ch
        elif ch == '{':
            depth += 1
        elif ch == '}':
            depth -= 1
        elif ch == ',' and depth == 0:
            parts.append(cur)
            cur = ''
            continue
        cur += ch
    parts.append(cur)
    return parts


def fix_value(v):
    stripped = v.strip()
    if stripped.startswith('{'):
        return v[:len(v) - len(v.lstrip())] + fix_markup(stripped)
    if stripped.startswith("'") and stripped.endswith("'"):
        return v
    if not stripped or any(c in stripped for c in (',', '=', '"', '&quot;')):
        return v[:len(v) - len(v.lstrip())] + "'" + stripped.replace("'", "\\'") + "'"
    return v


def fix_markup(m):
    inner = m[1:-1]
    sp = re.match(r'\s*[\w:.]+', inner)
    if not sp:
        return m
    head, rest = inner[:sp.end()], inner[sp.end():]
    if not rest.strip():
        return m
    tokens = split_top(rest)
    merged = []
    for t in tokens:
        if merged and '=' in merged[-1].split('{')[0] and not NAMED.match(t):
            merged[-1] += ',' + t      # continuation of the previous named value
        else:
            merged.append(t)
    out = []
    for t in merged:
        if NAMED.match(t):
            name, val = t.split('=', 1)
            out.append(name + '=' + fix_value(val))
        else:
            out.append(fix_value(t) if t.strip().startswith('{') else t)
    return '{' + head + ','.join(out) + '}'


def fix_file(path):
    src = open(path, encoding='utf-8-sig').read()
    new = ATTR.sub(lambda m: m.group(1) + fix_markup(m.group(2)) + m.group(3), src)
    if new != src:
        open(path, 'w', encoding='utf-8-sig').write(new)
        return True
    return False


if __name__ == '__main__':
    n = 0
    for root in sys.argv[1:]:
        for dp, dn, fn in os.walk(root):
            dn[:] = [d for d in dn if d not in ('bin', 'obj')]
            for f in fn:
                if f.endswith('.xaml') and fix_file(os.path.join(dp, f)):
                    n += 1
    print('fixed', n, 'files')
