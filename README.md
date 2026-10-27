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

## Layout

```
Redbox.sln              all projects, grouped by product area
Directory.Build.props   shared compiler settings
lib/                    third-party binaries that have no NuGet package (see lib/README.md)
docs/                   assembly inventory and decompilation notes
src/
  Common/               shared Redbox.* libraries (Core, IPC, Lua, Macros, ...)
  HAL/                  hardware abstraction layer service (halservice) and its tools
    Legacy/             old 2014 camera plugins still shipped in halservice/bin
  KioskEngine/          kiosk engine host, IDE, control panel, FMA and shared UI
  KioskServices/        engine kernel extensions and service proxies
  Rental/               rental application (model, services, WPF UI)
  DeviceService/        card-reader device service (.NET Core 2.2) and RBA SDK
  KioskClient/          KioskClientService.API (.NET Core 2.2)
  UpdateClient/         UpdateClientService.API (.NET Core 2.2) and Redbox.NetCore.*
  UpdateManager/        legacy update manager daemon/UI/console and their libraries
  Tools/                KioskShell, kioskutil, inventoryimport
```

Each project folder is named after the assembly it builds. See
[docs/assemblies.md](docs/assemblies.md) for the mapping from every shipped binary to its
project.

## How the sources were produced

See [docs/decompilation.md](docs/decompilation.md) for details: tool versions, how
ILMerged executables were split back into their libraries, which code is kept inline,
and the list of known gaps.
