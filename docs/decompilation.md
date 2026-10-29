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

The projects build normal (un-merged) assemblies. Projects whose original binary was ILMerged list the
merged assemblies in `<ILRepackAssemblies>`; building with `-p:ILRepack=true` re-merges them with ILRepack
into `bin/<config>/<tfm>/merged/` (see `build/ILRepack.targets`).

## Hand fixes after the baseline commit

All changes after the first commit are listed in `git log`. In summary:

- **Decompiler output that does not compile** (mechanical, scripts in `tools/fixes/`): tlbimp COM interop
  attributes and event providers, BAML-decompiled XAML markup extensions, XAML-compiler-generated members
  left in WPF code-behind, a mangled `_logger?.LogInformation(...)` call, property patterns on events,
  an ambiguous `ISet<>`.
- **BAML `Int32Collection`s**: ILSpy wrote `TriangleIndices` without separators (`"321103"`,
  `"012230654476…"`). They were re-split into the only valid quad index lists (`"3 2 1 1 0 3"`, and
  `0 1 2 2 3 0 6 5 4 4 7 6 10 9 8 8 11 10 14 13 12 12 15 14` for the 16-vertex mesh in
  `AnimatedDiscReturnViewUserControl.xaml`).
- **XAML file names**: two Rental.UI controls were renamed back to their original BAML paths
  (`Controls/WelcomeUserControl1/2.xaml`, classes `JoinedUserControl1/2`).
- **Missing referenced types**, taken from older versions in the kiosk's update store
  (`REDS/Update Manager/.store`) where possible:
  - `RebootStatus` (from `DeviceService.ComponentModel` 1.10.6), placed in a `MissingReferences/` folder
    of the project that needs it.
  - Reconstructed by hand (no original found): `RebootStatusRequest` (Status.Client) and the
    `Redbox.HardwareServices.TestProxy` stub project. These are marked `RECONSTRUCTED` in the source.
  - `Redbox.HardwareServices.KernelExtension` was re-decompiled with the TestProxy stub available so that
    ILSpy could resolve its types.
- **Project references**: `System.Xaml` added where WPF types are used without it being an original reference.
- **RBA_SDK_Simulator** (new project, `src/DeviceService/RBA_SDK_Simulator`): written from scratch to the
  contract `DeviceService.WebApi/Startup.cs` loads by reflection, because the original DLL was never shipped.
  See its README.

## Store installer (`src/Installer`)

Decompiled later from `Windows10-Store-Installer-3.1.0.48-setup.exe` with the same `ilspycmd`, after its PE
resources were dumped (see [systems/store-installer.md](systems/store-installer.md) §2.1). `host` follows the
"kept inline" rule above. The other projects were laid out like the rest of the tree. Retargeting and the build
fixes are in their own commit; see store-installer.md "Building". Hard-coded account passwords in `KioskAdmin`
were replaced with `<redacted>` before the first commit.

## Known gaps

Not part of `Redbox.sln` (they need libraries other than the 1.88 ones in `src/`), but buildable:

- `src/HAL/Legacy/*`: the 2014 camera plugins (1.3.0.119), built with their own solution
  `src/HAL/Legacy/HAL.Legacy.sln` against HAL Core / IPC Framework **1.3.0.84** from the update-store history
  (1.3.0.119 itself is not recoverable). See `src/HAL/Legacy/README.md` for the changes.
- `Redbox.KioskServices.Kernel`, `.KernelExtension`, `.Proxy` (1.81.0.27): built against the original engine
  **1.81.0.27** (VegaSR2) assemblies, which `Redbox.KioskEngine.ComponentModel`'s `IConfiguration` and several
  moved types require (`SendKioskPropertiesToKioskDataService`, ...; the shipped 1.88 removed them). Nothing in
  the 1.88 engine references these assemblies. The references come from a folder passed as
  `RedboxEngine181Refs` (`src/KioskServices/Engine181.targets`), exported from the rebuilt update store:

  ```
  python -I tools/history/rebuild_store.py ...                # once, see docs/version-history.md
  python -I tools/history/export_release.py --work <work> --commit <history/engine VegaSR2 commit> --out <refs>
  msbuild src/KioskServices/Redbox.KioskServices.KernelExtension/Redbox.KioskServices.KernelExtension.csproj -restore -p:RedboxEngine181Refs=<refs>
  ```

  With the real 1.81 `Redbox.KioskEngine.ComponentModel`, the `IPromotionServices` stand-in that
  `Redbox.KioskServices.Proxy` used to carry is gone, and one `ref` argument ILSpy had guessed is an `out`.

Other notes:

- Recovered-library assembly names (`Redbox.DirectShow`, `Redbox.UpdateManager.*`, `TaskScheduler`,
  `Interop.ClearImage`, ...) are guesses based on namespaces; their version is 1.0.0.0.
- `Redbox.UpdateService.Client` in `REDS/Update Manager/bin` is an ILMerged build of
  `Redbox.UpdateService.Client` + `Redbox.UpdateService.Model`; it is not reproduced separately.
- `DeviceService.ComponentModel` ships as 1.10.13.0 (DeviceService) and 1.0.0.0 (Kiosk Engine) with
  identical types; only 1.10.13.0 is built.
