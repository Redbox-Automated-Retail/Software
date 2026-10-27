"""Organize raw ILSpy output into the repo layout.

- strips ILMerged copies of other libraries (dedupe), adding ProjectReferences instead
- extracts hidden shared libraries (merged-only, no standalone DLL) into their own projects
- flattens root-namespace folders
- rewrites csproj references (ProjectReference / PackageReference / lib HintPath)
"""
import os, re, shutil, sys, collections, json
import xml.etree.ElementTree as ET

# work directory holding raw/ (ilspycmd output), allrefs/, meta.tsv, firstparty.txt
S = os.environ.get('REDBOX_DECOMP_WORK') or os.path.dirname(os.path.abspath(__file__))
RAW = os.path.join(S, 'raw')
REPO = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
SRC = os.path.join(REPO, 'src')

FIRSTPARTY = open(os.path.join(S, 'firstparty.txt')).read().split()

AREA = {}
def area(a, names):
    for n in names.split():
        AREA[n] = a
area('Common', 'Redbox.Core Redbox.Command.Tokenizer Redbox.Compression Redbox.GetOpts Redbox.IPC.Framework Redbox.JSONPrettyPrinter Redbox.Log.Framework Redbox.Lua Redbox.Macros Redbox.Macros.Functions Redbox.ProductLookupCatalog Redbox.REDS.Framework Redbox.Tokenizer.Framework Redbox.USB')
area('HAL', 'Redbox.HAL.Client Redbox.HAL.Component.Model Redbox.HAL.Configuration Redbox.HAL.Controller.Framework Redbox.HAL.Core Redbox.HAL.DataMatrix.Framework Redbox.HAL.DataStorage Redbox.HAL.DirectShow.Frame.Plugin Redbox.HAL.IPC.Framework Redbox.HAL.Script.Framework Redbox.HAL.Service.Framework halsvc-win32 halmgmtc HalUtilities MSHALTester CameraTuner HAL.PackageInstaller Redbox.DirectShow Redbox.HAL.Common.GUI.Functions Interop.ClearImage')
area('HAL/Legacy', 'Redbox.HAL.Camera.Framework Redbox.HAL.ClearimageV1 Redbox.HAL.DirectShow.Plugin Redbox.HAL.ICameraPlugin.Vidcap')
area('KioskEngine', 'kioskengine Redbox.KioskEngine.API Redbox.KioskEngine.ComponentModel Redbox.KioskEngine.Drawing Redbox.KioskEngine.Environment Redbox.KioskEngine.IDE Redbox.KioskEngine.Kernel Redbox.KioskEngine.PreferenceManager Redbox.ControlPanel.UI Redbox.Controls Redbox.FMA.UI redscc Redbox.Services.KioskBrokerServices.KioskShared')
area('KioskServices', 'Redbox.BrokerServices.Kernel Redbox.BrokerServices.KernelExtension Redbox.BrokerServices.Proxy Redbox.BrokerServices.Proxy.ComponentModel Redbox.FraudServices.Proxy Redbox.FraudServices.Proxy.ComponentModel Redbox.HardwareServices.Kernel Redbox.HardwareServices.KernelExtension Redbox.HardwareServices.Proxy Redbox.HardwareServices.Proxy.ComponentModel Redbox.KioskDataServices.Proxy Redbox.KioskDataServices.Proxy.ComponentModel Redbox.KioskServices.ComponentModel Redbox.KioskServices.Kernel Redbox.KioskServices.KernelExtension Redbox.KioskServices.Proxy Redbox.KioskServices.Proxy.ComponentModel Redbox.UpdateServices.Kernel Redbox.UpdateServices.KernelExtension Redbox.UpdateServices.Proxy Redbox.UpdateServices.Proxy.ComponentModel')
area('Rental', 'Redbox.Rental.Data Redbox.Rental.Kernel Redbox.Rental.Model Redbox.Rental.Services Redbox.Rental.UI')
area('DeviceService', 'DeviceService.Client DeviceService.Client.Core DeviceService.ComponentModel DeviceService.Domain DeviceService.WebApi RBA_SDK_ComponentModel RBA_SDK_STANDARD RBAInstaller BluefinService.Client HttpHelper Status.Client')
area('UpdateManager', 'updatemgrd updatemgr-ui update-console installer-front-end Redbox.UpdateManager.Client Redbox.UpdateManager.ComponentModel Redbox.UpdateService.Client Redbox.UpdateService.Model Redbox.DeltaCompression Redbox.UpdateManager.BITS Redbox.UpdateManager.Environment Redbox.UpdateManager.Kernel Redbox.UpdateManager.Remoting Redbox.UpdateManager.TaskScheduler Redbox.UpdateManager.ServiceProxies TaskScheduler')
area('UpdateClient', 'UpdateClientService.API Redbox.NetCore.Logging Redbox.NetCore.Middleware')
area('KioskClient', 'KioskClientService.API')
area('Tools', 'KioskShell kioskutil inventoryimport')

# Hidden shared libraries that only exist ILMerged into other binaries.
# name -> (namespaces, hosts in priority order, tfm)
EXTRACT = {
    'Redbox.DirectShow': (['Redbox.DirectShow', 'Redbox.DirectShow.Interop'], ['Redbox.HAL.Script.Framework', 'Redbox.HAL.DirectShow.Frame.Plugin', 'MSHALTester', 'CameraTuner'], 'net35'),
    'Redbox.HAL.Common.GUI.Functions': (['Redbox.HAL.Common.GUI.Functions', 'Redbox.HAL.Common.GUI.Functions.Properties'], ['MSHALTester', 'CameraTuner'], 'net35'),
    'Interop.ClearImage': (['ClearImage'], ['Redbox.HAL.DataMatrix.Framework'], 'net35'),
    'Redbox.DeltaCompression': (['Redbox.DeltaCompression', 'Redbox.DeltaCompression.Binary', 'Redbox.UpdateService.DeltaCompression.Instruction'], ['updatemgrd', 'update-console', 'installer-front-end'], 'net46'),
    'Redbox.UpdateManager.BITS': (['Redbox.UpdateManager.BITS'], ['updatemgrd', 'update-console', 'installer-front-end'], 'net46'),
    'Redbox.UpdateManager.Environment': (['Redbox.UpdateManager.Environment'], ['updatemgrd', 'update-console', 'installer-front-end'], 'net46'),
    'Redbox.UpdateManager.Kernel': (['Redbox.UpdateManager.Kernel'], ['installer-front-end', 'updatemgrd', 'update-console'], 'net46'),
    'Redbox.UpdateManager.Remoting': (['Redbox.UpdateManager.Remoting'], ['updatemgrd', 'update-console', 'installer-front-end'], 'net46'),
    'Redbox.UpdateManager.TaskScheduler': (['Redbox.UpdateManager.TaskScheduler'], ['updatemgrd', 'update-console', 'installer-front-end'], 'net46'),
    'Redbox.UpdateManager.ServiceProxies': (['Redbox.UpdateManager.ServiceProxies'], ['update-console', 'updatemgr-ui'], 'net46'),
    'TaskScheduler': (['TaskScheduler', 'TaskSchedulerInterop'], ['updatemgrd', 'update-console', 'installer-front-end'], 'net46'),
}
EXTRACT_NS = {ns: g for g, (nss, _, _) in EXTRACT.items() for ns in nss}

# Old/diverged binaries that merged older, incompatible copies of Redbox libraries:
# keep their merged Redbox code inline (third-party code is still stripped).
KEEP_INLINE = {'halmgmtc', 'inventoryimport', 'redscc', 'kioskutil',
               'Redbox.HAL.ClearimageV1', 'Redbox.HAL.DirectShow.Plugin'}

THIRD_NS = [  # namespace prefix -> third-party key
    ('Newtonsoft.Json', 'Newtonsoft.Json'), ('log4net', 'log4net'), ('ICSharpCode.SharpZipLib', 'ICSharpCode.SharpZipLib'),
    ('SevenZip', 'SevenZip'), ('LzmaAlone', 'SevenZip'), ('Wintellect.PowerCollections', 'PowerCollections'),
    ('Jayrock', 'Jayrock.Json'), ('Ionic', 'DotNetZip'), ('Quartz', 'Quartz'),
]
NEWTONSOFT_MERGED_ATTR_NS = ('Microsoft.CodeAnalysis', 'System.Runtime.CompilerServices')

LIB = {  # third-party key -> lib path (relative to repo)
    'GenuineChannels': 'lib/GenuineChannels/GenuineChannels.dll',
    'ICSharpCode.SharpZipLib': 'lib/SharpZipLib/0.85.5/ICSharpCode.SharpZipLib.dll',
    'InputSimulator': 'lib/InputSimulator/InputSimulator.dll',
    'Interop.NetFwTypeLib': 'lib/Interop/Interop.NetFwTypeLib.dll',
    'Jayrock.Json': 'lib/Jayrock/Jayrock.Json.dll',
    'PowerCollections': 'lib/PowerCollections/PowerCollections.dll',
    'Puzzle.SyntaxBox.NET3.5': 'lib/Puzzle.SyntaxBox/Puzzle.SyntaxBox.NET3.5.dll',
    'SevenZip': 'lib/SevenZip/SevenZip.dll',
    'Skybound.VisualTips': 'lib/Skybound.VisualTips/Skybound.VisualTips.dll',
    'SmartPropertyGrid': 'lib/SmartPropertyGrid/SmartPropertyGrid.dll',
    'WeifenLuo.WinFormsUI.Docking': 'lib/WeifenLuo/WeifenLuo.WinFormsUI.Docking.dll',
    'VistaDB.NET20': 'lib/VistaDB/VistaDB.NET20.dll',
    'Quartz@1': 'lib/Quartz/1.0.2.3/Quartz.dll',
    'Common.Logging@1': 'lib/Common.Logging/1.2.0.0/Common.Logging.dll',
    'log4net@1.2.9': 'lib/log4net/1.2.9/log4net.dll',
}
PKG = {  # third-party key -> (package, version) for .NET Framework projects
    'Newtonsoft.Json': ('Newtonsoft.Json', '12.0.3'),
    'log4net': ('log4net', '2.0.8'),
    'Quartz': ('Quartz', '2.3.3'),
    'Swashbuckle.Core': ('Swashbuckle.Core', '5.6.0'),
    'System.Net.Http.Formatting': ('Microsoft.AspNet.WebApi.Client', '5.2.7'),
    'System.Web.Http': ('Microsoft.AspNet.WebApi.Core', '5.2.7'),
    'System.Web.Http.SelfHost': ('Microsoft.AspNet.WebApi.SelfHost', '5.2.7'),
    'DotNetZip': ('DotNetZip', '1.9.1.8'),
}

# .NET Core / .NET Standard projects: explicit package lists taken from the shipped *.deps.json files
CORE_PKGS = {
    'DeviceService.ComponentModel': [('Newtonsoft.Json', '12.0.1')],
    'DeviceService.Client.Core': [('Microsoft.AspNetCore.SignalR.Client', '1.1.0')],
    'DeviceService.Client': [('Microsoft.AspNetCore.SignalR.Client', '1.1.0'), ('Newtonsoft.Json', '12.0.3')],
    'DeviceService.Domain': [('Microsoft.Win32.Registry', '5.0.0'), ('Serilog', '2.9.0'), ('System.IO.Ports', '4.6.0'),
                             ('Microsoft.Extensions.Logging.Abstractions', '2.2.0'), ('Microsoft.Extensions.Options', '2.2.0')],
    'DeviceService.WebApi': [('Microsoft.AspNetCore.App', None), ('Microsoft.AspNetCore.Hosting.WindowsServices', '2.2.0'),
                             ('Microsoft.AspNetCore.Mvc.Core', '2.2.0'), ('Microsoft.AspNetCore.SignalR', '1.1.0'),
                             ('Serilog', '2.9.0'), ('Serilog.AspNetCore', '2.1.1'), ('Serilog.Enrichers.Thread', '3.0.0'),
                             ('Serilog.Settings.Configuration', '3.0.1'), ('Serilog.Sinks.Async', '1.3.0'), ('Serilog.Sinks.Console', '3.1.1'),
                             ('Serilog.Sinks.RollingFile', '3.3.0'), ('Swashbuckle.AspNetCore', '4.0.1'), ('System.ValueTuple', '4.5.0')],
    'RBAInstaller': [('Microsoft.Extensions.Configuration', '2.2.0'), ('Microsoft.Extensions.Configuration.Json', '2.2.0'),
                     ('Microsoft.Extensions.DependencyInjection', '2.2.0'), ('Microsoft.Extensions.Hosting', '2.2.0'),
                     ('Microsoft.Extensions.Logging', '2.2.0'), ('Microsoft.Extensions.Options', '2.2.0'),
                     ('Microsoft.Extensions.DependencyModel', '2.0.4'),
                     ('Serilog', '2.9.0'), ('Serilog.Enrichers.Thread', '3.0.0'), ('Serilog.Extensions.Logging', '2.0.4'),
                     ('Serilog.Settings.Configuration', '3.0.1'), ('Serilog.Sinks.Console', '3.1.1')],
    'BluefinService.Client': [('Newtonsoft.Json', '12.0.1')],
    'HttpHelper': [('Newtonsoft.Json', '12.0.1')],
    'Status.Client': [('Newtonsoft.Json', '12.0.1'), ('Serilog', '2.9.0')],
    'KioskClientService.API': [('Microsoft.AspNetCore.App', None), ('Coravel', '4.0.2'), ('Microsoft.AspNetCore.Hosting.WindowsServices', '2.2.0'),
                               ('Microsoft.VisualStudio.Web.CodeGeneration.Design', '2.2.4'), ('Newtonsoft.Json', '12.0.2'), ('QRCoder', '1.3.9'),
                               ('Serilog', '2.8.0'), ('Serilog.AspNetCore', '2.1.1'), ('Serilog.Enrichers.Thread', '3.1.0'),
                               ('Serilog.Settings.Configuration', '3.1.0'), ('Serilog.Sinks.Async', '1.4.0'), ('Serilog.Sinks.Console', '3.1.1'),
                               ('Serilog.Sinks.RollingFile', '3.3.0'), ('Swashbuckle.AspNetCore', '4.0.1'), ('System.Collections', '4.3.0')],
    'UpdateClientService.API': [('Microsoft.AspNetCore.App', None), ('Coravel', '3.6.1'), ('MQTTnet', '3.1.2'),
                                ('Microsoft.AspNetCore.Hosting.WindowsServices', '2.2.0'), ('Nito.AsyncEx.Coordination', '5.0.0'),
                                ('Serilog', '2.9.0'), ('Serilog.AspNetCore', '3.2.0'), ('Serilog.Enrichers.Thread', '3.1.0'),
                                ('Serilog.Settings.Configuration', '3.1.0'), ('Serilog.Sinks.Async', '1.4.0'), ('Serilog.Sinks.Console', '3.1.1'),
                                ('Serilog.Sinks.RollingFile', '3.3.0'), ('Swashbuckle.AspNetCore', '6.1.5'), ('Swashbuckle.AspNetCore.Newtonsoft', '6.1.5')],
    'Redbox.NetCore.Logging': [('Microsoft.AspNetCore.Hosting.Abstractions', '2.2.0'), ('Microsoft.AspNetCore.Mvc.Abstractions', '2.2.0'),
                               ('Microsoft.AspNetCore.Mvc.Core', '2.2.0'), ('Microsoft.Extensions.Configuration.Abstractions', '2.2.0'),
                               ('Microsoft.Extensions.Configuration.Binder', '2.2.0'), ('Serilog.AspNetCore', '3.2.0'),
                               ('Serilog.Filters.Expressions', '2.0.0'), ('Serilog.Settings.Configuration', '3.1.0'),
                               ('Serilog.Sinks.Async', '1.4.0'), ('Serilog.Sinks.Console', '3.1.1'),
                               ('System.Configuration.ConfigurationManager', '4.5.0')],
    'Redbox.NetCore.Middleware': [('AWSSDK.CloudWatch', '3.5.0.17'), ('App.Metrics', '4.1.0'), ('App.Metrics.AspNetCore', '2.0.0'),
                                  ('App.Metrics.AspNetCore.Hosting', '2.0.0'), ('App.Metrics.Formatters.Prometheus', '4.1.0'),
                                  ('Microsoft.AspNetCore.Mvc.Abstractions', '2.2.0'), ('Microsoft.AspNetCore.Mvc.Core', '2.2.0'),
                                  ('Microsoft.AspNetCore.Server.Kestrel.Core', '2.2.0'), ('Microsoft.EntityFrameworkCore', '2.2.1'),
                                  ('Microsoft.Extensions.Http', '2.2.0')],
}

HAL_AREAS = ('HAL', 'HAL/Legacy')
# .NET Core / .NET Standard binaries were never ILMerged: a namespace shared with another assembly is their own code
NOT_MERGED_AREAS = ('UpdateClient', 'KioskClient', 'DeviceService')

# ---------------------------------------------------------------- helpers
def raw_ns_dirs(p):
    d = os.path.join(RAW, p)
    return [x for x in os.listdir(d) if os.path.isdir(os.path.join(d, x))]

WPF_LOWER = lambda name: name[:1].islower() and '.' not in name and name != 'log4net'   # ILSpy BAML resource folders (assets, views, ...)

def third_key(ns):
    for pre, key in THIRD_NS:
        if ns == pre or ns.startswith(pre + '.'):
            return key
    return None

# owner of a namespace by standalone assembly name (longest prefix)
def name_owner(ns):
    best = None
    for a in FIRSTPARTY:
        if ns == a or ns.startswith(a + '.'):
            if best is None or len(a) > len(best):
                best = a
    return best

def classify(p, ns, newtonsoft_merged):
    """returns ('own',) | ('third', key) | ('dep', project) | ('extract', group)"""
    k = third_key(ns)
    if k == 'SevenZip' and AREA.get(p) in HAL_AREAS:
        # HAL merged a custom LZMA SDK build (with SevenZipHelper) into Redbox.HAL.Component.Model; no standalone DLL exists
        if p == 'Redbox.HAL.Component.Model' or p in KEEP_INLINE:
            return ('own',)
        return ('dep', 'Redbox.HAL.Component.Model')
    if k:
        return ('third', k)
    if newtonsoft_merged and (ns in NEWTONSOFT_MERGED_ATTR_NS):
        return ('third', 'Newtonsoft.Json')
    g = EXTRACT_NS.get(ns)
    if g and p not in KEEP_INLINE:
        return ('extract', g)
    if p in KEEP_INLINE:
        return ('own',)
    if AREA.get(p) in HAL_AREAS and (ns == 'Redbox.IPC.Framework' or ns.startswith('Redbox.IPC.Framework.')):
        return ('own',) if p == 'Redbox.HAL.IPC.Framework' else ('dep', 'Redbox.HAL.IPC.Framework')
    o = name_owner(ns)
    if o and o != p and AREA.get(p) not in NOT_MERGED_AREAS:
        mine = set(os.listdir(os.path.join(RAW, p, ns))) if os.path.isdir(os.path.join(RAW, p, ns)) else set()
        theirs = set(os.listdir(os.path.join(RAW, o, ns))) if os.path.isdir(os.path.join(RAW, o, ns)) else set()
        if mine and not (mine & theirs):
            return ('own',)   # same namespace name, but genuinely this assembly's own types
        return ('dep', o)
    return ('own',)

def all_files(root):
    for dp, dn, fn in os.walk(root):
        for f in fn:
            full = os.path.join(dp, f)
            yield os.path.relpath(full, root).replace('\\', '/')

def file_ns(rel, nsdirs):
    """namespace key for a raw file path"""
    parts = rel.split('/')
    if len(parts) > 1:
        top = parts[0]
        if top == 'Properties' or WPF_LOWER(top):
            return None  # own, leave in place
        return top
    # root file: e.g. Redbox.Core.ErrorForm.resx, Redbox.Rental.UI.App.xaml
    name = parts[0]
    if name.endswith('.csproj') or name in ('app.ico', 'app.manifest'):
        return None
    if name in ('LICENSE', 'NOTICE') and 'log4net' in nsdirs:
        return 'log4net'
    best = None
    for ns in nsdirs:
        if name.startswith(ns + '.') and (best is None or len(ns) > len(best)):
            best = ns
    if best is None:
        # merged third-party compiler-generated files, e.g. log4net630819.-PrivateImplementationDetails-.cs
        for pre, key in THIRD_NS:
            if name.startswith(pre):
                return pre
        # namespace not present as directory: take longest dotted prefix that classifies as something
        bits = name.split('.')
        for i in range(len(bits) - 2, 0, -1):
            cand = '.'.join(bits[:i])
            if third_key(cand) or cand in EXTRACT_NS or name_owner(cand):
                return cand
        return None
    return best

def choose_root(own_ns, asm):
    # prefer this assembly's own namespaces over inline-kept copies of other libraries
    native = [n for n in own_ns if name_owner(n) in (None, asm) and n not in EXTRACT_NS]
    if native:
        own_ns = native
    tops = [n for n in own_ns if not any(n != o and n.startswith(o + '.') for o in own_ns)]
    if len(tops) > 1:
        common = os.path.commonprefix([t.split('.') for t in tops])
        if len(common) >= 2:
            return '.'.join(common)
    if not tops:
        return None
    if asm in tops:
        return asm
    for t in tops:  # case-insensitive match (HALUtilities vs HalUtilities)
        if t.lower() == asm.lower():
            return t
    counts = {t: sum(1 for n in own_ns if n == t or n.startswith(t + '.')) for t in tops}
    return max(tops, key=lambda t: counts[t])

def relocate(rel, ns, root):
    """new relative path for an own file"""
    parts = rel.split('/')
    if ns is None:
        return rel
    if len(parts) > 1:
        rest = '/'.join(parts[1:])
    else:
        rest = parts[0][len(ns) + 1:]
    if root and ns == root:
        return rest
    if root and ns.startswith(root + '.'):
        sub = ns[len(root) + 1:].replace('.', '/')
        return sub + '/' + rest
    return ns + '/' + rest

# ---------------------------------------------------------------- main
def main():
    if os.path.exists(SRC):
        for x in os.listdir(SRC):
            shutil.rmtree(os.path.join(SRC, x))
    plan = {}          # project -> info
    extract_files = collections.defaultdict(dict)   # group -> newrel -> src path
    ns_owner = {}      # namespace -> project (for using-based deps)
    report = []

    projects = FIRSTPARTY
    for p in projects:
        rawdir = os.path.join(RAW, p)
        nsdirs = [d for d in raw_ns_dirs(p) if d != 'Properties' and not WPF_LOWER(d)]
        newtonsoft_merged = any(d.startswith('Newtonsoft.Json') for d in nsdirs)
        cls = {ns: classify(p, ns, newtonsoft_merged) for ns in nsdirs}
        own_ns = [ns for ns, c in cls.items() if c[0] == 'own']
        root = choose_root(own_ns, p)
        deps, thirds, files, moves = set(), set(), {}, {}
        for rel in all_files(rawdir):
            if rel.endswith('.csproj'):
                continue
            ns = file_ns(rel, nsdirs)
            c = ('own',) if ns is None else cls.get(ns) or classify(p, ns, newtonsoft_merged)
            if c[0] == 'own':
                new = relocate(rel, ns, root)
                files[new] = os.path.join(rawdir, rel)
                moves[rel] = new
            elif c[0] == 'third':
                thirds.add(c[1])
            elif c[0] == 'dep':
                deps.add(c[1])
            elif c[0] == 'extract':
                g = c[1]
                deps.add(g)
                if p in EXTRACT[g][1]:
                    groot = EXTRACT[g][0][0]
                    new = relocate(rel, ns, groot)
                    prio = EXTRACT[g][1].index(p)
                    cur = extract_files[g].get(new)
                    if cur is None or EXTRACT[g][1].index(cur[1]) > prio:
                        extract_files[g][new] = (os.path.join(rawdir, rel), p, rel)
        if p not in KEEP_INLINE:
            for ns in own_ns:
                ns_owner.setdefault(ns, p)
        plan[p] = dict(root=root, deps=deps, thirds=thirds, files=files, moves=moves, own_ns=own_ns, cls=cls,
                       merged=sorted(deps), merged_third=sorted(thirds))

    for g, (nss, hosts, tfm) in EXTRACT.items():
        plan[g] = dict(root=nss[0], deps=set(), thirds=set(),
                       files={k: v[0] for k, v in extract_files[g].items()}, moves={v[2]: k for k, v in extract_files[g].items()}, own_ns=nss, cls={}, tfm=tfm,
                       hosts=sorted({v[1] for v in extract_files[g].values()}))
        for ns in nss:
            ns_owner[ns] = g

    # write files
    for p, info in plan.items():
        dest = os.path.join(SRC, AREA[p], p)
        # WPF: pair code-behind with its XAML (X.xaml + X.xaml.cs)
        xaml = {n[:-5].lower() for n in info['files'] if n.endswith('.xaml')}
        renamed = {}
        for new in list(info['files']):
            if new.endswith('.cs') and new[:-3].lower() in xaml:
                renamed[new] = new[:-3] + '.xaml.cs'
        for old, new in renamed.items():
            info['files'][new] = info['files'].pop(old)
            for k, v in info['moves'].items():
                if v == old:
                    info['moves'][k] = new
        for new, srcf in info['files'].items():
            d = os.path.join(dest, new)
            os.makedirs(os.path.dirname(d), exist_ok=True)
            shutil.copy2(srcf, d)
        info['dest'] = dest

    # ILMerge /internalize made the merged-only libraries' public types internal; undo that for extracted projects
    vis_re = re.compile(r'^internal ((?:static |sealed |abstract |partial |unsafe )*(?:class|struct|interface|enum|delegate) )', re.M)
    for g in EXTRACT:
        for new in plan[g]['files']:
            if new.endswith('.cs'):
                fp = os.path.join(plan[g]['dest'], new)
                t = open(fp, encoding='utf-8-sig').read()
                t2 = vis_re.sub(lambda m: "public " + m.group(1), t)
                if t2 != t:
                    open(fp, 'w', encoding='utf-8-sig').write(t2)

    # using-based deps for extracted projects (and to catch gaps elsewhere)
    using_re = re.compile(r'^using\s+(?:static\s+)?([A-Za-z_][\w.]*)\s*;', re.M)
    for p, info in plan.items():
        used = set()
        for new in info['files']:
            if new.endswith('.cs'):
                txt = open(os.path.join(info['dest'], new), encoding='utf-8-sig', errors='replace').read()
                used.update(using_re.findall(txt))
        info['used'] = used
        for u in used:
            if (p in EXTRACT or p in KEEP_INLINE) and u not in info['own_ns']:
                o = EXTRACT_NS.get(u) or name_owner(u) or ns_owner.get(u)
                if AREA[p] in HAL_AREAS and u.startswith('Redbox.IPC.Framework'):
                    o = 'Redbox.HAL.IPC.Framework'
                if o in KEEP_INLINE:
                    o = None
                if o and o != p:
                    info['deps'].add(o)
                k = third_key(u)
                if k:
                    info['thirds'].add(k)

    json.dump({p: i['moves'] for p, i in plan.items()}, open(os.path.join(S, 'moves.json'), 'w'), indent=0)
    resx = {}
    for p, i in plan.items():
        resx[p] = {new: os.path.basename(old)[:-5] + '.resources' for old, new in i['moves'].items() if old.endswith('.resx')}
    json.dump(resx, open(os.path.join(S, 'resx_names.json'), 'w'), indent=0)
    json.dump({p: dict(root=i['root'], deps=sorted(i['deps']), thirds=sorted(i['thirds']),
                       hosts=i.get('hosts'), area=AREA[p], merged=i.get('merged', []),
                       merged_third=i.get('merged_third', [])) for p, i in plan.items()},
              open(os.path.join(S, 'plan.json'), 'w'), indent=1)
    print('projects:', len(plan))

if __name__ == '__main__':
    main()
