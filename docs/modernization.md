# Modernization plan (`modernize` branch)

`main` stays the faithful decompile. This branch is a long-lived fork that:

1. moves the code to current .NET where that is feasible, and to .NET Framework 4.8 where it is not;
2. makes the decompiled code readable without changing what it does;
3. fixes what breaks on Windows 10/11;
4. talks to the self-hosted backend through configuration instead of Redbox's hosts.

It is never merged into `main`. Every step keeps `Redbox.sln` building and is one commit (or a short series of
commits) per coherent change, so `git log main..modernize` reads as a list of the changes.

## Linux as a goal (decided 2026-10-31)

This is a port, not a rewrite: the robot scripts, timings, payment flows and protocols are kept. Linux support is
built in along the way:

- Code that is Windows-only (registry, Windows services, BITS, COM, DirectShow, WMI, Shell Launcher, Task
  Scheduler) goes behind interfaces as each component is ported, with the Windows implementation unchanged.
- Non-UI components target `net10.0` with a Windows-specific implementation assembly where needed, so they also
  build and run on Linux.

| Component | Linux path |
|---|---|
| Kiosk Client | `net10.0`; already almost platform-neutral |
| Update Client | `net10.0` core; the Windows service host, BITS and PowerShell jobs behind interfaces (systemd, HTTP download, `pwsh`/shell) |
| HAL core (serial, scripts, jobs, storage) | `net10.0` + SQLite; serial port names come from configuration |
| HAL camera / barcode | `ICameraPlugin` already exists. DirectShow + ClearImage stay for Windows, and V4L2 + ZXing/libdmtx are added for Linux. |
| Device Service | depends on a Linux build of the Ingenico RBA SDK (to be checked); the simulator runs anywhere |
| Update Manager | `net10.0` core. The Lua `kernel.*` functions that touch Windows (registry, services, shell) get Linux equivalents or report "not supported". |
| Engine host + Rental / Control Panel / FMA UI | WinForms + WPF are Windows-only, so these move to **Avalonia**. This is the only real rewrite, and it comes last. Until then the engine runs on `net10.0-windows`. |
| Kiosk shell | KioskShell on Windows; a Wayland kiosk compositor (e.g. `cage`) plus a systemd unit on Linux |

The Windows 10/11 fixes below still matter: kiosks run Windows until the UI exists on Linux.

## Ground rules

- **Wire compatibility comes first.** The following stay byte-for-byte compatible with the existing backend
  (`Redbox-Automated-Retail/Backend`) and with unported components still on kiosks:
  - the `rcp://` and `srcp://` framing and command set;
  - the HAL protocol;
  - update-store formats and the `u p` / `u pr` / `installer *` commands;
  - MQTT topics and payloads;
  - broker registration (`Reserve3`, ...);
  - the KioskServices `IMessageService.Process(json)` call;
  - every REST/SignalR payload between the engine, Device Service, Kiosk Client and Update Client;
  - JSON shapes, including `JavaScriptSerializer` dates (`"\/Date(...)\/"`) and Newtonsoft settings.

  Where a serializer or transport is replaced, a golden test compares the old and new output.
- **Behavior stays identical** except where a change is listed here as a Windows 10/11 fix or as a
  configuration change. A real bug found along the way is fixed in its own commit and listed under that
  component.
- **Existing kiosk configuration keeps working.** Today's `appsettings*.json`, `*.exe.config`, registry values and
  `data\*.config` files are read as they are, including the Serilog sink names. New settings are optional.
- **No Redbox hosts or personal hostnames as defaults.** No secrets in the repo (see "Endpoints and secrets").

## Target frameworks

**.NET 10 (LTS, supported to November 2028)** is the target, not .NET 8: .NET 8 support ends on 2026-11-10.
Windows-only projects use `net10.0-windows`, and services that load x86 native code stay x86.

| Today | Moves to | Why |
|---|---|---|
| .NET Core 2.1/2.2 (Device Service, Kiosk Client, Update Client) | `net10.0` / `net10.0-windows`, x86 where needed | ASP.NET Core 2.2 → 10 is a well-trodden path; these projects don't reference the shared Redbox libraries |
| .NET Framework 3.5 (HAL, tools) | **4.8** first, then `net10.0-windows` (done for the HAL service folder; `halmgmtc`, MSHALTester and HalUtilities are still 4.8) | .NET 3.5 is the main thing that breaks on Windows 11 (an optional feature; Windows Sandbox can't enable it). 4.8 ships in Windows 10 1903+ and 11. It loads the same 2.0-era DLLs (VistaDB 3.5, log4net 1.2.9) unchanged. |
| .NET Framework 4.0–4.7.2 (engine, Update Manager, installer) | **4.8** first; .NET 10 where the blockers below are solved | Targeting 4.7+ alone fixes the TLS 1.0 default of `SslStream`/`HttpWebRequest`. |
| `netstandard2.0` libraries (Device Service model, RBA SDK) | stay `netstandard2.0` | the 4.8 engine and the .NET 10 services both use them |
| Shared `Redbox.*` libraries (`net46`) | `net48;net10.0-windows` multi-target, once a .NET 10 consumer needs them | |

### What keeps a component on 4.8 (for now)

| Blocker | Used by | Way off |
|---|---|---|
| .NET Remoting over GenuineChannels `gtcp://` (legacy reservation broker, the 1.88 default `UseLegacyBrokerService = true`) | engine (`Redbox.BrokerServices.Proxy`) | a minimal MS-NRTP/MS-NRBF client for the few broker calls, checked against `rb-remoting` (Mono, the kiosk's own GenuineChannels.dll); or switch the engine to the HTTP broker path (`UseAWSBrokerService`) if the backend supports it |
| .NET Remoting `HttpChannel` + binary formatter (KioskServices) | engine (`Redbox.KioskServices.Proxy`) | there is one call, `IMessageService.Process(string) -> string`. Use the existing `KioskServicesProtocol = REST` mode, which the backend serves, or a hand-written NRBF request for the one method. **Done:** the hand-written request (`Redbox.Remoting.Nrbf.RemotingHttpClient`) |
| VistaDB 3.5 (`VistaDB.NET20.dll`, 2008, commercial, no successor we can ship) | engine (`Redbox.KioskEngine.Environment`, `Redbox.Rental.Data`), HAL (`Redbox.HAL.DataStorage`) | spike: load it on .NET 10. If that fails, migrate to SQLite with a one-time importer, a larger change that needs sign-off |
| ASP.NET Web API 5 self-host (WCF underneath) | engine (`Redbox.KioskEngine.API`, `:9002`) | Kestrel minimal host with the same routes and Newtonsoft settings |
| Engine plug-ins compiled against the engine (kernel extensions, Rental bundles) and Lua scripts that call .NET types | engine | the plug-ins are built from this repo, so they move together; the Lua scripts in bundles have to keep finding the same types and members |
| IDE-only WinForms controls (Puzzle.SyntaxBox, WeifenLuo 2.5, SmartPropertyGrid, VisualTips) | `Redbox.KioskEngine.IDE` (developer tool inside the engine) | newer WeifenLuo/DockPanelSuite exists; SyntaxBox does not. Option: build the IDE only for 4.8 and leave it out of the .NET 10 engine. **Done:** SyntaxBox replaced by Scintilla (`CodeEditor`). WeifenLuo 2.5, SmartPropertyGrid and VisualTips are unchanged; the .NET 10 engine starts with them, but the IDE main window has not been opened on .NET 10 yet |
| `Microsoft.Vsa` / JScript page engine | setup host (RIX) | already solved for 4.8 in Backend PR #5 (`jsc` / `JScriptCodeProvider`); no .NET 10 equivalent, so the host stays on 4.8 |
| `System.Web.Extensions` `JavaScriptSerializer` | `Redbox.Core` and everything on it | a compatible serializer for the .NET 10 builds, with golden tests against the 4.8 output |
| Quartz.NET 1.0.2.3 / 2.3.3 | engine, Update Manager | Quartz 3.x has a different API; needed only for a .NET 10 move |
| COM: BITS, Task Scheduler 1.0, Windows Firewall, ClearImage, DirectShow | Update Manager, Update Client, engine, HAL | all work on .NET 10 on Windows (`[ComImport]`), so they aren't blockers, only things to test |

## Order of work

Ranked by value on the main kiosk path: what is broken on Windows 10/11 or end-of-life today, weighted by how
central it is. The engine is the most central component, but it already runs on Windows 10/11 on 4.7.2 and is
by far the riskiest to move to .NET 10, so its .NET 10 move comes last.

| # | Component | Target | Value | Risk |
|---|---|---|---|---|
| 0 | Build plumbing | — | `ArtifactsPath` opt-in so output can go outside the tree; shared settings for modernized projects; a `tests/` project per ported component | low |
| 1 | **HAL** (`src/HAL`, incl. `HAL.PackageInstaller`, `halsvc-win32`) | 4.8 x86 | **High:** without .NET 3.5 no HAL service is installed or runs (store-installer.md §11) | low–medium: serial/COM/DirectShow timing, VistaDB on the 4 CLR, log4net 1.2.9 |
| 2 | **Windows 10/11 fixes in the Update Manager kernel and KioskShell** (shell replacement, registry views, wmic) | 4.8 | **High:** the shell stays `explorer.exe` on Windows 11 | low |
| 3 | **Update Client** | `net10.0-windows` x86 | **High:** .NET Core 2.2 is EOL and needs the 2.2.1 runtimes (8 min of the install); MQTT TLS; IoT endpoints | medium: MQTTnet 3 on .NET 10, BITS COM, Coravel, App.Metrics/AWSSDK leftovers |
| 4 | **Kiosk Client** | `net10.0` | **High:** same EOL runtime; it is the gateway for every cloud call | medium: 596 files, Newtonsoft output must stay identical, Coravel 4 |
| 5 | **Device Service** (+ `RBAInstaller`, `Status.Client`, `BluefinService.Client`) | `net10.0-windows` x86 | **High:** same EOL runtime; it takes payments | medium: the RBA SDK native DLL (x86), System.IO.Ports, SignalR protocol compatibility with the engine's 1.1 client |
| 6 | **Update Manager** (`updatemgrd`, `update-console`, `updatemgr-ui`, libraries) | 4.8 (then .NET 10 assessed) | medium: TLS defaults, `wmic`, endpoint defaults | low on 4.8. On .NET 10 the blockers are Quartz 2, `JavaScriptSerializer` and the Lua x86 DLL. It references Remoting but does not use it. |
| 7 | **Kiosk Engine** (+ KioskServices kernel extensions, Rental) | 4.8 now, with Windows fixes and endpoint defaults; .NET 10 after the spikes above | medium on 4.8 (it works today); high on .NET 10 for maintainability | high: see blockers |
| 8 | **Installer pieces**: the setup host (bring over Backend PR #5's 4.8 + `jsc` overlay), `installer-front-end`, `KioskAdmin`, NAnt tasks/TimeKeeper | 4.8 | medium: the Backend's PowerShell installer already provisions working kiosks | low |
| 9 | Tools: `KioskShell`, `kioskutil`, `inventoryimport`, `redscc`, HAL tools | 4.8 or .NET 10 per tool | low | low |

Readability work (below) happens inside each component's port, not as a separate repo-wide pass.

## Per component

### 1. HAL → 4.8

- Retarget all `src/HAL` projects (and `src/HAL/Legacy`) from `net35`/`net40` to `net48`, keeping x86.
  `HAL.PackageInstaller` gets a 4.8 build, so step 7 of the installer works without .NET 3.5.
- `app.config`: `supportedRuntime v4.0`, and the 4.x `useLegacyV2RuntimeActivationPolicy` where mixed-mode
  DLLs need it.
- TLS: `Redbox.HAL.IPC.Framework` server sessions hard-code `SslProtocols.Tls`. Change them to `None` (the OS
  default, TLS 1.2/1.3) when the host is set to encrypt; the default (plain) is unchanged.
- Fail clearly on a 0-deck `hal.xml` instead of dying in `DecksManager.get_First` and leaving a service that
  can't be stopped. This is a bug fix, in its own commit.
- **Risks:** VistaDB 3.5 under the 4.x CLR (expected to work: 4.x runs 2.0 assemblies in-process); `log4net`
  1.2.9 → keep the binary; ClearImage COM registration; timing-sensitive serial code (no behavior change
  intended, but verify with the hardware emulator).
- **Verify:** Backend `tests/clients/sandbox` (com0com + hardware emulator + `30-halsvc.ps1`) in Windows
  Sandbox *without* .NET 3.5; HAL `halmgmtc`/`MSHALTester` against the emulator.

### 2. Windows 10/11 fixes (Update Manager kernel, KioskShell, engine)

- **Shell replacement without `wmic`:** `ShellLauncherFunctions` calls the same `WESL_UserSetting` methods
  through `System.Management` (`root\StandardCimv2\Embedded`), with the same arguments and return value to Lua.
- **Shell Launcher on Pro editions:** Shell Launcher exists only on Enterprise/Education/IoT. When it is missing,
  fall back to a per-user `Winlogon\Shell` value for the kiosk user. Only `replaceshellwith` uses the fallback,
  and it is logged. *Decision needed (see below).*
- `Windows10TouchScreen` (engine): `Win32_PnPEntity.Enable/Disable` via `System.Management` instead of `wmic`.
- **Registry views:** one helper opens `HKLM\SOFTWARE\Redbox` explicitly in the 32-bit view
  (`RegistryView.Registry32`), which is where the x86 engine, updatemgrd, the services and `rb-reprovision`
  read it. Every component and tool that touches `SOFTWARE\Redbox` uses that helper, whatever its bitness.
  This fixes the AnyCPU setup host's `MaxSlots` landing in the 64-bit view and the "kiosk id 0" seen by the
  .NET services on hosts that only have the 64-bit key.
- **TLS 1.2+:** retargeting to 4.8 makes `SslStream.AuthenticateAsClient(host)` and `HttpWebRequest` use the
  OS defaults. The `ServicePointManager.SecurityProtocol = Tls12` pins are replaced by `SystemDefault`, which
  allows TLS 1.3 and keeps 1.2.
- **.NET 3.5:** nothing on the branch needs it after steps 1, 6 and 9.

### 3–5. Update Client, Kiosk Client, Device Service → .NET 10

Common to the three:

- `Microsoft.NET.Sdk.Web`, `net10.0(-windows)`, generic host with `UseWindowsService()` (same service names:
  `updateclient$service`, `kioskclient$service`, `device$service`), and Kestrel on the same URLs and ports.
- **JSON stays Newtonsoft** (`AddNewtonsoftJson` with the 2.2 defaults: camelCase, `DateTimeZoneHandling`,
  null handling as before). A golden test records the 2.2 responses of representative endpoints and compares.
- **Serilog:** current packages, and the `RollingFile` sink stays registered (or is mapped to `File`) so existing
  `appsettings*.json` keep logging.
- Deployed **self-contained x86** (`win-x86`), so the kiosk needs no .NET runtime installed. This removes the
  2.2.1 runtime downloads from provisioning. It changes the release zip size (~70 MB), not the paths: the exe
  names in `bin\` stay the same.
- Swashbuckle 6/7, Coravel current, `Nito.AsyncEx` current. Drop unused EF Core 2.2 / App.Metrics / AWSSDK
  references if nothing uses them at runtime. If something does, port it.

Update Client specifics:

- MQTTnet 3.1.2 runs on .NET 10, so stay on 3.x first (no wire change). Later, MQTTnet 4/5 in its own commit.
- TLS for MQTT: `SslProtocols.None` (OS default) instead of a pinned `Tls12`, and explicit revocation handling
  (`RevocationMode`), configurable so it works with backends that have no CRL. The backend now serves a CRL,
  so the default stays "check".
- BITS (`BG_BASIC_CREDENTIALS`, ...) COM interop: unchanged, tested on .NET 10 x86.
- PowerShell endpoints and the `:60669` listener: unchanged in the port. Whether `:60669` is reachable from the
  network is checked on .NET 10. If it is, restricting it to localhost is proposed as its own change.

Kiosk Client specifics:

- The configuration it decrypts (`configuration.json`) uses the shared TripleDES/passphrase obfuscation (see
  "Endpoints and secrets"). Keep it bit-compatible, including `Encoding.Default`: that is UTF-8 on .NET Core
  2.2 and stays UTF-8 on .NET 10.

Device Service specifics:

- x86 (RBA SDK native `RBA_SDK.dll`), System.IO.Ports current.
- SignalR hub `/CardReaderEvents`: the engine's client is ASP.NET Core SignalR 1.1 on 4.7.2. Test that it
  negotiates with the .NET 10 server (JSON hub protocol v1, negotiate v0). If it doesn't, the engine side's
  client package is upgraded in the same step.
- `DeviceService.Client` (consumed by the engine) becomes `netstandard2.0;net48` instead of `net461`.
- `RBA_SDK_Simulator` and `tests/RBA_SDK_Simulator.SmokeTest` keep working (smoke test is already .NET 8 → 10).

### 6. Update Manager → 4.8

- Retarget the `src/UpdateManager` projects to `net48` x86. Drop the unused `System.Runtime.Remoting` reference.
- Defaults: `updatemgrd` `Settings` (`Url = http://bundle-dev-02:8002`, a Redbox dev box) and
  `installer-front-end` / `update-console` `UpdateServiceUrl` become unset; see "Endpoints and secrets".
- `srcp://` client: OS-default TLS; the certificate validation callback is unchanged (it validates against the
  installed CA, as `rb-reprovision` sets up).
- Keep `xdelta3.exe` 3.0u (the backend encodes with `-S none` for it). Paths over 260 characters are a known
  limit of that exe.
- **Verify:** Backend `tests/clients/run-updatemgr-harness.ps1` and `sandbox/updatemgr-round.ps1` against
  `rb-updatesvc`; seed activation with `update-console --eval`.

### 7. Kiosk Engine (+ KioskServices, Rental)

Now (4.8, no runtime change):

- `net472` → `net48`; the `Tls12` pin → OS default; `Windows10TouchScreen` without `wmic`; registry helper.
- Endpoint defaults removed (`KioskServicesURL`, `BrokerServicesURL`); unset means "not configured", which the
  proxies already handle (`ExecuteMessage` returns `Success = false`, broker registration is skipped and
  logged).
- `KioskServicesProxy` REST mode: `ContentLength` is the byte count of the UTF-8 body, not the string length
  (bug fix: any non-ASCII payload was truncated).
- The public `redbox.com` links in the Rental UI (the title QR codes, `TitleProduct.cs`,
  `TitleDetailsWatchOptionsPopupViewController.cs`) come from configuration. When unset, the QR code is not
  shown.

Later (.NET 10, after spikes, each a go/no-go):

1. VistaDB 3.5 on .NET 10.
2. Broker client without GenuineChannels against `rb-remoting` (`tests/clients/BrokerHarness*.cs`), or the
   HTTP broker path.
3. KioskServices over REST only, or a one-method NRBF client (`KioskServicesHarness`, all 33 MessageTypes).
4. Kestrel replacing Web API self-host on `:9002`, with route parity checked by a test that lists routes.
5. `JavaScriptSerializer` replacement with golden tests.

**Decided (2026-10-31): the engine goes to .NET 10, and VistaDB is replaced by SQLite.** The 4.8 step above stays
as an intermediate state. The options for each blocker, with the recommended one first:

- **Legacy broker (GenuineChannels `gtcp://`).** The surface is small:
  - the kiosk calls `IKioskRegister.Register(int, IKioskOperations self, string)`, passing itself as an `ObjRef`,
    and `IsRegistered`;
  - the broker calls back over the same persistent connection: `Reserve3(ReservationCart3)`, `Ping` and
    `CancelReservation(long)`, plus the stubs `Reserve`/`Reserve2`;
  - the payloads are `[Serializable]` POCOs from `KioskShared` and `List<T>`.

  Options:
  1. *(recommended)* A clean .NET 10 client for exactly this surface:
     - GTCP framing written from GenuineChannels' MIT-licensed source (2.5.9.9+, the same 2.5.9.x line as the
       kiosk's 2.5.9.5);
     - an MS-NRBF reader (`System.Formats.Nrbf`, in-box since .NET 9) and a small writer for the method-call and
       return records;
     - the `ObjRef` for the callback object;
     - GenuineChannels' ping and reconnect behavior.

     It is tested against `rb-remoting`, which runs the kiosk's real GenuineChannels.dll, with traffic captured
     from the 4.8 engine as golden files.
  2. Port GenuineChannels itself. It is built on `System.Runtime.Remoting` (sinks, `RealProxy`, contexts),
     which .NET 10 doesn't have, so this would mean writing a Remoting runtime first. Much larger, for no gain
     on a 5-method interface.
  3. A small 4.8 "broker bridge" process that hosts today's proxy and talks to the engine over localhost. It is
     quick, but keeps a .NET Framework process on the kiosk. It is kept only as a fallback.
  4. The HTTP broker path (`UseAWSBrokerService`, through the Update Client). The backend doesn't implement it
     today, so this needs backend work, and it doesn't serve the 1.88 default.
- **KioskServices (Remoting `HttpChannel`).**
  1. *(recommended)* Make REST the .NET 10 engine's default, since the backend serves it. Keep `Remoting` mode
     with the same NRBF code as the broker: one `Process(string) -> string` call over HTTP. Both modes are
     checked with the 33-case `KioskServicesHarness`.
  2. REST only, dropping the `Remoting` setting.
- **VistaDB → SQLite** (`Microsoft.Data.Sqlite`). It is used by:
  - engine `DataStoreProvider`/`DataCacheService`/`QueueServiceVistaDBData`/`SchemaMigrator`;
  - Rental `LocalData`/`ProfileData`;
  - HAL `Redbox.HAL.DataStorage`, which holds the inventory, counters and stats tables.

  The work:
  1. Map each table and the SQL dialect used (`VistaDBStringHelper`) to SQLite.
  2. One-time migration of existing `.vdb3` files on first start. The options for reading the old files:
     1. *(recommended)* a small `net48` `vistadb-export` tool, run once by the update package, that dumps each
        table with the shipped VistaDB.NET20;
     2. loading VistaDB.NET20 into .NET 10 just for the import, if it turns out to load;
     3. reading the file format directly (no documentation).
  3. The old files are kept as backups.

  HAL moves to SQLite in the same way when it goes to .NET 10.
- **Web API self-host on `:9002`.** ASP.NET Core inside the engine process (`FrameworkReference
  Microsoft.AspNetCore.App`), with the same routes, the Newtonsoft formatter and Swashbuckle. A route-list test
  compares the two builds.
- **`JavaScriptSerializer`.** Port Microsoft's MIT-licensed reference source of
  `System.Web.Script.Serialization` into `Redbox.Core`. It behaves the same by construction, and a golden test
  compares it with the 4.8 output.
- **BinaryFormatter.** The HAL script framework (`ExecutionContext`, `ExecutionEngine`) persists script state
  with it, and the Task Scheduler wrappers use it too. Read the existing data with `System.Formats.Nrbf`, and
  write new data in a documented format (JSON) with a version marker.
- **Quartz 1.0.2.3.** Only `SchedulerService` uses it. Port to Quartz.NET 3.x with the same cron expressions and
  job semantics, or use a small built-in scheduler if it uses only simple triggers.
- **IDE controls.**
  - WeifenLuo 2.5 → DockPanelSuite 3.1 (supports .NET Core).
  - Try loading Puzzle.SyntaxBox, SmartPropertyGrid and VisualTips on .NET 10 (WinForms 2.0-era DLLs often
    load). Replace any that don't: AvalonEdit or Scintilla.NET for the editor, the built-in `PropertyGrid`.
- **Smaller items:**
  - `System.Speech`, `System.DirectoryServices` and `System.Management`: NuGet packages.
  - `InputSimulator`: `SendInput` P/Invoke.
  - Windows Firewall COM interop: unchanged.
  - Lua: the x86 `lua51.dll`, unchanged.

### 8. Installer pieces

- Bring the Backend PR #5 overlay of the setup host into `src/Installer/host` here: 4.8, `JScriptCodeProvider`
  instead of VSA, and the Divelements resource workaround. The overlay in the Backend then becomes a plain
  build of this branch.
- `KioskAdmin`, NAnt tasks, `Redbox.TimeKeeper*`: 4.8. Passwords stay out of the code (they are already
  `<redacted>`); they become parameters.
- `installer-front-end`: 4.8 x86, and `UpdateServiceUrl` from configuration.

## VistaDB → SQLite

The .NET 10 builds use SQLite (`Microsoft.Data.Sqlite`) wherever the .NET Framework builds use VistaDB 3.5; the
net48 builds keep VistaDB unchanged.

| VistaDB file (kiosk) | Used by | SQLite file | Kind |
|---|---|---|---|
| `Kiosk Engine\bin\message.data` | engine message queue | `message.sqlite` | written by the component: converted once |
| `Kiosk Engine\data\<store>.data` (`Local.data`, `cache.data`, ...) | engine data stores (Lua, `LocalData`) | `<store>.sqlite` | written: converted once |
| `Kiosk Engine\data\profile.data` | Rental `ProfileData`, engine `DataCacheService` | `profile.sqlite` | delivered by the update store: converted again whenever it changes |
| `halservice\bin\data\HALData.vdb3`, `HALCounters.vdb3` | HAL tables (`Redbox.HAL.DataStorage`) | `HALData.sqlite`, `HALCounters.sqlite` | written: converted once |

`inventory.data` is a product lookup archive, not VistaDB, and is read as before. `session.data` is only deleted at
start-up.

**Format.** Same tables, columns and rows:
- Text columns are `COLLATE NOCASE` when the VistaDB database was case-insensitive (all except `profile.data`).
- Dates are text, `yyyy-MM-dd HH:mm:ss[.fffffff]`, so SQL comparisons on them still work.
- Decimals are exact text; binary data is BLOB.
- Primary keys are kept. An integer identity key becomes `INTEGER PRIMARY KEY AUTOINCREMENT` and continues from VistaDB's seed.
- Other indexes are named `<table>_<index>`, because SQLite index names are per database.

**Migration.** `src/Tools/vistadb-to-sqlite` (.NET Framework 4.8 x86, which is part of Windows 10/11; VistaDB only
runs there) converts one file: `vistadb-to-sqlite <source> <destination.sqlite> [--overwrite]`. The .NET 10 components
call it themselves through `Redbox.Data.Sqlite.LegacyDatabase` when they find a VistaDB file:
- **Written files** are converted once. The VistaDB file is then renamed to `<name>.vistadb-backup`, and SQLite is the database from then on.
- **`profile.data`** stays where the update store puts it. A stamp table in `profile.sqlite` records the source's name, size and time, so a new profile is converted again.
- **Fresh installs** have no old files, and the SQLite files are created as needed.

The engine ships the tool in a `vistadb-to-sqlite` folder (`build/VistaDbToSqlite.targets`); HAL will do the same
when it moves to .NET 10. The `REDBOX_VISTADB_TO_SQLITE` environment variable can point elsewhere. A failed
conversion throws instead of starting an empty database. A 4.8 build re-deployed after a .NET 10 build would not see
the data written since (the VistaDB files are backups by then).

**Verified.** `tests/Redbox.Data.SqliteParity` (`compare-sqlite-parity.ps1 -Samples <copies of kiosk files>`) runs
the same operations through both builds and compares 119 results:
- the engine data stores, including key order and case-insensitive keys;
- the message queue;
- every HAL table;
- with the kiosk image's files: `Local`/`cache` contents, the queue, HAL inventory, counters and options, and Rental's
  profile load (234 markets, 11,655 titles, 76,172 stores, 10,674 extracted images) and `DataCacheService`.

Result: identical. Converting the image's 330 MB `profile.data` takes 17 s. Not yet run on a kiosk or in the Sandbox
(the .NET 10 engine has not been run there yet).

Kept from the decompiled code as it was: `KioskFunctionCheckTable`'s `UPDATE` has no commas between its `SET` items
and fails on both databases. Not carried over: VistaDB `GetAll`/`GetKeys` looped forever on a row with an empty key or
value; SQLite skips such rows.

## Endpoints and secrets

Today's hard-coded or shipped defaults and what they become:

| Where | Today | On this branch |
|---|---|---|
| `KioskServicesProxy.DefaultKioskServicesUrl` | `https://bl2.accessredbox.net/KioskServices/MessageService.rem` | none: `KioskServicesURL` must be set (registry, by `rb-reprovision`) |
| `BrokerServicesProxy` default | `gtcp://blt.accessredbox.net:7890/RegistrationBroker/Default.rem` | none: `BrokerServicesURL` must be set |
| `updatemgrd` `Settings.Url` | `http://bundle-dev-02:8002` | none |
| `installer-front-end` / `update-console` / `updatemgrd` `UpdateServiceUrl` (shipped `.exe.config`) | `srcp://updatesvc.redbox.com:7002` | the configs this branch ships carry an empty value. `rb-reprovision` / the installer's profile fill it in. |
| Update Client `appsettings*.json` (not in this repo; shipped by the update store) | `iotcertificateservice.kiosk.prod.redbox.com`, `proxyservice...`, AWS IoT endpoint | the fork ships `appsettings.json` templates with every key present and empty values, because `rb-reprovision` only replaces keys that exist |
| Kiosk Client `appsettings.json` | `proxyservice.kiosk.prod.redbox.com` | same as above |
| Device Service `Data\KioskData.json` `ApiUrl` | `dataservice.kiosk.prod.redbox.com` | empty template |
| Rental UI QR links | `https://www.redbox.com/...` | configuration (`system.config`), unset hides them |

The keys are the ones `redbox_backend/kioskprofile.py` already emits, so the existing kiosk profile and
`rb-reprovision` configure the fork without change. Unset values never fall back to Redbox's hosts. Each
component logs one clear "`<key>` is not configured" line and stays offline for that service, as it does today
when the backend is unreachable.

**Secrets:** there are none in `src` (keys, passwords and API keys from the image are already `<redacted>` /
`*** Removed ***`). What remains in code are the fixed obfuscation keys:

- the TripleDES key/IV in `Redbox.Core.ByteArrayExtensions`, also used by `rb-reprovision` for `usdata.staged`;
- the GUID-derived key in `Redbox.NetCore.Middleware.EncryptionHelper`;
- the Kiosk/Update Client passphrase.

The backend and existing kiosk data depend on them, so they stay as the default but can be overridden by
configuration. They are obfuscation, not protection (docs/systems/README.md), and the docs say so.

## Readability

The decompile is already fairly clean: 35 `//IL_xxxx` lines in 6 files and 24 "Unknown result type"
comments. The work is mostly elsewhere:

- **Mechanical, per component as it is ported:** remove the IL/unknown-type comments and redundant casts
  (`((IDisposable)(object)x)`, `((SymmetricAlgorithm)(object)p)`); file-scoped namespaces (already);
  `using` declarations; pattern matching. These are done with `dotnet format` / Roslyn code fixes and checked by
  build + tests, one commit per kind of fix.
- **Decompiler artifacts:** hand-written `<>c__DisplayClass` / `<>9__` / `[CompilerGenerated]` leftovers
  (42 places) are collapsed back to lambdas and local functions. State machines decompiled as `MoveNext`
  classes are turned back into `async`/iterators.
- **Names:** decompiler locals (`text2`, `dictionary3`, `num`, `flag`) are renamed by hand, prioritized on the
  main paths: IPC framing, update protocol, HAL jobs, payment flow, broker, configuration. A repo-wide rename of
  7,224 files by hand is out of scope; tooling can't infer meaning.
- **Nullable:** `<Nullable>enable</Nullable>` per ported project (not globally), annotate public surfaces, fix real
  null bugs, and `!` only where the decompiled flow proves non-null. Warnings stay visible (no blanket
  `NoWarn` for nullable codes in modernized projects).

## Verification

| What | How |
|---|---|
| Build | `msbuild Redbox.sln -restore -m -p:Configuration=Release` after every step (output via `-p:ArtifactsPath=...` outside the tree) |
| Golden tests | new `tests/` projects per component: JSON/serializer parity, IPC framing, config-key presence |
| Backend client harnesses | Backend `tests/clients/*` (broker, KioskServices 33 cases, update manager, UCS) built against this branch's DLLs, against the backend under WSLC (`rb.ps1 up`) |
| Kiosk end-to-end | Backend `tests/clients/sandbox/sandbox.ps1` / `provisioning/installer/tests/sandbox.ps1` in Windows Sandbox with this branch's builds (no .NET 3.5 in the sandbox) |

Nothing is run on the host PC that changes its system (no installers, no service registration). That happens
only in Windows Sandbox.

## Decisions for the user

1. **Target .NET 10 instead of .NET 8** (8 is out of support on 2026-11-10). *Decided: .NET 10.*
2. **Engine:** *Decided: .NET 10*, with the broker and KioskServices reimplemented (see §7). VistaDB:
   *decided: SQLite*.
3. **Unset endpoint defaults:** *decided: reserved placeholder hosts* (`*.kiosk-backend.invalid`).
4. **Shell on Windows Pro:** add the `Winlogon\Shell` fallback (recommended), or keep requiring an edition with
   Shell Launcher.
5. **Self-contained x86 deployment** for the .NET 10 services (recommended), or require a .NET 10 x86 runtime
   on the kiosk.
6. **Broker:** clean .NET 10 GTCP/NRBF client (recommended) vs. a 4.8 bridge process.

## Progress

| Step | State | Commit(s) |
|---|---|---|
| Survey and plan | done | this file |
| Build output outside the tree (`RB_ARTIFACTS` / `-p:ArtifactsPath`) | done | `Build: optional artifacts output ...` |
| Engine, KioskServices, Rental, KioskShell: net472 → net48 | done, builds | `Engine, KioskServices, Rental, KioskShell: target .NET Framework 4.8` |
| HAL: net35/net40 → net48 (+ `App.config`s), TLS default | done; runs in Windows Sandbox without .NET 3.5 | `HAL: target .NET Framework 4.8 ...`, `HAL IPC: use the OS TLS defaults ...` |
| No `wmic`: Shell Launcher (`kernel.replaceshellwith`) and the engine's touch-screen toggle via `System.Management` | done, builds; WMI calls not run (this PC has no Shell Launcher) | `Windows 11: call WMI directly instead of wmic.exe` |

| Update Client → .NET 10, self-contained win-x86 | done; Sandbox: IoT certificate, MQTT/TLS connect, command handled | `Update Client: .NET 10 ...` |
| Kiosk Client → .NET 10, self-contained win-x86 | done; Sandbox: proxy/data/ad calls against the backend | `Kiosk Client: .NET 10 ...` |
| Device Service → .NET 10, self-contained win-x86 (+ simulator, smoke test, `tests/DeviceService.LegacyClientCheck`) | done; the 4.8 engine client library passes all checks against it | `Device Service: .NET 10 ...` |
| Endpoint defaults → `*.kiosk-backend.invalid` placeholders; `General/PublicWebsiteUrl` for the title QR codes | done, builds | `Endpoints: placeholder defaults ...` |
| KioskServices REST body length (non-ASCII) | done, builds against the 1.81 references; the Backend case harness targets the 1.80 API and cannot load this build | `KioskServices REST: ...` |
| Update Manager + shared libraries → net48; TLS OS defaults | done; Backend update-manager harness passes | `Update Manager and shared libraries ...`, `TLS: OS defaults ...` |

| `Redbox.Core`: net48 + net10.0-windows; `JavaScriptSerializer` from Microsoft's reference source | done; `tests/Redbox.Core.JsonParity`: 20 cases identical | `Redbox.Core: build for net48 and net10.0-windows ...` |
| Shared libraries (`src/Common`): net48 + net10.0-windows | done; Update Manager harness still passes on net48 | `Shared libraries: build for net48 and net10.0-windows` |
| Engine, kernel extensions, Rental, HAL client: also net10.0-windows (self-contained win-x86) | builds; not run yet (the Sandbox was taken by another test) | `Engine: also build for net10.0-windows ...`, `Engine (.NET 10): self-contained win-x86` |
| Engine API `:9002` on ASP.NET Core (Web API compatibility layer) | done; `tests/Redbox.KioskEngine.API.Parity`: 14 requests identical to Web API | `Engine API (:9002) on .NET 10 ...` |
| Update Manager: also net10.0-windows | done; Backend update-manager harness passes on the .NET 10 build | `Update Manager: also build for net10.0-windows` |
| Legacy broker over GTCP without GenuineChannels | done; `tests/BrokerGtcp.Check`: 89 offline + 21 live checks against rb-remoting | `Broker on .NET 10: GTCP client ...` |
| VistaDB → SQLite (engine, Rental, HAL storage, migration tool) | done; `tests/Redbox.Data.SqliteParity`: 119 results identical | see "VistaDB → SQLite" |
| **.NET 10 engine end to end** | **runs in Windows Sandbox**: start view, HAL start-up jobs (HAL 4.8), `:9002`, VistaDB files converted on first start, registered with the reservation broker over GTCP, with the .NET 10 Kiosk Client, Update Client and Device Service | `.NET 10 runtime: ...`, `Engine (.NET 10): load the UI assemblies ...` |
| KioskServices (`Redbox.KioskServices.*`): also net10.0-windows; Remoting mode as a hand-written binary .NET Remoting call over HTTP (`Redbox.Remoting.Nrbf`) | done; `tests/KioskServicesRemoting.Check`: 4 messages give the same answer over Remoting (rb-remoting's real .NET Remoting host) and REST | `KioskServices on .NET 10: ...` |
| **HAL on .NET 10**: `halsvc-win32`, `HAL.PackageInstaller`, `CameraTuner`, the camera plug-in and their libraries (self-contained win-x86); job contexts as JSON, 4.8 contexts imported | **runs in Windows Sandbox** with the .NET 10 engine (start view, its HAL jobs complete); `tests/HalContextStore.Check`: 35 of the kiosk's 4.8 contexts imported and round-tripped | `HAL on .NET 10: ...` |
| HAL: clean stop on configuration errors (a `hal.xml` without decks) | done; Sandbox: P006, service ends Stopped | `HAL: stop cleanly when the configuration has errors ...` |
| IDE: Scintilla (`Scintilla5.NET`, MIT) in place of Puzzle.SyntaxBox; the .NET 10 engine without BinaryFormatter | done; `tests/CodeEditor.Check`: 29 checks on net48 and .NET 10; Sandbox: the engine reaches its start view with `EnableUnsafeBinaryFormatterSerialization` false | `IDE: Scintilla in place of Puzzle.SyntaxBox ...` |
| `build/release.ps1 -Framework net10.0-windows` for the multi-target components | done; `kop-hal-install`: net48 zip as before (34 files), .NET 10 zip self-contained (324 files, `NewFiles` lists them all); not yet published to the backend or installed by the Update Manager | `release.ps1: -Framework ...` |

Notes from the .NET 10 engine work:

- **Multi-targeting.** Every project the engine uses builds for `net48;net10.0-windows`, so the 4.8 engine stays
  shippable while the .NET 10 one is brought up. Things .NET 10 lacks are behind `#if NETFRAMEWORK` with a .NET 10
  path next to them (LINQ to SQL overload, `System.Configuration.Install`, AppDomains, Evidence, `BinaryFormatter`,
  .NET Remoting).
- **Name clashes with newer .NET.** `System.Range`, `System.Threading.ITimer` and a non-generic
  `System.Collections.Generic.KeyValuePair` collide with Redbox types of the same name; `using` aliases resolve them.
- **`kioskengine.dll.config`.** On .NET 10 `ConfigurationManager` reads `<app>.dll.config`, so a release of the
  .NET 10 engine has to put the kiosk's `kioskengine.exe.config` there too.
- **Compiles is not runs.** What the first Sandbox runs of the .NET 10 engine found, all fixed:
  - code page 1252 needs `CodePagesEncodingProvider` (VistaDB; registered by `Redbox.Core`);
  - `<app>.dll.config` instead of `<app>.exe.config`, and no undeclared sections (`NetFrameworkCompatibility.ImportExeConfig`);
  - `Encoding.Default` is UTF-8 (hash/encryption code uses the ANSI code page explicitly);
  - an `.exe` project's code is in `<name>.dll` (theme and IPC command scans);
  - `Assembly.LoadFile` makes a second copy of an already-loaded assembly (`AssemblyLoading.LoadFile`);
  - the IDE's Puzzle.SyntaxBox resources needed BinaryFormatter; SyntaxBox is now replaced by Scintilla
    (`CodeEditor`), and the engine runs without the BinaryFormatter compatibility package.

  Quartz 1.0 and VistaDB 3.5 load and work on .NET 10 as they are.

Notes from the .NET 10 services:

- **`System.Version` over JSON.** On .NET 5+ `Version` has a `TypeConverter`, so Newtonsoft reads and writes it as a
  string. The engine's SignalR 1.1 client (and 2.2) use the object form. Without a converter every Device Service
  hub command from the engine was silently dropped. `DeviceService.ComponentModel.LegacyVersionJsonConverter`
  restores the 2.2 form. Any other .NET 10 API that carries `Version` needs the same converter (none found in the
  Kiosk Client or Update Client).
- **Development environment.** With `ASPNETCORE_ENVIRONMENT=Development` the generic host validates DI scopes, and
  the Kiosk Client's existing registrations (a singleton that uses a scoped service) fail at start-up. 2.2 did the
  same in Development; kiosks run as Production.
- **Releases.** `build/release.ps1` publishes .NET 5+ projects self-contained (the project's own
  `RuntimeIdentifier`), so a release zip is ~110 MB per service, but kiosks need no .NET runtime installed.
- **Multi-target components** (engine, HAL) are released for one framework: `-Framework net48` (the default, what
  kiosks run today) or `-Framework net10.0-windows`. A .NET 10 release publishes the exes self-contained, builds the
  plug-in libraries, leaves out the component's .NET Framework-only projects, and lists every file in `NewFiles`,
  because the runtime is new to the kiosk's repository (the backend refuses unlisted new files). The restore runs
  without `TargetFramework`; given globally it would also apply to the netstandard2.0 references (NETSDK1005).

HAL in Windows Sandbox (Backend `tests/clients/sandbox`, emulator, backend under WSLC, 2026-10-31): the 4.8 build
(this branch's `halsvc-win32`, `HAL.PackageInstaller`, `halmgmtc` over the image's HAL install) started as
`halsvc$default` on .NET 4.0.30319.42000 with only the v4 runtime allowed, and listened on 7001. The engine
then started and reached its start view. HAL's errors were the same as with the original binaries on the same
setup:

- `RebootTime` secret option missing;
- `ARCUSNOTRESPONSIVE` from the emulator in `init` and `hardware-healer`.

The one extra line, "Unable to load assembly Interop.ClearImage.dll to scan for commands", comes from the
separate `Interop.ClearImage.dll`. Redbox's build merged it into `Redbox.HAL.DataMatrix.Framework`.
`build/release.ps1` still puts the separate DLL into the `kop-hal-install` zip, on `main` too. Follow-up: leave
merged-away assemblies out of release zips.

Notes from the HAL step:

- The `.exe.config` files on kiosks (`halsvc-win32`, `halmgmtc`, `MSHALTester`) contain only
  `<supportedRuntime version="v2.0.50727"/>`, so a 4.8 build does not start until its config is replaced. The
  projects now ship those configs (the original contents with `v4.0`/4.8 as the runtime), so a release that
  includes `bin\*.exe.config` fixes this. A release that keeps the kiosk's old configs would leave HAL down.
- `HAL.PackageInstaller` refuses to run unless `GetPlatform()` says `Windows7`, which means OS version 6.x
  except 6.0. Without a Windows 10 compatibility manifest, Windows 10/11 report 6.2, so it passes. Adding such
  a manifest to the HAL exes would break this check.
- A 0-deck `hal.xml` died in `DecksManager.First`; it is now the P006 configuration error and the service stops
  (see the HAL .NET 10 notes).

Notes from HAL on .NET 10 (Sandbox, 2026-10-31):

- **What runs where.** `halservice\bin` holds one framework: the .NET 10 release replaces `halsvc-win32`,
  `HAL.PackageInstaller`, `CameraTuner`, `Redbox.HAL.Service.Framework`, the libraries and
  `camera.plugins\Redbox.HAL.DirectShow.Frame.Plugin.dll`. `halmgmtc` (its own copies of the HAL libraries, log4net
  1.2.9) and the two legacy camera plug-ins (`DirectShow`, `Vidcap`; not configured on the image's kiosk) are still
  net48 only and are left out of a .NET 10 release. Follow-up: `halmgmtc` on .NET 10.
- **Job contexts.** The 4.8 HAL saves every job in `data\contexts\<id>\ctx.data` (and cooperative jobs' compiled
  programs) with the BinaryFormatter. The .NET 10 HAL writes `ctx.json` (the same `GetObjectData` entries) and keeps
  a program as its script source (`<program>.hs`, compiled again on load). On its first start it reads each
  `ctx.data` with `System.Formats.Nrbf` (a decoder; nothing is deserialized into objects), saves it as `ctx.json` and
  renames the old file to `ctx.data.imported`, so a kiosk keeps its jobs and their results across the upgrade. A
  4.8 job's programs are taken from the current scripts. `ExecutionContext.Save` on 4.8 opened `ctx.data` without
  truncating it (`File.OpenWrite`), so old files can carry stale bytes after the record; the decoder stops at the
  record's end.
- **HAL's own quirks kept.** `Error` parsing keeps the space after `ERROR:`, so an error's text gains a space every
  time a context is loaded (4.8 too); `HalContextStore.Check` ignores repeated spaces in that one entry.
- **Platform check.** .NET 5+ reports the real Windows version (10.0) whatever the manifest, so `GetPlatform()`
  maps 10+ to `Windows7` (which stands for "7 or later"); otherwise the .NET 10 `HAL.PackageInstaller` refuses to run.
- **Folder scans.** HAL's command scan and the installer's handler scan load every `*.dll` in `bin`. A
  self-contained .NET folder has ~230 of them (the runtime, native DLLs, and each app's `<name>.dll`), so the scans
  skip the runtime's assemblies, native DLLs and app assemblies (a `<name>.runtimeconfig.json` next to them). The
  .NET Framework scans never saw the apps either, since those were `.exe` files.
- **Threads.** `Thread.Abort` (DirectShow capture threads) does not exist on .NET; the threads already poll their
  stop event every 100 ms, so `Stop` sets it and waits.
- **Stopping after start-up errors.** Any configuration error made the service call `Stop()`, whose first
  `NullReferenceException` (from parts that were never set up) escaped and left the service Running but not
  listening, on 4.8 too. It now stops each part on its own and ends Stopped.
