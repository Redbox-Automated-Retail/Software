"""Make one project of a history release openable in Visual Studio, e.g. to see its views in the XAML designer.

The history/* branches hold decompiled source without project files. This copies one project folder of a
release (default: the rental UI) into <out>, exports that release's original assemblies into <out>/refs
(export_release.py) and writes an SDK-style csproj that builds the project alone against them. Only the copied
project is compiled; everything it uses comes from the original binaries of the same release, so code removed
since (Starz, the welcome screen, ...) is still there. Nothing is written into the repository.

Build it once (Visual Studio, or MSBuild with -restore) before opening a view: the designer needs the compiled
project.

Usage:
  python -I view_project.py --work <dir> --commit engine/Z --out <dir> [--project src/Rental/Redbox.Rental.UI]
"""
import argparse
import io
import os
import re
import shutil
import subprocess
import sys
import tarfile
from xml.sax.saxutils import escape

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from export_release import REPO, export  # noqa: E402

SOURCE_EXT = (".cs", ".xaml", ".resx", ".csproj", ".config", ".manifest", ".ico")
# framework assemblies the kiosk projects use; unused ones cost nothing
FX_REFS = ["Microsoft.CSharp", "ReachFramework", "System.ComponentModel.DataAnnotations", "System.Configuration",
           "System.Management", "System.Net.Http", "System.Runtime.Serialization", "System.Speech", "System.Web",
           "System.Web.Extensions", "System.Xml", "System.Xml.Linq", "WindowsFormsIntegration"]
TFM = re.compile(r'TargetFramework\("\.NETFramework,Version=v(\d)\.(\d)(?:\.(\d))?"')
# ILSpy writes "x is T { E: not null } v" for event null checks, which C# rejects for events (CS0154)
NOT_NULL_PATTERN = re.compile(r"\bis ([\w.]+) \{ (\w+): not null \} (\w+)")
# ILSpy drops the separators of MeshGeometry3D.TriangleIndices ("0 1 2 2 3 0" -> "012230"), which overflows Int32
# (MC2000). The same meshes are in the repository's own (fixed) XAML, matched by their Positions.
# App.xaml merges its style dictionary from "pack://Application:,,,Redbox.X;component/...", which WPF rejects
# ("The URI prefix is not recognized"), so the designer would show every view without its styles.
PACK_URI = re.compile(r"pack://application:,,,(?=[^/])", re.I)
MESH = re.compile(r'Positions="([^"]*)"([^>]*?) TriangleIndices="(\d*)"')


def mesh_indices():
    known = {}
    for f in tree_files(os.path.join(REPO, "src")):
        if f.endswith(".xaml"):
            text = open(os.path.join(REPO, "src", f), encoding="utf-8-sig").read()
            for m in re.finditer(r'Positions="([^"]*)"[^>]*? TriangleIndices="([\d ]+ [\d ]*)"', text):
                known[m.group(1)] = m.group(2)
    return known


def fix_mesh(text, known):
    def sub(m):
        idx = known.get(m.group(1))
        if idx:
            return f'Positions="{m.group(1)}"{m.group(2)} TriangleIndices="{idx}"'
        return f'Positions="{m.group(1)}"{m.group(2)}'  # unknown mesh: WPF falls back to sequential triangles
    return MESH.sub(lambda m: sub(m) if len(m.group(3)) > 1 else m.group(0), text)


def tree_files(root):
    for d, _, files in os.walk(root):
        for f in files:
            yield os.path.relpath(os.path.join(d, f), root)


def target_framework(proj):
    info = os.path.join(proj, "Properties", "AssemblyInfo.cs")
    m = TFM.search(open(info, encoding="utf-8").read()) if os.path.exists(info) else None
    return "net" + "".join(g for g in m.groups() if g) if m else "net472"


def main():
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("--work", required=True, help="work directory of rebuild_store.py (holds blobs/)")
    ap.add_argument("--commit", required=True, help="release commit or tag on a history/* branch, e.g. engine/Z")
    ap.add_argument("--out", required=True, help="new folder to write the project into")
    ap.add_argument("--project", default="src/Rental/Redbox.Rental.UI", help="project folder in the release")
    a = ap.parse_args()
    if os.path.exists(a.out) and os.listdir(a.out):
        sys.exit(f"{a.out} is not empty")

    name = os.path.basename(a.project.rstrip("/"))
    refs = os.path.join(a.out, "refs")
    assemblies = export(a.work, a.commit, refs)

    tar = subprocess.run(["git", "archive", "--format=tar", a.commit, a.project], cwd=REPO, capture_output=True,
                         check=True).stdout
    with tarfile.open(fileobj=io.BytesIO(tar)) as t:
        t.extractall(a.out, filter="data")
    proj = os.path.join(a.out, *a.project.split("/"))
    if not os.path.isdir(proj):
        sys.exit(f"{a.project} is not in {a.commit}")
    shutil.copyfile(os.path.join(REPO, "Directory.Build.props"), os.path.join(a.out, "Directory.Build.props"))

    files = sorted(tree_files(proj))
    known = None
    for f in files:
        if f.endswith((".cs", ".xaml")):
            path = os.path.join(proj, f)
            text = open(path, encoding="utf-8-sig").read()
            if f.endswith(".cs"):
                fixed = NOT_NULL_PATTERN.sub(r"is \1 \3 && \3.\2 != null", text)
            else:
                if known is None and "TriangleIndices" in text:
                    known = mesh_indices()
                fixed = PACK_URI.sub("pack://application:,,,/", text)
                fixed = fix_mesh(fixed, known) if known is not None else fixed
            if fixed != text:
                open(path, "w", encoding="utf-8").write(fixed)
    wpf = any(f.endswith(".xaml") for f in files)
    winexe = "App.xaml" in files
    winforms = any(f.endswith(".Designer.cs") for f in files)
    resources = [f for f in files if not f.lower().endswith(SOURCE_EXT)]

    out = ['<Project Sdk="Microsoft.NET.Sdk">', "  <PropertyGroup>",
           f"    <AssemblyName>{name}</AssemblyName>", f"    <RootNamespace>{name}</RootNamespace>",
           f"    <OutputType>{'WinExe' if winexe else 'Library'}</OutputType>",
           f"    <TargetFramework>{target_framework(proj)}</TargetFramework>",
           "    <PlatformTarget>x86</PlatformTarget>", "    <AllowUnsafeBlocks>True</AllowUnsafeBlocks>",
           "    <GenerateAssemblyInfo>false</GenerateAssemblyInfo>"]
    if wpf:
        out.append("    <UseWPF>True</UseWPF>")
    if winforms:
        out.append("    <UseWindowsForms>True</UseWindowsForms>")
    out.append("  </PropertyGroup>")
    if resources:
        out.append("  <ItemGroup>")
        for f in resources:
            out.append(f'    <None Remove="{escape(f)}" />')
            out.append(f'    <Resource Include="{escape(f)}" />')
        out.append("  </ItemGroup>")
    out.append("  <ItemGroup>")
    out += [f'    <Reference Include="{r}" />' for r in FX_REFS]
    # the release's own libraries; its executables stay out (they carry ILMerged copies of the libraries)
    for file, asm in sorted(assemblies.items()):
        if file.lower().endswith(".dll") and asm != name:
            out += [f'    <Reference Include="{escape(asm)}">',
                    f"      <HintPath>{escape(os.path.relpath(os.path.join(refs, file), proj))}</HintPath>",
                    "      <Private>True</Private>", "    </Reference>"]
    out += ["  </ItemGroup>", "</Project>", ""]
    csproj = os.path.join(proj, name + ".csproj")
    open(csproj, "w", encoding="utf-8").write("\n".join(out))
    print(f"{len(files)} files, {len(assemblies)} release assemblies -> {csproj}")


if __name__ == "__main__":
    main()
