# Decompilation notes

## Source material

- Kiosk image folders: `halservice`, `HAL-Tools`, `MS HAL Tester`, `KioskShell`, `REDS/*`.
- 596 managed binaries were found; 241 unique assemblies. 100 are Redbox-authored and are
  decompiled here. Everything else is third-party (NuGet package or `lib/`).
- Where a binary exists in both `bin/` and `bin/.staged_/`, the `.staged_` copy was used.
  (The two copies have the same version number; the staged copy is the pending update.)

## Tooling

- ILSpy `ilspycmd` 11.0 (`ICSharpCode.Decompiler` 11.0.0.9375), whole-project mode with
  BAML decompilation:
  `ilspycmd -p --decompile-baml -r <all reference assemblies> -o <dir> <assembly>`
- The decompiled output was then reorganised by script (namespace folders flattened under each
  project's root namespace, ILMerged code split out, csproj files regenerated as SDK-style
  projects with ProjectReference/PackageReference). That reorganised output is the first
  commit of this repository; every later commit is a hand fix.

## ILMerged executables

Many executables were shipped as single files produced by ILMerge, containing copies of
other Redbox libraries and of third-party libraries. These copies are **not** duplicated in
the source tree:

- Merged copies of Redbox libraries that also ship standalone (for example `Redbox.Core`
  inside `updatemgrd.exe`) were removed; the executable references that library's project.
  The merged copy was compared type-by-type with the standalone DLL first.
- Merged third-party code (Newtonsoft.Json, log4net, SharpZipLib, DotNetZip, Jayrock,
  PowerCollections, the LZMA SDK) was removed and replaced with a NuGet package or a
  `lib/` reference.
- Libraries that **only** exist merged (no standalone DLL was shipped) were recovered into
  their own projects. Their original assembly names are not known; projects are named after
  their root namespace:

  | Project | Recovered from |
  |---|---|
  | `Redbox.DirectShow` | `Redbox.HAL.Script.Framework.dll` (also in MSHALTester, CameraTuner, the DirectShow plugins) |
  | `Redbox.HAL.Common.GUI.Functions` | `MSHALTester.exe` (also in CameraTuner) |
  | `Interop.ClearImage` | `Redbox.HAL.DataMatrix.Framework.dll` (COM interop for Inlite ClearImage) |
  | `Redbox.DeltaCompression` | `updatemgrd.exe`, `update-console.exe`, `installer-front-end.exe` |
  | `Redbox.UpdateManager.BITS` / `.Environment` / `.Kernel` / `.Remoting` / `.TaskScheduler` / `.ServiceProxies` | same |
  | `TaskScheduler` | same (third-party Windows Task Scheduler wrapper, no binary available) |

  ILMerge was run with `/internalize`, which turned these libraries' public types into
  `internal` ones. In the recovered projects top-level types were made `public` again so
  the executables can reference them.

- The HAL LZMA code is a custom build of the LZMA SDK (it adds `SevenZipHelper`) that was
  merged into `Redbox.HAL.Component.Model`; no standalone binary exists, so it is kept as
  source in that project.

### Kept inline

These older binaries merged in **older, incompatible** versions of Redbox libraries
(missing types, different signatures, or .NET 3.5 builds of libraries that now target
.NET 4.6). Their merged Redbox code is kept inside the project as-is:

- `halmgmtc` (2016, HAL 1.10 stack)
- `inventoryimport`, `kioskutil`, `redscc` (.NET 3.5)
- `Redbox.HAL.ClearimageV1`, `Redbox.HAL.DirectShow.Plugin` (2014 legacy plugins)

## Rebuilding the original binaries' shape

The projects build normal (un-merged) assemblies. Some deployments expect single-file
executables; see the ILRepack note in the README.

## Known gaps

See the "Known gaps" section at the end of the README (kept up to date with the fixes).
