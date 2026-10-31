# Modernization plan (`modernize` branch)

`main` stays the faithful decompile. This branch is a long-lived fork that:

1. moves the code to current .NET where that is feasible, and to .NET Framework 4.8 where it is not;
2. makes the decompiled code readable without changing what it does;
3. fixes what breaks on Windows 10/11;
4. talks to the self-hosted backend through configuration instead of Redbox's hosts.

It is never merged into `main`. Every step keeps `Redbox.sln` building and is one commit (or a short series of
commits) per coherent change, so `git log main..modernize` reads as a list of the changes.

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
| .NET Framework 3.5 (HAL, tools) | **4.8** first, `net10.0-windows` evaluated later | .NET 3.5 is the main thing that breaks on Windows 11 (an optional feature; Windows Sandbox can't enable it). 4.8 ships in Windows 10 1903+ and 11. It loads the same 2.0-era DLLs (VistaDB 3.5, log4net 1.2.9) unchanged. |
| .NET Framework 4.0–4.7.2 (engine, Update Manager, installer) | **4.8** first; .NET 10 where the blockers below are solved | Targeting 4.7+ alone fixes the TLS 1.0 default of `SslStream`/`HttpWebRequest`. |
| `netstandard2.0` libraries (Device Service model, RBA SDK) | stay `netstandard2.0` | the 4.8 engine and the .NET 10 services both use them |
| Shared `Redbox.*` libraries (`net46`) | `net48;net10.0-windows` multi-target, once a .NET 10 consumer needs them | |

### What keeps a component on 4.8 (for now)

| Blocker | Used by | Way off |
|---|---|---|
| .NET Remoting over GenuineChannels `gtcp://` (legacy reservation broker, the 1.88 default `UseLegacyBrokerService = true`) | engine (`Redbox.BrokerServices.Proxy`) | a minimal MS-NRTP/MS-NRBF client for the few broker calls, checked against `rb-remoting` (Mono, the kiosk's own GenuineChannels.dll); or switch the engine to the HTTP broker path (`UseAWSBrokerService`) if the backend supports it |
| .NET Remoting `HttpChannel` + binary formatter (KioskServices) | engine (`Redbox.KioskServices.Proxy`) | there is one call, `IMessageService.Process(string) -> string`. Use the existing `KioskServicesProtocol = REST` mode, which the backend serves, or a hand-written NRBF request for the one method |
| VistaDB 3.5 (`VistaDB.NET20.dll`, 2008, commercial, no successor we can ship) | engine (`Redbox.KioskEngine.Environment`, `Redbox.Rental.Data`), HAL (`Redbox.HAL.DataStorage`) | spike: load it on .NET 10. If that fails, migrate to SQLite with a one-time importer, a larger change that needs sign-off |
| ASP.NET Web API 5 self-host (WCF underneath) | engine (`Redbox.KioskEngine.API`, `:9002`) | Kestrel minimal host with the same routes and Newtonsoft settings |
| Engine plug-ins compiled against the engine (kernel extensions, Rental bundles) and Lua scripts that call .NET types | engine | the plug-ins are built from this repo, so they move together; the Lua scripts in bundles have to keep finding the same types and members |
| IDE-only WinForms controls (Puzzle.SyntaxBox, WeifenLuo 2.5, SmartPropertyGrid, VisualTips) | `Redbox.KioskEngine.IDE` (developer tool inside the engine) | newer WeifenLuo/DockPanelSuite exists; SyntaxBox does not. Option: build the IDE only for 4.8 and leave it out of the .NET 10 engine |
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

If 1 or 2 fails, the engine stays on 4.8. That is a supported, in-box runtime on Windows 10/11.

### 8. Installer pieces

- Bring the Backend PR #5 overlay of the setup host into `src/Installer/host` here: 4.8, `JScriptCodeProvider`
  instead of VSA, and the Divelements resource workaround. The overlay in the Backend then becomes a plain
  build of this branch.
- `KioskAdmin`, NAnt tasks, `Redbox.TimeKeeper*`: 4.8. Passwords stay out of the code (they are already
  `<redacted>`); they become parameters.
- `installer-front-end`: 4.8 x86, and `UpdateServiceUrl` from configuration.

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

1. **Target .NET 10 instead of .NET 8** (8 is out of support on 2026-11-10). *Recommended: .NET 10.*
2. **Engine:** 4.8 now and .NET 10 after the spikes (recommended), or go straight for .NET 10.
3. **Unset endpoint defaults:** empty values (recommended), or reserved `*.invalid` placeholder hosts.
4. **Shell on Windows Pro:** add the `Winlogon\Shell` fallback (recommended), or keep requiring an edition with
   Shell Launcher.
5. **Self-contained x86 deployment** for the .NET 10 services (recommended), or require a .NET 10 x86 runtime
   on the kiosk.
6. **VistaDB:** if it doesn't load on .NET 10, migrate to SQLite or keep the engine/HAL on 4.8.

## Progress

| Step | State | Commit(s) |
|---|---|---|
| Survey and plan | done | this file |
