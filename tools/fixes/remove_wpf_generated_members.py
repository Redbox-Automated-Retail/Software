"""Remove XAML-compiler-generated members that ILSpy left in WPF code-behind.

PresentationBuildTasks regenerates these into obj/*.g.cs when the XAML is compiled, so keeping the decompiled
copies causes duplicate-member errors (CS0111). Only members marked [GeneratedCode("PresentationBuildTasks", ...)]
are removed.

usage: python remove_wpf_generated_members.py <dir> [<dir> ...]
"""
import os
import re
import sys

MEMBER = re.compile(
    r'\n(?:\t\[DebuggerNonUserCode\]\r?\n)?\t\[GeneratedCode\("PresentationBuildTasks", "[\d.]+"\)\]\r?\n'
    r'(?:\t\[[^\n]*\]\r?\n)*'
    r'\t[^\n{;]*\([^\n]*\)\r?\n\t\{\r?\n(?:\t\t[^\n]*\r?\n|\r?\n)*?\t\}\r?\n')


def fix(path):
    src = open(path, encoding='utf-8-sig').read()
    new = MEMBER.sub('\n', src)
    new = re.sub(r'\n\n\n+', '\n\n', new)
    new = re.sub(r'\n\n(\}\s*)$', r'\n\1', new)
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
                if f.endswith('.xaml.cs') and fix(os.path.join(dp, f)):
                    n += 1
    print('fixed', n, 'files')
