# Redbox kiosk software — decompiled sources

Recompilable C# sources for the Redbox kiosk software stack, produced by decompiling the
shipped .NET binaries with ILSpy and restructuring the output into one Visual Studio
solution.

The goal is **fidelity**: code is kept as the decompiler produced it, apart from the
changes needed to compile it. Every hand edit is a separate git commit on top of the raw
decompiled baseline, so `git log` / `git diff` shows exactly what was changed and why.

## Building

Requirements:

- Visual Studio 2022 or later (or Build Tools) with the **.NET desktop development** workload.
  The full MSBuild is required (not just `dotnet build`): many projects target
  .NET Framework 3.5/4.x and embed WinForms `.resx` resources with images, which the
  .NET SDK's resource generator cannot handle for those targets.
- .NET Framework 3.5 enabled in Windows Features (for the HAL projects).
- Internet access for NuGet restore. The .NET Core 2.x targets are end-of-life;
  their reference packs still restore from nuget.org.

```bash
msbuild Redbox.sln -restore -m -p:Configuration=Release
```

Many of the shipped executables were single files produced by ILMerge. The projects here build
ordinary, separate assemblies; add `-p:ILRepack=true` to also produce merged copies (in a `merged\`
folder next to each affected output) using the assembly lists recorded in each project.

A few projects are kept as source only and are not in `Redbox.sln`, because the libraries they were
compiled against are not available; see "Known gaps" in
[docs/decompilation.md](docs/decompilation.md).

## Layout

```
Redbox.sln              all projects, grouped by product area
Directory.Build.props   shared compiler settings
lib/                    third-party binaries that have no NuGet package (see lib/README.md)
docs/                   assembly inventory, decompilation notes, update-store survey
  systems/              how each system works: protocols, data formats, configuration
build/                  ILRepack re-merge targets
tools/                  scripts that generated src/ and fixed decompiler output
tests/                  smoke test for the reconstructed RBA SDK simulator
src/
  Common/               shared Redbox.* libraries (Core, IPC, Lua, Macros, ...)
  HAL/                  hardware abstraction layer service (halservice) and its tools
    Legacy/             old 2014 camera plugins left in halservice's staging folders
  KioskEngine/          kiosk engine host, IDE, control panel, FMA and shared UI
  KioskServices/        engine kernel extensions and service proxies
  Rental/               rental application (model, services, WPF UI)
  DeviceService/        card-reader device service (.NET Core 2.2), RBA SDK and a reconstructed RBA SDK simulator
  KioskClient/          KioskClientService.API (.NET Core 2.2)
  UpdateClient/         UpdateClientService.API (.NET Core 2.2) and Redbox.NetCore.*
  UpdateManager/        legacy update manager daemon/UI/console and their libraries
  Tools/                KioskShell, kioskutil, inventoryimport
```

Each project folder is named after the assembly it builds. See
[docs/assemblies.md](docs/assemblies.md) for the mapping from every shipped binary to its
project.

## How the systems work

[docs/systems/](docs/systems/README.md) documents each system in depth: what it does, how it starts, its
protocols and data formats (for example the update service protocol and the HAL command protocol), its
configuration, and where in `src/` each part is implemented. Start with the overview in
[docs/systems/README.md](docs/systems/README.md).

## How the sources were produced

See [docs/decompilation.md](docs/decompilation.md) for details: tool versions, how
ILMerged executables were split back into their libraries, which code is kept inline,
and the list of known gaps.

## Reconstructed code (not decompiled)

A few pieces were written by hand because the original binary was never shipped. Each is clearly marked
in its source:

- [`RBA_SDK_Simulator`](src/DeviceService/RBA_SDK_Simulator/README.md): an iUC285 card-reader simulator that
  DeviceService loads when `AppSettings:UseSimulator` is true, so the device service runs without hardware.
  It has its own REST API for swiping, inserting and tapping test cards. `tests/RBA_SDK_Simulator.SmokeTest`
  runs the real `IUC285Proxy` against it.
- `Redbox.HardwareServices.TestProxy`: an API stub only.
- `RebootStatusRequest` in Status.Client.

## Version history

The kiosk image contains the update manager's full update history. [docs/update-store.md](docs/update-store.md)
describes its format and what it could recover: source history, and the older libraries the excluded projects need.
