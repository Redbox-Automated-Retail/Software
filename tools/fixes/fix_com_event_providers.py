"""Make tlbimp-generated COM event provider classes compilable.

tlbimp emits `*_EventProvider` classes whose events are implemented directly by `add_X`/`remove_X`
methods. Decompiled, those are plain methods, which C# rejects as interface event implementations
(CS0470/CS0535). This renames each accessor method and adds an event that forwards to it.

usage: python fix_com_event_providers.py <dir> [<dir> ...]
"""
import os
import re
import sys

ACCESSOR = re.compile(r'public void (add|remove)_(\w+)\((\w+) P_0\)')


def fix(path):
    src = open(path, encoding='utf-8-sig').read()
    events = {}
    for kind, name, handler in ACCESSOR.findall(src):
        events[name] = handler
    if not events:
        return False
    src = ACCESSOR.sub(lambda m: f'private void {m.group(1)}_{m.group(2)}_Impl({m.group(3)} P_0)', src)
    wrappers = ''.join(
        f'\n\tpublic event {handler} {name}\n\t{{\n\t\tadd\n\t\t{{\n\t\t\t{"add"}_{name}_Impl(value);\n\t\t}}\n'
        f'\t\tremove\n\t\t{{\n\t\t\tremove_{name}_Impl(value);\n\t\t}}\n\t}}\n'
        for name, handler in events.items())
    # insert the events before the class's closing brace
    idx = src.rstrip().rfind('}')
    src = src[:idx].rstrip('\r\n') + '\n' + wrappers + src[idx:]
    open(path, 'w', encoding='utf-8-sig').write(src)
    return True


if __name__ == '__main__':
    for root in sys.argv[1:]:
        for dp, dn, fn in os.walk(root):
            for f in fn:
                if f.endswith('_EventProvider.cs') and fix(os.path.join(dp, f)):
                    print('fixed', os.path.join(dp, f))
