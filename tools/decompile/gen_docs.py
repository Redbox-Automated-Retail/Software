"""Write docs/assemblies.md: shipped binary -> project mapping."""
import os, json, collections
# work directory holding raw/ (ilspycmd output), allrefs/, meta.tsv, firstparty.txt
S = os.environ.get('REDBOX_DECOMP_WORK') or os.path.dirname(os.path.abspath(__file__))
REPO = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
plan = json.load(open(os.path.join(S, 'plan.json')))
import importlib.util
spec = importlib.util.spec_from_file_location('org', os.path.join(os.path.dirname(os.path.abspath(__file__)), 'organize.py'))
org = importlib.util.module_from_spec(spec); spec.loader.exec_module(org)

rows = collections.defaultdict(list)
for line in open(os.path.join(S, 'meta.tsv'), encoding='utf-8'):
    r = line.rstrip('\n').split('\t')
    rows[r[0]].append(r)

def tfm_of(p):
    t = open(os.path.join(REPO, 'src', plan[p]['area'], p, p + '.csproj'), encoding='utf-8').read()
    import re
    m = re.search(r'<TargetFramework>([^<]+)<', t)
    return m.group(1) if m else ''

out = ['# Assembly inventory', '',
       'Every Redbox-authored .NET binary found in the kiosk image, and the project that rebuilds it.',
       'Paths are relative to the root of the kiosk image. Where a binary exists both in `bin/` and',
       '`bin/.staged_/`, the `.staged_` copy (the newest pending update) was decompiled.', '']
by_area = collections.defaultdict(list)
for p, i in plan.items():
    by_area[i['area']].append(p)
for area in sorted(by_area):
    out += [f'## src/{area}', '', '| Project | Version | Target | Shipped as | Notes |', '|---|---|---|---|---|']
    for p in sorted(by_area[area], key=str.lower):
        notes = []
        if p in org.EXTRACT:
            notes.append('no standalone DLL shipped; recovered from ILMerged copies in ' + ', '.join(f'`{h}`' for h in plan[p]['hosts']))
            ver, paths = '', ''
        else:
            rs = rows[p]
            ver = sorted({r[1] for r in rs})[-1]
            paths = '<br>'.join(sorted({'`' + r[8].lstrip('./').replace('/.staged_', '') + '`' for r in rs}))
        if p in org.KEEP_INLINE:
            notes.append('older merged copies of Redbox libraries kept inline (incompatible with current versions)')
        merged = [d for d in plan[p]['deps'] if d in plan]
        out.append(f'| `{p}` | {ver} | {tfm_of(p)} | {paths} | {"; ".join(notes)} |')
    out.append('')
open(os.path.join(REPO, 'docs', 'assemblies.md'), 'w', encoding='utf-8').write('\n'.join(out))
print('ok')
