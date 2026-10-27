"""Generate SDK-style csproj files for the organized projects (run after organize.py)."""
import os, re, json, shutil, collections
import xml.etree.ElementTree as ET
from xml.sax.saxutils import escape
import importlib.util

# work directory holding raw/ (ilspycmd output), allrefs/, meta.tsv, firstparty.txt
S = os.environ.get('REDBOX_DECOMP_WORK') or os.path.dirname(os.path.abspath(__file__))
spec = importlib.util.spec_from_file_location('org', os.path.join(os.path.dirname(os.path.abspath(__file__)), 'organize.py'))
org = importlib.util.module_from_spec(spec); spec.loader.exec_module(org)

plan = json.load(open(os.path.join(S, 'plan.json')))
REPO, SRC, RAW = org.REPO, org.SRC, org.RAW
ALLREFS = os.path.join(S, 'allrefs')

# original assembly references (name -> version) per first-party assembly
origrefs = {}
for line in open(os.path.join(S, 'meta.tsv'), encoding='utf-8'):
    r = line.rstrip('\n').split('\t')
    if r[0] in plan and r[0] not in origrefs and '.staged_' in r[8]:
        origrefs[r[0]] = dict(x.split('/') for x in r[9].split(';') if x)

def proj_dir(p):
    return os.path.join(SRC, plan[p]['area'], p)

def rel(frm, to):
    return os.path.relpath(to, frm).replace('/', '\\')

def is_core(tfm):
    return tfm.startswith('netcoreapp') or tfm.startswith('netstandard')

FX_REF_IGNORE = {'mscorlib', 'System', 'netstandard'}
FX_ASSEMBLIES = {'PresentationCore', 'PresentationFramework', 'WindowsBase', 'System.Xaml', 'WindowsFormsIntegration',
                 'ReachFramework', 'System.Printing', 'UIAutomationProvider', 'UIAutomationTypes', 'UIAutomationClient',
                 'PresentationFramework.Aero', 'System.Windows.Forms', 'System.Drawing', 'System.Speech', 'Microsoft.CSharp',
                 'Microsoft.VisualBasic', 'System.Web', 'System.Web.Extensions', 'System.Management', 'System.ServiceProcess',
                 'System.Configuration', 'System.Configuration.Install', 'System.Runtime.Remoting', 'System.Runtime.Serialization',
                 'System.ServiceModel', 'System.Data', 'System.Data.Linq', 'System.Xml', 'System.Xml.Linq', 'System.Core',
                 'System.Net.Http', 'System.Transactions', 'System.Security', 'System.Design', 'System.IdentityModel',
                 'System.ComponentModel.DataAnnotations', 'System.Numerics', 'System.IO.Compression', 'System.IO.Compression.FileSystem',
                 'System.DirectoryServices', 'System.Messaging', 'System.EnterpriseServices', 'Accessibility', 'stdole'}

def third_items(p, tfm, keys, hints):
    """-> (packages{id:ver}, libs{name:path})"""
    pk, libs = {}, {}
    ov = origrefs.get(p, {})
    area = plan[p]['area']
    def lib(key, name=None):
        libs[name or os.path.splitext(os.path.basename(org.LIB[key]))[0]] = org.LIB[key]
    for k in sorted(set(keys) | set(hints)):
        if k == 'log4net':
            if area.startswith('HAL') or ov.get('log4net') == '1.2.9.0':
                lib('log4net@1.2.9')
            elif tfm in ('net35', 'net20'):
                pk['log4net'] = '1.2.10'
            else:
                pk['log4net'] = '2.0.8'
        elif k == 'Quartz':
            if ov.get('Quartz', '').startswith('1.'):
                lib('Quartz@1'); lib('Common.Logging@1')
            else:
                pk['Quartz'] = '2.3.3'
        elif k in org.PKG:
            i, v = org.PKG[k]; pk[i] = v
        elif k in org.LIB:
            lib(k)
        else:
            print('  !! unmapped third-party', p, k)
    return pk, libs

def main():
    report = []
    for p, info in sorted(plan.items()):
        d = proj_dir(p)
        rawproj = os.path.join(RAW, p, p + '.csproj')
        props = collections.OrderedDict()
        fxrefs, embedded, hints = [], [], set()
        sdk = 'Microsoft.NET.Sdk'
        if os.path.exists(rawproj):
            root = ET.parse(rawproj).getroot()
            sdk = root.get('Sdk')
            for pg in root.findall('PropertyGroup'):
                for e in pg:
                    if e.tag in ('LangVersion', 'CheckForOverflowUnderflow', 'GenerateAssemblyInfo', 'RootNamespace'):
                        continue
                    props[e.tag] = e.text or ''
            for ig in root.findall('ItemGroup'):
                for e in ig:
                    inc = e.get('Include')
                    if e.tag == 'Reference':
                        h = e.find('HintPath')
                        if h is None:
                            if inc not in FX_REF_IGNORE:
                                fxrefs.append(inc)
                        elif inc not in plan:
                            if inc == 'System.Net.Http':
                                fxrefs.append(inc)
                            else:
                                hints.add(inc)
                    elif e.tag == 'EmbeddedResource':
                        embedded.append((inc.replace('\\', '/'), e.get('LogicalName')))
            moves = None
        else:  # extracted project
            props['AssemblyName'] = p
            props['TargetFramework'] = org.EXTRACT[p][2]
            host = plan[p]['hosts'][0]
            hroot = ET.parse(os.path.join(RAW, host, host + '.csproj')).getroot()
            for e in hroot.iter():
                if e.tag in ('PlatformTarget', 'UseWindowsForms', 'AllowUnsafeBlocks'):
                    props[e.tag] = e.text
                if e.tag == 'Reference' and e.find('HintPath') is None and e.get('Include') not in FX_REF_IGNORE:
                    fxrefs.append(e.get('Include'))
        tfm = props['TargetFramework']
        usewpf = props.get('UseWPF', '').lower() == 'true'
        if sdk == 'Microsoft.NET.Sdk.WindowsDesktop':
            sdk = 'Microsoft.NET.Sdk'
        if 'System.Windows.Forms' in origrefs.get(p, {}) and 'UseWindowsForms' not in props:
            props['UseWindowsForms'] = 'True'
        if sdk == 'Microsoft.NET.Sdk.Web':
            props['GenerateRazorAssemblyInfo'] = 'false'
        if info['root']:
            props['RootNamespace'] = info['root']
        if os.path.exists(os.path.join(d, 'Properties', 'AssemblyInfo.cs')):
            props['GenerateAssemblyInfo'] = 'false'
        rep = repack_list(p)
        if rep:
            props['ILRepackAssemblies'] = ';'.join(rep)
        if 'OutputType' in props and props['OutputType'] in ('Exe', 'WinExe') and 'ApplicationIcon' in props:
            if not os.path.exists(os.path.join(d, props['ApplicationIcon'])):
                del props['ApplicationIcon']

        # files actually present in the organized project
        present = set(org.all_files(d))
        # raw path -> new path (re-derive from organize logic by basename match)
        rawfiles = set(org.all_files(os.path.join(RAW, p))) if os.path.isdir(os.path.join(RAW, p)) else set()

        out = []
        out.append(f'<Project Sdk="{sdk}">')
        out.append('  <PropertyGroup>')
        order = ['AssemblyName', 'RootNamespace', 'OutputType', 'TargetFramework', 'PlatformTarget', 'UseWPF', 'UseWindowsForms',
                 'AllowUnsafeBlocks', 'ApplicationIcon', 'ApplicationManifest', 'GenerateAssemblyInfo']
        for k in order + [k for k in props if k not in order]:
            if k in props:
                out.append(f'    <{k}>{escape(props[k])}</{k}>')
        out.append('  </PropertyGroup>')

        # embedded (non-resx) resources, remapped
        emb = []
        mv = MOVES.get(p, {})
        for inc, logical in embedded:
            new = mv.get(inc)
            if new and new in present:
                emb.append((new, logical))
        # resx: explicit manifest names
        resx = []
        for f in sorted(present):
            if f.endswith('.resx'):
                resx.append(f)
        if emb or resx:
            out.append('  <ItemGroup>')
            for inc, logical in emb:
                w = inc.replace('/', '\\')
                out.append(f'    <None Remove="{escape(w)}" />')
                if usewpf and '/' in logical:
                    # entry of the <Assembly>.g.resources container (WPF pack resource), not a manifest resource
                    out.append(f'    <Resource Include="{escape(w)}" />')
                    continue
                out.append(f'    <EmbeddedResource Include="{escape(w)}" LogicalName="{escape(logical)}" />')
            for f in resx:
                logical = resx_logical(p, f, info)
                out.append(f'    <EmbeddedResource Update="{escape(f.replace("/", chr(92)))}" LogicalName="{escape(logical)}" />')
            out.append('  </ItemGroup>')

        if not is_core(tfm):
            # framework assemblies the original referenced (ILSpy omits some, e.g. WPF ones in WinForms projects)
            for r in origrefs.get(p, {}):
                if r in FX_ASSEMBLIES and not (usewpf and r in ('PresentationCore', 'PresentationFramework', 'WindowsBase', 'System.Xaml')):
                    fxrefs.append(r)
        refs = sorted(set(fxrefs))
        if refs:
            out.append('  <ItemGroup>')
            for r in refs:
                out.append(f'    <Reference Include="{r}" />')
            out.append('  </ItemGroup>')

        if is_core(tfm) or p in org.CORE_PKGS:
            pk = {i: v for i, v in org.CORE_PKGS.get(p, [])}
            libs = {}
        else:
            pk, libs = third_items(p, tfm, info['thirds'], hints)
        if libs:
            out.append('  <ItemGroup>')
            for name, path in sorted(libs.items()):
                out.append(f'    <Reference Include="{name}">')
                out.append(f'      <HintPath>{rel(d, os.path.join(REPO, path))}</HintPath>')
                out.append('    </Reference>')
            out.append('  </ItemGroup>')
        if pk:
            out.append('  <ItemGroup>')
            for i, v in sorted(pk.items()):
                out.append(f'    <PackageReference Include="{i}"' + (f' Version="{v}"' if v else '') + ' />')
            out.append('  </ItemGroup>')

        deps = set(info['deps'])
        for r in origrefs.get(p, {}):
            if r in plan and r != p:
                deps.add(r)
        deps.discard(p)
        if deps:
            out.append('  <ItemGroup>')
            for dep in sorted(deps):
                out.append(f'    <ProjectReference Include="{rel(d, os.path.join(proj_dir(dep), dep + ".csproj"))}" />')
            out.append('  </ItemGroup>')
        out.append('</Project>')
        open(os.path.join(d, p + '.csproj'), 'w', encoding='utf-8').write('\n'.join(out) + '\n')
        missing = [r for r in origrefs.get(p, {}) if r.startswith('Redbox.') and r not in plan]
        if missing:
            report.append(f'{p}: references missing assemblies {missing}')
    print('\n'.join(report))

THIRD_ASM = {'Newtonsoft.Json': 'Newtonsoft.Json', 'log4net': 'log4net', 'ICSharpCode.SharpZipLib': 'ICSharpCode.SharpZipLib',
             'SevenZip': 'SevenZip', 'PowerCollections': 'PowerCollections', 'Jayrock.Json': 'Jayrock.Json', 'DotNetZip': 'Ionic.Zip'}

def repack_list(p, seen=None):
    """assemblies that were ILMerged into p's original binary (transitively)"""
    seen = set() if seen is None else seen
    out = []
    info = plan[p]
    for t in info.get('merged_third', []):
        a = THIRD_ASM.get(t)
        if t == 'SevenZip' and info['area'].startswith('HAL'):
            a = None  # HAL's LZMA code lives in Redbox.HAL.Component.Model
        if a and a not in out:
            out.append(a)
    if p in org.KEEP_INLINE:
        return out
    for d in info.get('merged', []):
        if d in seen:
            continue
        seen.add(d)
        if d not in out:
            out.append(d)
        for x in repack_list(d, seen):
            if x not in out:
                out.append(x)
    return out

RESX_LOGICAL = {}
MOVES = {}
def resx_logical(p, f, info):
    # organize.py stripped the namespace prefix from root-level resx names; rebuild the original manifest name
    return RESX_LOGICAL[p][f]

if __name__ == '__main__':
    RESX_LOGICAL.update(json.load(open(os.path.join(S, 'resx_names.json'))))
    MOVES.update(json.load(open(os.path.join(S, 'moves.json'))))
    main()
