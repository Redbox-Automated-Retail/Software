# Kiosk Engine

The Kiosk Engine ("Redbox Engine for Distributed Stores", REDS) is the customer-facing process on the
kiosk. It is a .NET Framework 4.7.2 WinForms/WPF host (`kioskengine.exe`, version 1.88.0.80 in the image)
that:

- owns the touch screen (one full-screen window that renders either GDI+ "actor" scenes or WPF views),
- loads **resource bundles** (zip files of XML resources, Lua scripts, images and fonts) and runs one
  *application bundle* at a time (Rental, Control Panel, Field Maintenance),
- hosts a **Lua** virtual machine and exposes several hundred .NET "kernel functions" to scripts,
- hosts **kernel extensions** (Broker / Hardware / Update services) that bridge to other local and remote
  services (HAL robot service, update manager, reservation broker),
- keeps a persistent outbound **message queue** (`message.data`) and local key/value data stores,
- exposes a local **IPC command listener** (`rcp://localhost:8004`) and a local **HTTP Web API**
  (`http://localhost:9002`) used by the update manager, Update Client Service, Kiosk Client Service and
  the Preference Manager tool.

Most of the business logic of the rental application lives in `Redbox.Rental.*` (see `rental.md`); the
engine provides the runtime those assemblies plug into. The Lua runtime (`Redbox.Lua`), the bundle file
format library (`Redbox.REDS.Framework`), the IPC framework and other shared libraries are documented in
`common-libraries.md`; only how the engine uses them is described here.

---

## 1. Deployment

### 1.1 Files in the image (`REDS/Kiosk Engine/`)

| Path | Contents |
|---|---|
| `bin/kioskengine.exe` (+ `.config`) | Engine host. x86 (`PlatformTarget x86`), .NET 4.7.2. |
| `bin/Redbox.KioskEngine.*.dll`, `Redbox.Controls.dll`, `Redbox.*Services.*.dll`, `Redbox.Rental.*` | Engine libraries, kernel extensions and the rental application assemblies (all loaded in-process). |
| `bin/Redbox.ControlPanel.UI.exe`, `bin/Redbox.FMA.UI.dll` | WPF views for the Control Panel and Field Maintenance applications. Loaded as assemblies by the engine (the Control Panel `.exe` is not run on its own in normal operation). |
| `bin/Redbox.KioskEngine.PreferenceManager.exe` | Stand-alone WPF diagnostic tool (Section 9.4). |
| `bin/redscc.exe` | Bundle compiler (Section 9.5). |
| `bin/log4net.config` | Logging configuration (Section 8). |
| `bin/message.data` | VistaDB database holding the outbound message queue (Section 7.2). Note: it lives in `bin`, not `data`. |
| `bundles/*.bundle` | Resource bundles (zip). Image contains `ra-2.x`, `cp`, `fm`, `library-rental-common`, `content-legal`, `content-speech` and several `content-*` bundles. |
| `bundles/.staged_/`, `bin/.staged_/`, `data/.staged_/` | Files staged by the update system (see `update-manager.md`, `update-client.md`). |
| `data/*.config` | JSON configuration files managed by the update manager (Section 8.1): `system.config`, `kioskengine.config`, `kioskhealth.config`, `marketing.config`, `versionsneeded.config`. |
| `data/*.data`, `data/*.active`, `data/*.staged`, `data/*.json`, `data/ProfileImages/`, `data/AnalyticsSessions/`, `data/certs/` | Rental data (product catalog, pricing, inventory, analytics sessions, card-encryption certificates). Owned by the rental application, see `rental.md`. |
| `logs/` | `kioskengine.log*`, `error/kioskengine-error.log*`, `UI/*.log`, `Test/testsvcs.log`. |
| `screenshots/screenshot_N.png` | Written by the F1 hot key. |
| `afteractivate.lua`, `beforeactivate.lua` | Update-manager activation scripts (they use `kernel.markscriptcomplete` / `kernel.filedelete`, which are **update manager** kernel functions, not engine functions). The engine never runs them. |

The installed path is `C:\Program Files\Redbox\REDS\Kiosk Engine\` (seen in logs and config files).
Paths the engine computes at run time are relative to `bin`: data `..\data`, bundles `..\bundles`,
kernel extensions `..\kext` (legacy, unused), file cache `..\cache`, screenshots `..\screenshots`,
logs `..\logs`.

The image also contains `bundles/ra-2.x.bundle.mod`, `bundles/fm.bundle.orig`,
`bin/Redbox.ControlPanel.UI.exe.orig` etc. These are not loaded (only `*.bundle` files are scanned) and
appear to be local modifications made to this image after it was taken from a kiosk (inferred from the
file names and dates).

### 1.2 Process, start-up and dependencies

- `kioskengine.exe` is an interactive desktop process, not a Windows service. It is launched by
  `KioskShell` (default path `C:\Program Files\Redbox\REDS\Kiosk Engine\bin\kioskengine.exe` in
  `src/Tools/KioskShell/Properties/Settings.cs`) and by the update manager daemon
  (`${REDS}\Kiosk Engine\bin\kioskengine.exe`, `src/UpdateManager/updatemgrd/Daemon/Properties/Settings.cs`).
  See `common-libraries.md` and `update-manager.md`.
- Single instance: a local `NamedLock` named `{FB5E48FF-4677-4cfa-8ED1-265114D7564C}` and a global mutex
  `Global\{2E14AC6E-F57C-4221-9BCA-D1FB6DD11FE0}`; a second instance exits with error code `INSTANCE`
  (`src/KioskEngine/kioskengine/EngineApplication.cs`).
- Command line (`src/KioskEngine/kioskengine/Options.cs`):
  `kioskengine [-D:property=value ...] [--bundle[:|=]file]`. `--bundle` is a bundle specifier
  (`"Product Name,version"`) that overrides the default bundle. `-D` properties are parsed but not used by
  any code in 1.88.

### 1.3 Endpoints

| Direction | Endpoint | Purpose | Defined in |
|---|---|---|---|
| Listens | `rcp://localhost:8004` (IPC.Framework TCP) | `engine ...` commands from update manager / KioskShell | `UpdateServicesHost.ExecuteListener` |
| Listens | `http://localhost:9002` (ASP.NET Web API self-host + Swagger) | Engine REST API | `Redbox.KioskEngine.API/APIService.cs` |
| Listens | Window message `WM_COPYDATA` (0x4A) to the host form | Legacy text commands | `kioskengine/HostForm.cs`, `CommandHelper.cs` |
| Connects | `rcp://localhost:7001` (image registry overrides to `rcp://127.0.0.1:7001`) | HAL service (robot, vend/return/sync jobs) | `Redbox.HardwareServices.Proxy/HardwareService.cs`; protocol in `hal.md` |
| Connects | `rcp://localhost:7004` | Update manager service: config file activation, downloads, schedule events | `Redbox.UpdateServices.Proxy`, `Redbox.UpdateManager.Client`; protocol in `update-manager.md` |
| Connects | `gtcp://blt.accessredbox.net:7890/RegistrationBroker/Default.rem` | Legacy reservation broker (.NET Remoting over GenuineChannels TCP); only if `UseLegacyBrokerService` is true | `Redbox.BrokerServices.Proxy/BrokerServicesProxy.cs` |
| Connects | `{KioskDataServicesUrl}/api/KioskData/AddKioskSessionJson` (HTTPS) | Upload of analytics sessions from the message queue | `Redbox.KioskDataServices.Proxy` |
| Connects | `http://localhost:9001` (Kiosk Client Service), `http://localhost:6001` (Update Client Service), Device Service | Used by rental services running in-process | `rental.md`, `kiosk-client.md`, `update-client.md`, `device-service.md` |

Firewall exemptions: `FirewallHelper.CreateFirewallExemptions()` runs first, but on anything newer than
Windows XP it only logs "Not running on Windows XP; skipping firewall exemptions."

---

## 2. Architecture

```mermaid
flowchart TB
  subgraph kioskengine.exe
    Host[kioskengine<br/>Program / EngineApplication / HostForm]
    Env[Redbox.KioskEngine.Environment<br/>ResourceBundleService, KernelExtensionService,<br/>ViewService, QueueService, CallbackService, ...]
    Kern[Redbox.KioskEngine.Kernel<br/>KernelService (Lua VM) + core kernel functions]
    Draw[Redbox.KioskEngine.Drawing<br/>RenderingService / Scene / Actor]
    API[Redbox.KioskEngine.API<br/>Web API :9002]
    IDE[Redbox.KioskEngine.IDE<br/>IDE, debugger, preference dialogs, bundle picker]
    KX1[BrokerServicesHost]
    KX2[HardwareServicesHost]
    KX3[UpdateServicesHost<br/>IPC listener :8004, ConfigurationService]
    Rental[Redbox.Rental.* + Rental.UI<br/>(rental.md)]
    CP[Redbox.ControlPanel.UI]
    FMA[Redbox.FMA.UI]
    Lua[(Lua VM per bundle activation)]
  end
  Bundles[(bundles/*.bundle)] --> Env
  Host --> Env --> Kern --> Lua
  Env --> Draw
  KX2 -->|rcp :7001| HAL[HAL service]
  KX3 -->|rcp :7004| UM[Update manager]
  KX1 -->|gtcp :7890| Broker[Reservation broker]
  UMc[updatemgrd / KioskShell] -->|rcp :8004| KX3
  UCS[UpdateClientService / KioskClientService / PreferenceManager] -->|http :9002| API
  Rental --> Env
  CP --> Env
  FMA --> Env
```

### 2.1 Projects

| Project | Role |
|---|---|
| `src/KioskEngine/kioskengine` | Entry point (`Redbox.KioskEngine.Bootstrap` namespace): `Program`, `EngineApplication` (an `ApplicationContext` that is also `IInputService`, `IFileCacheService`, `IEngineApplication`), `HostForm` (full-screen form, hot keys, `WM_COPYDATA`), `CommandHelper`. Also wires up the rental services. |
| `src/KioskEngine/Redbox.KioskEngine.ComponentModel` | All service interfaces (`IKernelService`, `IResourceBundleService`, `IKernelExtensionHost`, `IQueueService`, `IViewService`, `IConfigurationService`, `IMachineSettingsStore`, ...), `KernelFunctionAttribute`, `ErrorList`/`Error`, API DTOs (`ApiServiceBaseResponse<T>`, `ApiService/EngineCore/*`), `QueueServiceConstants`. |
| `src/KioskEngine/Redbox.KioskEngine.Environment` | Implementations of the engine services (bundles, extensions, views, timers, tweens, callbacks, queue, data stores, scheduler, settings stores, preferences, card-reader detection, text-to-speech, themes, IDE launcher). Embeds the VistaDB migration scripts `Migrations.message.data.*.sql` and `Migrations.session.data.*.sql`. |
| `src/KioskEngine/Redbox.KioskEngine.Kernel` | `KernelService` (Lua VM host, script loading, test plans), `KernelFunctionRegistryService`, and 33 files of core kernel functions (Section 5.3). |
| `src/KioskEngine/Redbox.KioskEngine.Drawing` | `RenderingService`, `Scene`, `Actor`: the GDI+ 2-D scene graph and its WPF hosting (`ElementHost`). |
| `src/KioskEngine/Redbox.KioskEngine.API` | Self-hosted ASP.NET Web API 5.2 + Swashbuckle on `KioskEngineApiServiceBaseUrl`. |
| `src/KioskEngine/Redbox.KioskEngine.IDE` | WinForms developer tooling: IDE main form, Lua debugger windows, resource editors, bundle selector, preference manager dialog (F4), task manager / task runner, secure browser, `SwitchStatusForm`, `UnlockForm`. |
| `src/KioskEngine/Redbox.KioskEngine.PreferenceManager` | Separate WPF exe that inspects a running kiosk through the engine API and Kiosk Client Service. |
| `src/KioskEngine/Redbox.ControlPanel.UI` | WPF views/controllers of the technician Control Panel ("Redbox Desktop"). |
| `src/KioskEngine/Redbox.FMA.UI` | WPF views of the Field Maintenance Application (planogram selection). |
| `src/KioskEngine/Redbox.Controls` | Shared WPF controls and converters (`RoundedButton`, `BorderButton`, `SectionalBar`, `SpinControl`, `GridExt`, ...). |
| `src/KioskEngine/Redbox.Services.KioskBrokerServices.KioskShared` | Serializable DTOs and remoting interfaces shared with the reservation broker server (Section 6.4). |
| `src/KioskEngine/redscc` | Bundle compiler (.NET 3.5, contains its own old copies of Core/GetOpts/Lua/REDS.Framework). |
| `src/KioskServices/Redbox.BrokerServices.*` | Broker kernel extension, kernel functions, remoting proxy. |
| `src/KioskServices/Redbox.HardwareServices.*` | Hardware kernel extension, kernel functions, HAL client proxy, sync processors, alert manager, preference pages. `Redbox.HardwareServices.TestProxy` is a hand-written stub (marked `RECONSTRUCTED`). |
| `src/KioskServices/Redbox.UpdateServices.*` | Update kernel extension: IPC listener, `ConfigurationService`, update-manager proxy and kernel functions. |
| `src/KioskServices/Redbox.KioskDataServices.Proxy*` | Queue processor that uploads analytics sessions to Kiosk Data Service. |
| `src/KioskServices/Redbox.FraudServices.Proxy*` | `IBarcodeTrackingService`; in 1.88 it only logs (`BarcodeTrackingService.AddBarcodeTrackingInfoToQueue`). |
| `src/KioskServices/Redbox.KioskServices.*` (1.81.0.27) | Older "Kiosk Services" extension. Shipped in `bin` but **not loaded by the 1.88 engine** and excluded from the build (see `docs/decompilation.md`). Section 6.6. |
| `src/KioskServices/Redbox.KioskServices.ComponentModel` (2.6.0.11) | `IMessageService { string Process(string payload); }` remoting interface for the Kiosk Services server. |

All services are singletons registered in `Redbox.Core.ServiceLocator` and fetched by interface type.

---

## 3. Start-up and lifecycle

### 3.1 Process start (`Program.Main` → `Program.Run`)

1. Install `AppDomain.UnhandledException` and `Application.ThreadException` handlers (both log, write
   `..\data\UnhandledException.txt` and exit).
2. Configure log4net from `bin\log4net.config` for the root logger and the named loggers `CardReaderLog`,
   `UserLoginLog`, `TestServices`, `ControlPanelActionLog`, `FieldMaintenanceActionLog`.
3. Force `ServicePointManager.SecurityProtocol = Tls12`.
4. `EngineApplication.Instance.Initialize()` (3.2). If the error list contains `INSTANCE`, exit silently;
   if it contains errors, show `ErrorForm` and exit.
5. `ReadUnhandledExceptionAndSendAlert()`: if `UnhandledException.txt` exists (JSON
   `{"Message":..., "Exception":...}` written by the previous crash), delete it and, unless
   `system/General/SendAppCrashAlert` is false, send a health alert of type `ApplicationCrash`, subtype
   `Unhandled`.
6. `EngineApplication.Instance.Run()` (3.3), then `Application.Run(EngineApplication.Instance)`.

### 3.2 `EngineApplication.Initialize()`

In order (`src/KioskEngine/kioskengine/EngineApplication.cs`):

1. Firewall helper (no-op), `ConfigureDefaultMacros()` → macros `EngineVersion`, `RunningPath`,
   `ProductName = "Redbox Kiosk Engine"`, `ProductVersion`.
2. Single-instance locks; command-line parse errors become `E005`.
3. `CommandHelper.Initialize()` registers the `WM_COPYDATA` commands.
4. Settings stores rooted at registry key `SOFTWARE\Redbox\REDS\Kiosk Engine` (HKLM for machine, HKCU for
   user).
5. `KernelExtensionService.Initialize([BrokerServicesHost, HardwareServicesHost, UpdateServicesHost])` —
   the three extension hosts are **compiled-in types**, created with `Activator.CreateInstance`, each
   wrapped in a `KernelExtension` record (`ID` = new GUID, `Name` = type name, `Version` = file version)
   and given `PreLoad()`. Only `UpdateServicesHost.PreLoad` does work: it initialises the
   `ConfigurationService` (update-manager configuration client).
6. `InitializeConfiguration()`: start the configuration client and register every `data\*.config` file
   under its file name without extension (`system`, `kioskengine`, `kioskhealth`, `marketing`,
   `versionsneeded`), default category `"default"`. A reload of `kioskhealth` restarts the kiosk event and
   health services. If the data folder cannot be enumerated, the process exits.
7. Register ~35 engine services (`IViewService`, `ITimerService`, `ICallbackService`, `ISecurityService`,
   `IPreferenceService`, `IConfiguration` (rental), `IDeviceServiceClientHelper`, track-data services,
   `IReservationService`, `IHttpService`, `IUpdateClientService`, `IApiService`, ...). `ITextToSpeechService`
   only if `system/General/TextToSpeechEnabled`.
8. Add the three engine preference pages (`Engine Core/General`, `Engine Core/IDE`, `Engine Core/Resource`).
9. Initialise scheduler (Quartz), data stores, font cache, bitmap cache, rendering, kernel (creates the
   first Lua VM), environment manager (card-reader detection), queue service on `bin\message.data`,
   Kiosk Client Service client, shopping sessions (`bin\session.data` is deleted at start), debug service,
   authentication/store/health/marketing services (rental).
10. `KernelExtensionService.LoadAllExtensions()` → each host's `Load()` (Section 6).
11. `ResourceBundleService.Initialize(exePath, DevBundlePath)`.
12. Sync processors (`ISyncUnknownProcessor`, `ISyncEmptiesProcessor`) from the hardware proxy.
13. `InitializeRedboxRental()` — creates and registers the rental services (session, control panel and
    field maintenance services, store manager, kiosk events, inventory, `RedboxRentalService`, data
    service, pricing, ads, loyalty, health items, ping). It calls
    `IResourceBundleService.RegisterAssemblyFunctions("Rental Kernel", typeof(RentalDataFunctions).Assembly)`
    so that the rental kernel functions are added at every bundle activation. Details in `rental.md`.
14. `ResourceBundleService.LoadBundles()` (4.3).
15. Choose the bundle: `--bundle` if given, otherwise `DefaultBundleName` (config
    `system/Store/DefaultBundle`, default `"Rental Application,*"`; the image's `system.config` has
    `General/DefaultBundle = "Rental Application,*"`). `Activate(name, version)` only marks it as the
    pending switch target; the actual switch happens in the idle loop.
16. Start the health ping timer and the Web API (`IApiService.Initialize`).

### 3.3 `EngineApplication.Run()`

Creates `HostForm` (main form), registers it for environment notifications, creates the default scene
(`IRenderingService.CreateScene("Default", width, height, bg, elementHost)`) whose WPF content is hosted
in an `ElementHost`, merges the WPF style dictionaries of `Redbox.Rental.UI`, `Redbox.ControlPanel.UI`
and `Redbox.FMA.UI`, calls `OrchestrationService.RegisterViewDefinitions()` (rental WPF view
registry), and shows the form. If no bundle is pending and there is no default bundle, the
`SelectBundleForm` is shown.

### 3.4 Steady state: the idle loop

`Application.Idle` → `EngineApplication.OnIdle` → `IdleLoop()`, which runs while the message queue is
empty and is re-armed by a 5 ms one-shot timer:

1. `SwitchBundleIfNecessary()` — performs a pending bundle switch (4.4) on the UI thread.
2. `TimerService.UpdateTimers()` — fires Lua soft timers (`kernel.timercreate`).
3. `TweenService.UpdateTweens()` — animations (`kernel.tweencreate`).
4. If `UseTimerToInvokeCallbackEntries` is false: drain callbacks for up to 10 ms. Otherwise (default
   true) the `CallbackService` drains them from its own 5 ms timer through `ThreadSafeHostUpdate`.
5. `RenderScene()` — double-buffered GDI+ render of the active scene (skipped for WPF scenes).

Every 15 s the loop logs idle counts and callback statistics.

All Lua code runs on the UI thread. Background work (HAL events, broker requests, HTTP replies, Quartz
jobs) is marshalled back by enqueueing an `ICallbackEntry` into `CallbackService` or by
`ThreadSafeHostUpdate` (`Control.Invoke`).

### 3.5 Shutdown

Triggers: Escape key on the host form, `engine shutdown` IPC command, `ShutdownEngine` `WM_COPYDATA`,
`POST api/engine/shutdown`, `kernel.applyunsafeshutdown()`, Windows session end.

`EnvironmentFunctions.CanShutdown()` returns false while the Control Panel or Field Maintenance bundle is
active, while any kernel extension reports `CanSwitch == false`, during start-up, or while a rental
session is active (unless in maintenance mode). If shutdown is not allowed, the request is remembered
(`FlagForSafeShutdown`) and the bundle is expected to call `kernel.applysafeshutdown()` when idle.

`OnMainFormClosed` then flushes callbacks, resets timers/scheduler, clears the scene and caches, disposes
the Lua VM, stops the queue worker, closes data stores, stops health/pricing/event services, deactivates
extensions, stops the configuration client and ends the Kiosk Client Service application session.

---

## 4. Bundles

### 4.1 Bundle file format

A bundle is a **zip archive** (`ResourceBundleStorageType.Zipped`) or, for development, a loose folder
containing `manifest.resource` (`ResourceBundleStorageType.FileSystem`). The reader is
`Redbox.REDS.Framework.ResourceBundle` (`common-libraries.md`). Layout of `cp.bundle`:

```
manifest.resource                    required
application-startup.resource         one XML file per resource, at the zip root
config.resource
spinner.resource
...
assets/scripts/application-startup.lua    content files referenced by resources
assets/images/spinner-32x32.gif
assets/fonts/...
```

Only root-level `*.resource` entries are resources; everything else is an asset referenced by an
`<aspect file="...">`.

Each `.resource` file is an XML document:

```xml
<resource type="script" depends="activity_functions, keynavigation_functions, idle_timer_functions, view_functions">
    <property name="name" value="application_startup" />
    <aspect name="content" file="assets/scripts/application-startup.lua" />
</resource>
```

- `type` — resource type, declared in the manifest schema (4.2).
- `depends` — comma-separated resource names; for scripts, executed first (5.2).
- `<property name=... value=...>` or `<property name=...><value>a</value><value>b</value></property>`
  (list). `name` is the lookup key used by scripts (e.g. `application_startup`).
- `<aspect name="content">` holds the payload either inline (XML for tables/views) or via `file=`.

Examples of the types used by the shipped bundles (counts over `ra-2.x`, `cp`, `fm`,
`library-rental-common`): 200 `bitmap`, 134 `font`, 181 `script`, 36 `view`, 11 `table`, 4 `publickey`,
2 `stylesheet`, plus one `manifest` per bundle.

```xml
<resource type="font">
    <property name="name" value="button_font" />
    <property name="font_family" value="Verdana" />
    <property name="point_size" value="12" />
    <property name="font_style" value="Regular" />
</resource>

<resource type="table">
  <property name="name" value="strings" />
  <aspect name="content">
    <row>
      <column name="more_time_prompt_message">Do you want more time?</column>
      <column name="yes_button">YES</column>
    </row>
  </aspect>
</resource>
```

`<column type="System.Int32">` values are converted with `ConversionHelper.ChangeType` when read by
`kernel.gettable` / `kernel.getdictionary`.

### 4.2 The manifest

`manifest.resource` (type `manifest`) carries the bundle identity and the **schema** of all resource
types. Properties read by `ManifestInfo` (`src/Common/Redbox.REDS.Framework/ManifestInfo.cs`):

| Property | Meaning |
|---|---|
| `product_name` | Bundle name, e.g. `Rental Application`, `Control Panel Application`, `Field Maintenance Application`, `Library Rental Common`, `Content Legal`, `Content Speech`. |
| `product_version` | e.g. `3.55.0.27` (all bundles in the image). |
| `bundle_type` | `Application` (default/0), `Library`, `Content`, `HTMLApplication`. |
| `startup_script` | Name of the script resource run on activation (required for `Application`). |
| `requires` | List (or `|`-separated string) of bundle specifiers `"Name,version"`; only `Library`/`Content` bundles are allowed (error `B122`). |
| `test_control` | Name of an XML test-plan resource (used only in the `Integration` environment, 5.5). |
| `control_assembly` | Required for `HTMLApplication` bundles (none in the image). |
| `public_key` | Name of a `publickey` resource used to encrypt card track data (Section 7.5). |

Real example (`ra-2.x.bundle`):

```xml
<resource type="manifest">
  <property name="name" value="manifest" />
  <property name="product_name" value="Rental Application" />
  <property name="product_version" value="3.55.0.27" />
  <property name="startup_script" value="application_startup" />
  <property name="requires">
    <value>Content Legal,*</value>
    <value>Library Rental Common,*</value>
    <value>Content Speech,*</value>
  </property>
  <property name="public_key" value="public_key" />
  <aspect name="schema"> ... resourceType declarations ... </aspect>
</resource>
```

The `schema` aspect declares `resourceType`s with a `superType`, `propertyType`s
(`name`, `type`, `label`, `category`, `defaultValue`, `isFilter`, `score`) and an `aspectType` whose
`type` is the .NET loader (`Redbox.REDS.Framework.TextAspectType`, `XmlAspectType`, `BitmapAspectType`).
The `root` type, inherited by all others, declares the filter properties:

| Filter property | `score` | Set at run time from |
|---|---|---|
| `store_number` | 4 | bundle filter (`kernel.setstorenumber`) |
| `culture`, `market`, `vendor`, `view_name`, `runtime`, `platform`, `environment` | 2 | `culture`/`market`/`vendor` by Lua (`kernel.setculture` etc.), `view_name` by `ViewService.Show`, `runtime="Client"`, `platform=".NET/Windows"`, `environment` = registry `Store\Environment` (default `Production`) |

plus `effective_date` (and `expiry_date`, read by the lookup code).

Bundle specifiers: `"Name,version"`; `*` in the version is replaced by `999999`
(`"*"` → `999999.999999`). `BundleCollection.GetBundle` returns the exact version, otherwise the highest
installed version lower than the requested one — so `"Rental Application,*"` means "latest".

### 4.3 Discovery (`ResourceBundleService.LoadBundles`)

- Search path: registry `Core\BundleSearchPath` (string array), default `..\bundles`, relative to `bin`.
  With a debugger attached and `DevBundlePath` (`kioskengine.exe.config`, value
  `C:\work\tfs\Redbox_Rental\Rental Bundles\Main`) existing, every folder with a `manifest.resource`
  under it is used instead.
- Every `*.bundle` in the folder is opened and its resources loaded, **except** a hard-coded skip list:
  `content-banner-marketing`, `content-incart-rundata`, `content-pricing`, `content-promo`,
  `content-vend-screen`, `content-marketing-rundata`, `content-email-opt-in` (`.bundle`). These files are
  still in the image but are ignored by 1.88 (the log shows only `content-legal`, `content-speech`, `cp`,
  `fm`, `library-rental-common` and `ra-2.x` loaded).
- If a folder has no `*.bundle` but has `manifest.resource`, it is loaded as a loose bundle.
- Error `B001` (warning) if a search-path folder does not exist.

### 4.4 Activation and switching (`ResourceBundleService.Switch`)

`Activate(name, version)` (and `ActivateDefaultBundle`, `Restart`, `kernel.activatebundle`, the
`engine load-control-panel` command, the `Activate` `WM_COPYDATA` command) only sets `SwitchToBundle`;
`SetSwitchToBundle` ignores a request to switch to the already-active bundle unless the environment is
`Integration`. The idle loop then calls `Switch`:

1. Show `SwitchStatusForm` ("switching" splash with `SwitchStatusMessage`).
2. Wait until every kernel extension's `CanSwitch` is true, re-checking every 15 s; give up after
   5 minutes (the switch is silently abandoned). Hardware blocks while a return job is runnable
   (`E246`); Broker blocks while a reservation call is in progress (`BSH666`).
3. `Deactivate()` the current bundle: reset tweens, flush and suspend callbacks, `DeactivateAllExtensions`,
   clear all macros except `RunningPath`/`EngineVersion`, reset scheduler, timers, scene, views, style
   sheets, bitmap/font caches, **the Lua VM (`KernelService.Reset` creates a new `LuaVirtualMachine`)**,
   the kernel-function registry, input handlers, environment notifications, shopping sessions,
   callbacks and data stores. Then wait 5 s.
4. Set `ActiveBundle`, create its filter (`runtime`, `platform`, `environment`, plus every string setting
   in `system.config` category `Filters` — the image has `alternate_start_view = VER_NEW_LAYOUT`).
5. `ResourceBundle.Activate` checks the manifest (`E003` missing manifest, `E004` missing startup script).
6. Resolve `requires`; collect startup scripts of required bundles. Build the `ResourceBundleSet`
   (resource lookups search the application bundle and its required bundles).
7. Set macros `ProductName`/`ProductVersion` from the manifest.
8. `ResetAllExtensions()` — each extension re-registers its kernel functions (Section 6).
9. Unregister from the AWS broker service (`IReservationService.UnRegisterFromAWSBrokerService(BundleSwitch)`,
   rental).
10. `KernelService.LoadStartupScript` for each startup script (compile only; error `B033`).
11. Register the "Rental Kernel" assembly functions, then push **all** registered kernel functions into
    the new Lua VM.
12. Resume callbacks, then either start the Lua debugger (bundle picker "break on start"), run the test
    plan (`Integration` environment with `test_control`), or `ExecuteApplication()`.
13. After a successful switch `EngineApplication` calls `ActivateAllExtensions()` and starts the queue
    worker. On failure, `ErrorForm` is shown and the previous bundle is re-queued.

The log of the image shows a full switch to the Rental Application taking ~11 s.

### 4.5 Resource lookup and filters

`GetResource(name)` (`ResourceLookup.GetResource`) picks, among all resources with that name in the
bundle set:

- only resources that are *effective*: `effective_date <= now <= expiry_date`, optional day-part
  properties `daypart_dayofweek_only`, `daypart_effective_weekday`/`daypart_expiry_weekday`,
  `daypart_effective_time`/`daypart_expiry_time`;
- score: 1 if the resource has no filter properties; otherwise, if **all** its filter properties equal
  the current filter values, the sum of their `score`s (else 0);
- the highest score wins.

This is how localisation and per-market overrides work, e.g. `error-message-view.resource` and
`error-message-view_es-mx.resource` both define `error_message_view`; the second has
`culture = es-MX` and wins (score 2) when the culture filter is `es-MX`.

---

## 5. Scripting model (Lua)

### 5.1 Hosting

`KernelService` (`src/KioskEngine/Redbox.KioskEngine.Kernel/KernelService.cs`) owns one
`Redbox.Lua.LuaVirtualMachine` (see `common-libraries.md` for the Lua binding internals), a
`LuaDebugger` (disabled by default; the IDE enables it), and a cache of compiled chunks keyed by resource
name. A new VM is created at every bundle activation, so no Lua state survives a bundle switch.

- `LoadScript(name, source)` → `LuaRuntime.LoadString(source, name)`, cached.
- `ExecuteScript(resourceName)` → look up the script resource, load its `depends` list recursively
  (each dependency is compiled and executed **once**, the first time it is loaded), compile and call the
  script. Script content is decoded as ASCII. Every 5+ minutes it runs
  `collectgarbage('collect'); return collectgarbage('count')` and logs the Lua heap size.
- `ExecuteChunk(text)` / `ExecuteFunction(globalName, args...)` run arbitrary code or a global function
  (used by `WM_COPYDATA ExecuteScript`, the Quartz scheduler, the IDE immediate window).
- Lua exceptions go to `RaiseDebuggerOrError` (break into the debugger if attached, otherwise log).

### 5.2 Script structure

The application's `startup_script` is an ordinary script resource whose `depends` list pulls in the
libraries. E.g. the Rental `application_startup` depends on `lib_constants`, `data_error_handler`,
`localization_functions`, `vend_functions`, ... `pickup_next`; the engine log shows each being executed
(`...Execute script resource 'lib_constants'.`). Scripts typically:

```lua
StoreNumber = kernel.getmachinevalueordefault( "Store", "ID", "N/A" )
kernel.registerconfigurationcallback( "Watch-StoreID",
    function( store, action, path, key, value ) ... end )
kernel.timercreate( 'date_time_refresh', 1000, 1000, function() ... end )
kernel.timerstart( "date_time_refresh" )
view_show( 'control_panel_view' )      -- bundle library helper that calls kernel.pushview/showview
```

Views referenced by name are either Lua/GDI views (`view` resources, 5.6) or WPF views registered by the
.NET UI assemblies (`[ViewDefinition(ViewName=...)]`, e.g. `select_planogram_view` in
`Redbox.FMA.UI`).

Event callbacks from .NET to Lua are Lua functions passed to kernel functions that take delegate
parameters (`HitHandler`, `TimerFiredHandler`, `KeyPressHandler`, `HardwareEventHandler`,
`ConfigurationCallback`, `LuaFunction`, ...). They are always invoked on the UI thread through the
callback queue.

### 5.3 Kernel functions

A kernel function is a static .NET method with one or more
`[KernelFunction(Name = "table.function")]` attributes
(`src/KioskEngine/Redbox.KioskEngine.ComponentModel/KernelFunctionAttribute.cs`).
`KernelFunctionRegistryService` collects them per assembly with an "extension" label;
`KernelService.RegisterFunctions` creates the namespace table (`kernel`, `scheduler_services`) and calls
`LuaRuntime.RegisterMethod(name, method)`. Registration sources at each activation:

| Label | Assembly | Count |
|---|---|---|
| Core Server Engine | `Redbox.KioskEngine.Kernel` | 308 |
| Broker Services | `Redbox.BrokerServices.Kernel` | 1 |
| Hardware Services | `Redbox.HardwareServices.Kernel` | 60 |
| Update Services / Configuration Services | `Redbox.UpdateServices.Kernel` | 8 |
| Rental Kernel | `Redbox.Rental.Kernel` (see `rental.md`) | ~158 |

The core functions, grouped by source file in `src/KioskEngine/Redbox.KioskEngine.Kernel/`
(all names are prefixed with `kernel.` unless shown otherwise):

| File | Functions | Notes |
|---|---|---|
| `ActorFunctions.cs` | `createactor`, `removeactor(s)`, `clearactors`, `show/hideactor`, `enable/disableactor`, `markactordirty`, `setactortext/html/tag/image/opacity/location/x/y/width/height/dimension/frame`, `setactorforeground/background/gradienttargetcolor`, `setactorgradientangle`, `setactortextalignx/y`, `setrelativeactor`, `getactorx/y/width/height/tag/text/opacity/textregion`, `moveactorx/y`, `centeractorx/y`, `measureactortext`, `isactorimageset/visible/enabled`, `clearactorimage`, `registeractorhitnotify`, `executehitnotify`, `getactorsbytag`, `getactorbytaborder`, `get/settaborder`, `setactortabstops`, `setbordercolor`, `setborderstrokeweight`, `set/clearcornersize`, `get/setlasthitactortag`, `get/setlasthitactorname` | GDI scene graph manipulation. |
| `BundleFunctions.cs` | `activatebundle(name, version)`, `activatedefaultbundle`, `executescript(name)`, `gettable(name)`, `getdictionary(name)`, `getresourceproperty`, `getresourceaspectproperty`, `getresourcestoragename`, `isresourceoverridden`, `get/setfilter`, `get/setvendor`, `get/setmarket`, `get/setstorenumber`, `get/setculture`, `get/setenvironment`, `getruntime`, `getplatform`, `getapplication`, `get/setswitchstatusmessage`, `isculturevalid`, `load_xps_document` | `gettable` returns an array of row tables; `getdictionary` merges all columns of all rows into one table. |
| `CallbackFunctions.cs` | `callbackenqueue(func, parms, name)`, `callbackenqueuenobreak`, `callbacksuspend`, `callbackresume`, `callbackflush` | Defer a Lua call to the callback queue. |
| `DataStoreFunctions.cs` | `datastoreget/set/remove/clear/count/getall/getkeys/drop(store, table[, key[, value]])`, `datastoreclose(name)`, `datastorecloseall` | Key/value stores in `data\<store>.data` (7.3). |
| `DateFunctions.cs` | `dateadd`, `datecompare`, `stringdatecompare`, `getelapseddays/hours`, `datetimeparsefromstringdate/time`, `gettimezone`, `tolocaltime` | |
| `EnvironmentFunctions.cs` | `applysafeshutdown`, `applyunsafeshutdown`, `registerdatastoreerrorhandler`, `hasvalidcardreader`, `wait(ms)`, `handle_disconnected_usb_card_reader` | 3.5. |
| `FileFunctions.cs` | `readalltext`, `writealltext`, `replaceinfile`, `createfile`, `deletefile`, `deletefilefromrelativepath`, `tablefromfileserializeddictionary`, `tablefromfileserializedlistdictionary` | Unrestricted file access. |
| `ImageFunctions.cs` | `registerimagefromcache`, `createthumbnail`, `trimbitmapcache` | |
| `InputFunctions.cs` | `register/remove/clear` `idlenotify`, `activatednotify`, `clicknotify`, `doubleclicknotify`, `keypressnotify`; `show/hidemouse`, `getmousepos(screen)`, `setmousepos`, `mousesingleclick`, `mousedoubleclick`, `sendkeys`, `confirm`, `showmessage` | Handlers are stored in `EngineApplication`. `registerdoubleclicknotify` is a no-op in 1.88 (decompiled body only calls `ContainsKey`). |
| `LogFunctions.cs` | `log(msg)`, `logto(logName, msg)`, `logf(fmt, parms)` | `logto` targets a named log4net logger (`UserLoginLog`, `ControlPanelActionLog`, ...). |
| `MacroFunctions.cs` | `setmacro`, `getmacro`, `expandmacros` | `${Name}` macros used in view text and paths. |
| `MathFunctions.cs` | `round`, `random`, `formatasbytesize` | |
| `PathFunctions.cs` | `getdirectoryname`, `getfullpath`, `combinepaths`, `getpathextension`, `getfilenamefrompath`, `doesdirectoryexist`, `doesfileexist` | |
| `ProcessFunctions.cs` | `startprocess`, `starthiddenprocess`, `startprocesswaitforexit`, `killprocessbyid`, `listrunningprocesses`, `listpidrundll32s`, `showtaskmanager`, `showtaskrunner` | |
| `QueueFunctions.cs` | `queuecounts`, `queuecounts2`, `queuestartworker`, `queuestopworker`, `queueisworkerstarted`, `queueclear`, `queuegetallmessages`, `queuegetmessages(type)`, `queuedequeue(pred)`, `queuedeletemessage(id)`, `queuedeletemessagesbytype` | Outbound queue (7.2). |
| `RegistryFunctions.cs` | `getregistryvalue(path,key)`, `setregistryvalue`, `removeregistrykey`, `removeregistryvalue` | Absolute registry paths. |
| `SchedulerFunctions.cs` | `scheduler_services.start/shutdown/clear/add_entry/remove_entry/get_entries/get_entry` | Quartz cron jobs that call a Lua global function (7.4). |
| `ScreenSaverFunctions.cs` | `screensaverisactive/isrunning/gettimeout/settimeout/disable/enable` | |
| `SecureBrowserFunctions.cs` | `launchsecurebrowser(approvedUrls, navigateCallback)`, `closesecurebrowser` | Locked-down browser limited to a URL list (e.g. `approved_urls` table). |
| `SecurityServiceFunctions.cs` | `securitycreateuser`, `securityauthenticate`, `securityremoveuser`, `securityuserexists`, `securityisaccountlocked`, `securityuserslideexpirydate` | Local **Windows** accounts (`PrincipalContext(ContextType.Machine)`), used to cache technician logins. |
| `SessionAnalyticsFunctions.cs` | `analyticssessionexists`, `analyticsaddview`, `analyticsclose`, `analyticsdisposesession`, `analyticsaddnode(withattributes)`, `analyticsaddelement(s)`, `analyticsaddorupdateelement`, `analyticsremoveelement`, `analyticsaddattribute(s)`, `analyticsaddauthtransactionInfo`, `analyticsaddviewevent(2)` | Front-end to the rental `IAnalyticsService`. |
| `SettingsFunctions.cs` | `get/set/remove` `uservalue`, `machinevalue`, `uservalueordefault`, `machinevalueordefault`, `removeuserkey`, `removemachinekey`, `registerconfigurationcallback`, `removeconfigurationcallback` | Registry settings stores (8.2); callbacks receive `(store, action, path, key, value)` with `store` = `machine`/`user`, `action` = `set`/`removekey`/`removevalue`. |
| `ShoppingSessionFunctions.cs` | `sessionstartnew(2)`, `sessionaddevent(2)`, `sessionsetpostalcode(2)`, `sessionsetemail(2)`, `sessionend(2)`, `sessionsetcarttype(2)`, `sessionsetcurrent` | In-memory shopping sessions. |
| `SoundFunctions.cs` | `soundplaysync/async(resource)`, `soundstop`, `ttsrepeat` | |
| `StopwatchFunctions.cs` | `stopwatchcreate/remove/start/stop/isrunning/getmilliseconds/format` | |
| `SystemFunctions.cs` | `systemrenamecomputer`, `trimworkingset`, `setworkingsetsize` | |
| `TextToSpeechFunctions.cs` | `ttsenabled`, `ttsgettimeout` | |
| `TimerFunctions.cs` | `timercreate(name, due, period, handler)`, `timerstart/stop/remove/exists`, `timerstartall/stopall`, `timerwait`, `gettimerperiod`, `set_timer_period_and_duetime` | Soft timers pumped by the idle loop. |
| `TouchScreenFunctions.cs` | `toggletouchscreen`, `touchscreenstatus`, `touchscreendriver` | |
| `TweenFunctions.cs` | `tweencreate(name, type, begin, finish, duration, change, end)`, `tweenstart/stop/remove` | |
| `VersionFunctions.cs` | `fileversion`, `osversion`, `iswindows7` | |
| `ViewFunctions.cs` | `pushview`, `popview`, `popviewdiscard`, `peekview`, `peekpriorview`, `showview`, `reenterview`, `getviewframe`, `getviewrendertype`, `getlastshownview`, `setanalyticsviewmessage`, `safe_to_change_view`, `get_start_over_view_name`, `view_goback`, `view_goback_safe` | View stack (5.6). |
| `Win32ServiceFunctions.cs` | `getservicepath`, `startservice`, `stopservice`, `getservicestatus` | Windows services, e.g. `HalSvc$Default`. |

Extension functions:

| Extension | Functions |
|---|---|
| Broker (`Redbox.BrokerServices.Kernel/ReservationServicesFunctions.cs`) | `kernel.resvsvcs_cancel(storeNumber, referenceNumber, reasonCode, callback)` — forwards to `IKioskClientService.Transaction.CancelReservation`; the Lua callback is ignored (a `null` callback is passed). |
| Hardware (`Redbox.HardwareServices.Kernel/HardwareFunctions.cs`) | Jobs: `hardwareinint` (sic, init), `hardwarevend(evt, status, deck, slot, barcodes)`, `hardwarepreposvend`, `hardwarevendlocations`, `hardwareresetlocations`, `hardwarevendunknown`, `hardwarereturn`, `hardwarereturnunknown`, `hardwaresync`, `hardwaresynclocations`, `hardwaresoftsync`, `hardwaregetmachineinfo`, `hardwaregethardwarestatus`, `hardwarecheckpicker`, `hardwareqlmunload`, `hardwareqlmthin`, `hardwarevmzthin`, `hardwareqlmunloadandthin`, `prepforredeploy`, `hardwareprepunloadzone`, `hardwarereadfrauddisc`, `getnonthinsinvmz`, `vmzremovedthins`, `getbarcodesinbin`, `getinventorystats`, `vmzmerchsummary`, `filemarkerdisk`, `clearmerchstatus`, `getbarcodevmzposition`, `hardwareschedulejob(script, label, ...)`. Job control: `hardwaresuspendjob`, `hardwareresumejob`, `hardwaretrashjob`, `hardwareterminatejob`, `hardwareresumealljobs`, `hardwaresuspendalljobs`, `hardwaremaintenancemodesuspendjobs`, `hardwaremaintenancemoderesumejobs`, `hardwaregetjobs`, `hardwaregetsuspendedmaintenancemodejobs`, `is_active_hardware_job`, `hardwaregetprogramresults`, `hardwaregetjoberrors`. Direct: `hardwareexecuteimmediate(cmd, handler, clearStack)`, `hardwareexecuteservicecommand`, `hardwaregetinitstatus`, `hardwareisbusy`, `hardwarenotifyidle`, `hardwarenotifymaintmodestate`, `hasabedevice`, `irhardwareinstalled`, `enqueue_hardware_properties`, `sync_unknowns(force)`, `sync_unknowns_complete(jobId)`, `sync_empties`. Alerts (`HardwareAlertManagerServiceFunctions.cs`): `hardware_alert_manager_send_alert`, `..._send_alert2`, `..._send_alert_job`, `..._format_job_result`, `..._format_errors`. |
| Update (`Redbox.UpdateServices.Kernel`) | `kernel.config_getvalue(file, category, setting)`, `kernel.config_getsettingdata`, `kernel.config_getvalidconfigdata(file, category)`, `kernel.updmgrsvc_forcesubscriptionpublish`, `kernel.updmgrsvc_forcescheduleevent(name)`, `kernel.updmgrsvc_getsubscriptionstate`, `kernel.updmgrsvc_executecommand(cmd)`, `kernel.updmgrsvc_getdownloads`. |

Job-starting hardware functions return a Lua table `{ success = bool, job_id = string, errors = { [code] = description } }`
(`HardwareFunctions.CreateResultTable`). The two handlers are called as
`evt(jobId, dateTime, message)` and `status(jobId, status)` where status is a HAL `HardwareJobStatus`
name (`Pending`, `Running`, `Suspended`, `Stopped`, `Completed`, `Errored`, `Garbage`).

### 5.4 Example: a vend from Lua

```lua
local r = kernel.hardwarevend(
    function( job_id, when, message ) kernel.log( "HAL event: " .. message ) end,
    function( job_id, status ) if status == "Completed" then ... end end,
    nil, nil,                       -- no prepositioned deck/slot
    { "BARCODE1", "BARCODE2" } )
if not r.success then for code, desc in pairs( r.errors ) do ... end end
```

`HardwareService.Vend` schedules the HAL job (priority `Highest`; with a prepositioned deck/slot it
schedules the `vend` program and pushes `barcodes, slot, deck, count, "PREPOSITION-PICKER"` on the job
stack), then `ConfigureJob` subscribes to the job's `EventRaised`/`StatusChanged`, logs the HAL stack,
calls `job.Connect()` and `job.Resume()`. Events come back on HAL client threads and are enqueued as
`HardwareEventCallback`/`HardwareStatusChangeCallback`; on a terminal status the job is disconnected and
removed from the active list. The HAL side is described in `hal.md`.

### 5.5 Test plans

With registry `Store\Environment = Integration` and a manifest `test_control`, activation runs
`KernelService.ExecuteTestPlan`: the resource is XML with `<testset name=...>` elements containing
`<setup>`, `<execution>`, `<teardown>`, each with `<script value="path-in-bundle"/>` entries executed as
chunks. Between execution and teardown the bundle is re-activated. Output goes to the `TestServices`
logger (`logs\Test\testsvcs.log`). At the end the engine waits (max 5 min) for a safe point and exits.

### 5.6 Views

`ViewService` keeps a stack of view frame instances. `kernel.pushview(name)` + `kernel.showview()`
renders the top of stack: it sets the `view_name` filter, updates the scene, runs the view's `onEnter`
script, records analytics and text-to-speech, and posts `ShowView` activity to the view-health monitor.

A Lua/GDI view is a `view` resource:

```xml
<resource type="view">
  <property name="name" value="error_message_view" />
  <property name="version" value="3.0,Core,A" />
  <property name="active_flag" value="passive" />
  <aspect name="content">
    <scene name="Default" width="1024" height="768" backgroundColor="228,32,69"
           onEnter="error_message_view_enter" onLeave="error_message_view_leave">
      <actor name="message_label" x="85" y="220" width="700" height="400"
             text="${MessageText}" font="font_target_larger" foregroundColor="white"
             horizontalAlignment="near" optionFlags="Text" />
    </scene>
  </aspect>
</resource>
```

Scene attributes (`ViewFrame.Parse`): `name`, `width`, `height`, `render_type` (`GDI` default or `WPF`),
`clear` (default true), `cached`, `stylesheet`, `viewWindow`, `onEnter`, `onLeave`, `backgroundImage`,
`backgroundColor` (default `153,153,152`). Actor attributes: `name`, `enabled`, `style`, `x`, `y`,
`width`, `height`, `tag`, `text`, `onHit`, `visible`, `tabOrder`, `opacity`, `cornerSize`,
`strokeWeight`, `hotSpot`, `borderColor`, `gradientAngle`, `hitFlags`, `textRegion`, `backgroundColor`,
`foregroundColor`, `relativeToActor`, `cornerSweepAngle`, `textRotationAngle`, `gradientTargetColor`,
`textTranslationPoint`, `optionFlags`, `verticalAlignment`, `horizontalAlignment`, `WPFControlName`,
`WPFControlAssemblyName`. Missing values can come from a `stylesheet` resource style
(`enabled`/`disabled` states).

WPF views are registered in code by the UI assemblies and cached with `ViewService.CacheViewFrame`; they
are shown in the `ElementHost` of the default scene. Most of the 1.88 Rental UI is WPF (`rental.md`).

---

## 6. Kernel extensions and the services they provide

### 6.1 Extension contract

```csharp
public interface IKernelExtensionHost : IDisposable {
    IKernelExtension Extension { get; set; }
    ErrorList PreLoad();     // engine Initialize, before configuration
    ErrorList Load();        // after core services are registered
    ErrorList Reset();       // every bundle activation: register kernel functions
    ErrorList Activate();    // after a successful switch
    ErrorList Deactivate();  // before a switch / at shutdown
    void HandleHostCrash(Exception e);
    bool CanSwitch(out ErrorList errors);
    void RegisterCommunicationActivity(Action<object, EventArgs> action);
}
```

In 1.88 extensions are **statically linked**: the list `BrokerServicesHost, HardwareServicesHost,
UpdateServicesHost` is hard-coded in `EngineApplication.Initialize`. The `KernelExtensionSearchPath`
registry value (default `..\kext`) is still defined but never scanned. Earlier versions loaded `.kext`
files from that folder; `afteractivate.lua` in the image deletes the leftover
`kext\Redbox.DVD.TicketServices.dll` and `kext\ticket-services.kext`.

### 6.2 Hardware Services (`HardwareServicesHost`)

- **Load**: `HardwareService.Initialize()` registers `IHardwareService` and `IHardwareState`, and creates a
  `Redbox.HAL.Client.HardwareService(IPCProtocol.Parse(url))` where `url` is registry
  `Remote Services\Store Services\HardwareServiceURL` (default `rcp://localhost:7001`; image log shows
  `rcp://127.0.0.1:7001`). Timeout `HardwareServiceTimeout` default 600000 ms. Initialises
  `HardwareAlertManagerService`. Adds preference pages "Hardware Services/General", "Credit Card Reader",
  "Device Service/General", "Device Service", "Device Service Simulator" (and Return/Vend/Statistics/
  Hardware State pages only if `IsTest()`, which is hard-coded false).
- **Reset**: macros `HALVersion` (client assembly version) and `HALServiceVersion` (registry
  `HKLM\SOFTWARE\Redbox\HAL\Product Version`), recreate the HAL client, disconnect and forget all active
  jobs, register the hardware kernel functions.
- **CanSwitch**: false while any return job is Pending/Running/Suspended or an `ExecuteImmediate` is in
  flight.
- Services: `IHardwareService` (all methods in `Redbox.HardwareServices.Proxy/HardwareService.cs`: one per
  HAL job type, job control, `ExecuteImmediate` (serialised by a lock, runs on the thread pool, result
  delivered through a callback with the HAL stack), `ExecuteServiceCommand`, scheduler status,
  maintenance-mode job suspension, correction statistics, IR hardware detection, router/touch-screen
  resets), `IHardwareAlertManagerService` (formats job results and sends `IHealthServices.SendAlert`, e.g.
  type `TitleEmpty`), `ISyncUnknownProcessor`/`ISyncEmptiesProcessor` (schedule location syncs for
  unknown/empty slots; the last unknown-sync date is in registry
  `Local Services\Hardware Services\LastUknownSyncDate`).

### 6.3 Update Services (`UpdateServicesHost`)

- **PreLoad**: `ConfigurationService.Initialize()` → `Redbox.UpdateManager.Client.ConfigurationClient`
  (update manager at `rcp://localhost:7004`, 60 s timeout) and registers `IConfigurationService`
  (Section 8.1).
- **Load**: `UpdateManagerServiceProxy.Initialize()` → `IUpdateManagerService` over
  `Remote Services\Store Services\UpdateManagerServiceURL` (default `rcp://localhost:7004`, timeout
  `UpdateManagerServiceTimeout` 600000 ms). Adds two preference pages and starts the **engine IPC
  listener** thread.
- **Reset**: registers the update and configuration kernel functions.
- **CanSwitch**: always true.

`IUpdateManagerService` methods map to update-manager text commands (`Redbox.UpdateManager.Client/UpdateManagerService.cs`):

| Proxy method | Command sent to `rcp://localhost:7004` |
|---|---|
| `ForceScheduleEvent(name)` | `schedule-event force name: '<name>'` |
| `GetDownloadState` | `client list-current-downloads` |
| `GetSubscriptionState` | `client get-dvd-data-state` |
| `ForceSubscriptionPublish` | `client force-data-files` |
| `ExecuteCommand(cmd)` | `<cmd>` verbatim |
| (configuration client) activate staged config | `configfile activate configfileid:<OID>` |

#### Engine IPC listener (`rcp://localhost:8004`)

`IPCServiceHost.Create(IPCProtocol.Parse(url), product, version, copyright, 3, 3, 8192, 64, 5)` with
`url` = registry `Local Services\Update Services\KioskEngineServiceUrl` (default `rcp://localhost:8004`).
The command repository scans `Redbox.DVD.UpdateServices.dll`, `Redbox.UpdateServices.KernelExtension.dll`,
`kioskengine.exe`, `Redbox.IPC.Framework.dll` for `[Command]` classes. Framing, the generic `help`,
`macro`, `script`, `statistics`, `command-repository` commands and the response format are documented in
`common-libraries.md` (IPC.Framework). Engine-specific commands (`EngineCommand.cs`):

| Command | Action |
|---|---|
| `engine ping` | No-op (connectivity check). |
| `engine shutdown` | `CanShutDown()` ? exit : flag for safe shutdown. |
| `engine load-control-panel` | If `CanRestart()` (no active rental session, extensions idle), `Activate("Control Panel Application", "*")`. |
| `engine reset-bundle` | If `CanRestart()`, re-activate the current bundle. (Declared `internal static void ResetBundle(string[])`, unlike the other instance methods; whether the IPC framework can dispatch it is not verified.) |

The update manager client also has methods that send `engine load-bundle bundle: '...'`,
`engine show-offline-screen`, `engine bring-to-front`, `engine start [bundle: '...']`,
`engine execute-script name: '...'`, `engine export-queue path: '...'`,
`engine export-sessions path: '...'`; the 1.88 engine has **no handlers** for these.

Example (log of the image, update manager loading the Control Panel):

```
17:19:34,755|IPC Worker Pool #0|INFO |engine load-control-panel
17:19:34,756|IPC Worker Pool #0|INFO |Engine Command: load-control-bundle
17:19:34,771|1|INFO |Beginning ResourceBundleService.Switch
17:19:34,797|1|INFO |Deactivate resource bundle: ...\bundles\ra-2.x.bundle
17:19:39,834|1|INFO |Activate resource bundle: ...\bundles\cp.bundle
```

### 6.4 Broker Services (`BrokerServicesHost`)

Legacy online-reservation channel. The 1.88 rental code uses an AWS broker/Kiosk Client Service path
instead (`IReservationService`, config `Jarvis/UseAWSBrokerService = True` in `kioskengine.config`;
see `rental.md`); the legacy broker connects only if `IConfiguration.UseLegacyBrokerService` is true.

- **Load**: registers `IReservationServices` = `ReservationServicesProxy`; preference page
  "Broker Reservation Services/General" (registry path `Remote Services\Reservation Services`).
- **Reset**: registers `kernel.resvsvcs_cancel`; `BrokerServicesProxy.Reset()` disconnects.
- **CanSwitch**: false while a reservation request is being processed (`BSH666`).

Protocol: .NET Remoting over **GenuineChannels** TCP (`GenuineTcpChannel`, binary formatter) to
`Remote Services\Reservation Services\BrokerServicesURL`, default
`gtcp://blt.accessredbox.net:7890/RegistrationBroker/Default.rem`. Channel properties: `ConnectTimeout`
5000, `MaxTotalSize`/`MaxContentSize`/`MaxQueuedItems` 500000, `ReconnectionTries` 1,
`InvocationTimeout` = `BrokerServicesTimeout` (default 30000), `MaxTimeSpanToReconnect` 30000,
`SleepBetweenReconnections` 5000, `PersistentConnectionSendPingAfterInactivity` 20000.
No authentication beyond the transport.

Sequence:

1. The rental code calls `IReservationServices.Register(storeNumber, prices, reserveCallback, cancelCallback)`.
   A JSON "version" string is built:
   ```json
   {"Runtime":"Kiosk Engine","Version":"1.88.0.80",
    "PricingPolicy":{ "<group>": { "<sub>": { "<name>": 1.99 } } },
    "GiftCardsEnabled":false,"RecoOnPickup":false,"AuthorizeAtPickup":true,
    "CancelReservation":true,"Features":1}
   ```
   (`GiftCardsEnabled`/`RecoOnPickup` from `system/General/GiftCardsEnabled` and
   `RecommendationOnPickup`; `Features` flags: `MultiDiscVend = 1`.)
2. The kiosk obtains the server object `IKioskRegister` at the URL and calls
   `Register(int kioskID, IKioskOperations kiosk, string kioskVersion)`, passing **itself**
   (`BrokerServicesProxy : MarshalByRefObject, IKioskOperations`) as a callback object.
3. The server later calls back on the kiosk:
   - `ReservationResult Reserve3(ReservationCart3 cart)` (`Reserve`/`Reserve2` return null),
   - `CancelReservationResult CancelReservation(long referenceNumber)`,
   - `void Ping()`.
   Each call is queued to the UI thread as a callback entry and the remoting thread blocks until the
   rental callback returns.
4. On GenuineChannels events `GeneralServerRestartDetected`, `GeneralConnectionClosed` or
   `HostResourcesReleased`, a reconnect timer re-registers after 1 s and then every 60 s.

DTOs (`src/KioskEngine/Redbox.Services.KioskBrokerServices.KioskShared`, all `[Serializable]`):

```text
ReservationCart3 { int KioskID; long ReferenceNumber; string CardID; bool IsGc; List<string> GcIds, CcIds;
  string Email; DateTime ReserveDate; List<ReservationTitle3> Titles; decimal DiscountedSubTotal, SubTotal,
  Tax, GrandTotal, HiveOnlinePromotionDiscount; int NumberOfCreditsRemaining; string PromoCode,
  CustomerNumber; bool AppliedCredit, AppliedHiveOnlinePromotion, AppliedTitleMarketing;
  List<int> HistoryTitleIDs; decimal TaxRate; string Zip; bool IsMnp, IsMdv, LoyaltyRedemption;
  decimal DefaultServiceFeeAmount, ActualServiceFeeAmount; bool AuthorizeAtPickup;
  ReservationCardHash ReservationCardHash; List<ReservationCardHash> AlternateCardHashes }
ReservationTitle3 { int ItemID, TitleID; ReservationType ReservationType; decimal Price, Discount;
  DiscountType DiscountType; decimal DiscountedPrice; int NumberOfCredits; decimal LoyaltyPoints }
ReservationCardHash { string CardBinHash, Last4 }
ReservationResult { bool ReservationSucceeded; string ErrorMessage; List<TitleResult> TitleResults }
TitleResult { int ItemID; bool Reservable; string ReservedBarcode; List<DiscStatus> UnreservableDiscs }
DiscStatus { string Barcode; int DiscTypeID }
CancelReservationResult { bool CancellationSucceeded; string ErrorMessage }
enum ReservationType { _Unspecified, Rental, Purchase }
enum DiscountType { None, Promotion, Loyalty, Credit }
enum DiscType { _Unspecified, Known, Empty, Unknown, Thinned, Damaged, WrongTitle, Reserved }
enum DiscFormat { _Unspecified=0, DVD=1, BluRay=2, Xbox360=3, PS2=4, DigitalCode=17 }
```

`Reserve3` groups title IDs by `"rental"`/`"purchase"`, invokes the rental reservation callback, and
maps reserved items back to `TitleResult` by `TitleID` (`Reservable = barcode != "empty"`). The request is
logged with `Email`, `CardID`, `CardBinHash` and `Last4` masked with `x`.

### 6.5 Kiosk Data Services and Fraud Services proxies

These are plain libraries (no extension host). `KioskSessionJsonAnalyticsService` (registered in
`EngineApplication.Initialize`) registers a queue processor for message type `AddKioskSessionJson`.
The rental `AnalyticsService.SendToKioskDataService` enqueues, with priority
`system/QueueService/KioskAnalyticsMessagePriority` (default 0):

```json
{ "KioskId": 12345, "SessionId": "45d06b3a-...", "StartTime": "2025/01/30 17:00:01",
  "EndTime": "2025/01/30 17:01:50", "IsUnCompressed": false, "SessionData": "<base64>" }
```

`SessionData` = base64( little-endian int32 uncompressed length ‖ gzip(UTF-8 session JSON) )
(`StringExtensionMethods.CompressString`).

When the queue worker dequeues it (and the application is not in stand-alone mode),
`KioskDataServicesProxy` sends:

```
POST {KioskDataServicesUrl}/api/KioskData/AddKioskSessionJson
Content-Type: application/json
Authorization: Bearer <KioskDataServicesKey>
x-redbox-kioskid: <kiosk id>
x-redbox-activityid: <SessionId from the body>

<the queued JSON>
```

`KioskDataServicesUrl` and `KioskDataServicesKey` come from `system.config` category `General`
(read through the rental `IConfiguration`). The response body is URL-decoded and parsed as a JSON
dictionary; the message counts as processed when it contains `"success": true`. The
`AWSBaseMessageProcessorService<S,D>` base class (re-serialises the message to another DTO before
sending) exists but has no subclass in 1.88.

`Redbox.FraudServices.Proxy.BarcodeTrackingService` implements `IBarcodeTrackingService` by logging the
tracking info only; fraud reporting goes through the Kiosk Client Service in 1.88 (`rental.md`).

### 6.6 Kiosk Services 1.81 (not loaded)

`Redbox.KioskServices.KernelExtension` (1.81.0.27) is shipped in `bin` but not referenced by the 1.88
engine and does not compile against ComponentModel 1.88. When it was used it registered
`IHealthServices`, `IPromotionServices`, `IPersonalizationServices`, `ICustomerOfferService`,
`ILoyaltyServices`, ten preference pages under `Remote Services\...`, kernel functions
(`kernel.authsvcs_*`, `kernel.healthsvcs_*`, `kernel.storesvcs_*`, `kernel.getsoftwareversions`), and
the `KioskServices` (default) and `SoftSync` queue processors. Its transport
(`Redbox.KioskServices.Proxy/KioskServicesProxy.cs`):

- URL registry `Remote Services\Store Services\KioskServicesURL`, default
  `https://bl2.accessredbox.net/KioskServices/MessageService.rem`; `KioskServicesProtocol` `Remoting`
  (default) or `REST`; `KioskServicesTimeout` 30000.
- Remoting: `HttpChannel` with binary formatter, `IMessageService.Process(string jsonRequest)` returns a
  JSON string.
- REST: `POST <url>`, `Content-Type: application/x-www-form-urlencoded`, body
  `payload=<url-encoded JSON>`; response is URL-decoded JSON.
- Requests are JSON objects with a `MessageType` (`Authenticate`, `DiscountCodeValidation`,
  `EmailConfirm`, `GetCustomerOffers`, `GetOpsMarket`, `GetPendingBanners`, `GetPendingKiosks`,
  `GetPendingStates`, `KioskOptIn`, `UpdateAccount`, ...); responses are dictionaries with `Success`.

In 1.88 the same kernel function names (`authsvcs_authenticateuser`, `healthsvcs_*`, `storesvcs_*`) are
re-implemented in `Redbox.Rental.Kernel` on top of the Kiosk Client Service (`rental.md`,
`kiosk-client.md`).

---

## 7. Data and storage

### 7.1 Files written by the engine

| File | Format | Purpose |
|---|---|---|
| `bin\message.data` | VistaDB 4 database | Outbound message queue (7.2). |
| `bin\session.data` | VistaDB (legacy) | Deleted at every start; the `Migrations.session.data.*.sql` scripts (Session, SessionEvent, ShoppingCart, ShoppingCartLineItemGroup, ShoppingCartLineItem, ShoppingCartLineItemPrice, SessionError tables) are still embedded but no longer used for persistence. |
| `data\<store>.data` | VistaDB, or a `Redbox.Compression` archive | Lua data stores (7.3). |
| `data\UnhandledException.txt` | ASCII JSON `{"Message","Exception"}` | Crash marker, reported at next start. |
| `cache\*` | raw | `IFileCacheService` (`SetFileContent`/`GetFileContent`), created on demand. |
| `screenshots\screenshot_<n>.png` | PNG | F1 key. |
| Registry `HKLM\SOFTWARE\Redbox\REDS\Kiosk Engine\...` | values | Machine settings (8.2). Because the process is 32-bit, on 64-bit Windows this is probably under `WOW6432Node` (inferred, not checked). |

### 7.2 Message queue (`QueueService`)

Schema (`Migrations.message.data.0000001.sql`):

```sql
CREATE TABLE Queue(
    ID int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Priority tinyint NOT NULL,
    Type varchar(32) NOT NULL,
    Data image NOT NULL,
    CreatedOn datetime NOT NULL DEFAULT 'GETDATE()' );
```

plus `System([Key] varchar(16) PK, Value varchar(256))` holding `SchemaVersion` (the image is at 1).
The database is packed at start and opened `ExclusiveReadWrite`. Migration scripts are embedded resources
named `Redbox.KioskEngine.Environment.Migrations.<file name>.<NNNNNNN>.sql`, applied in order
(`SchemaMigrator`).

- `Data` = `"RedboxEncrypted"` (ASCII) ‖ TripleDES( ASCII(JSON of the message object) ), using the fixed
  key/IV in `Redbox.Core.ByteArrayExtensions.Encrypt` (`common-libraries.md`). Readers accept
  unprefixed plain data too.
- Priority bands (`QueueService.Initialize`):

  | Band | Priorities | Active window |
  |---|---|---|
  | Peak | 100–255 | `system/QueueService/PeakPriorityOnlyTimeRange` (`h:mm-h:mm`), default 14:00–22:00; the image log shows 18:00–22:00 |
  | offPeak | 50–99 | any time outside Peak |
  | overNight | 0–49 | `OvernightPriorityTimeRange`, default 01:00–05:00 |

  The current band with the lowest minimum that contains the current time is selected; only messages
  with `Priority >= band minimum` are eligible
  (`SELECT TOP 1 ... WHERE Priority >= @PriorityValue ORDER BY Priority DESC, ID ASC`). So low-priority
  messages (e.g. analytics at priority 0) are only sent overnight.
- Worker: a `System.Threading.Timer` every 60000 ms (`QueueReadPeriod`), started after each successful
  bundle switch, processes **one** message per tick. Processors are matched by
  `IQueueMessageProcessorService.SupportedMessageTypes` (fallback: a processor with `IsDefault`), and get
  `BeforeMessageProcessing`, `ProcessMessage`, `MessageProcessedSuccessfully`, `AfterMessageProcessed`.
- The message is **deleted after the attempt whatever the result** (`QueueService.Dequeue`); retry logic
  is up to the processor.
- Error code `Q999` (VistaDB exception) triggers `HandleCorruptDatabase`: raise the corrupt-DB
  notification (maintenance mode, reason `SOFT-FILE01`) and send a `KioskAlert` directly through
  processors named `KSKioskAlertMessageProcessorService` or `AWSKioskAlertMessageProcessorService`
  (neither exists in 1.88, so in practice only the notification happens). On open failure the file is
  renamed `message.data.corrupt` (`QueueServiceVistaDBData`).
- Processors registered in the image log: `AddKioskSessionJsonMessageProcessorService` (6.5) and
  `KcsUpdateMerchStatusMessageProcessorService` (rental).
- `ExportToXml(file)` (`ExportQueue` `WM_COPYDATA` command) writes `<message priority=... >` elements.

### 7.3 Lua data stores

`kernel.datastore*(store, table, key, value)` → `DataStoreProvider.GetProvider(store)`: if
`data\<store>.data` exists and is a valid `Redbox.Compression` archive, a read-only
`ProductLookupDataStoreProvider` is used (product catalog files shipped by the update system);
otherwise a VistaDB database is created/opened (`NonexclusiveReadWrite`) with one table per Lua table,
columns `Key varchar(250)` (primary unique index `IX_Key`) and `Value ntext`
(`VistaDbDataStoreProvider`). On errors the Lua handler registered with
`kernel.registerdatastoreerrorhandler` is called with the store name.

### 7.4 Scheduler

`scheduler_services.add_entry(jobname, label, startTime, endTime, cron, functionName, programName,
misfireInstruction, parameters)` adds a Quartz.NET cron trigger. Entries are persisted as a JSON array in
registry `Core\SchedulerEntries` (default `[]`). When a trigger fires, `SchedulerJob` calls the Lua
global `functionName` with the parameters (as strings) on the UI thread.

### 7.5 Card data encryption key

`TrackDataEncryptionService` encrypts card account numbers/track 2 for the legacy (non device-service)
card path. If `system/General/CertificateName` is set (image: `202302`) a store certificate is used
(`data\certs\<name>.cer` — inferred from the file names in `data\certs`); otherwise the RSA public key
from the bundle resource named by the manifest's `public_key` property (`publickey` resource,
`<RSAKeyValue>` XML, `key_id` property) is used. See `device-service.md` for the EMV path.

---

## 8. Configuration

### 8.1 Update-manager configuration files (`data\*.config`, `IConfigurationService`)

JSON files written by the update manager (`update-manager.md`) and read by
`Redbox.UpdateManager.Client.ConfigurationClient`:

```json
{"OID":1,"Name":"system.config","Path":"C:\\Program Files\\redbox\\reds\\Kiosk Engine\\data","GenerationOID":1171,
 "ConfigurationData":[
  {"CategoryID":1,"CategoryName":"General","SettingName":"DefaultBundle","SettingDataType":"string",
   "DefaultTypeName":"all","DefaultTypeRank":0,"DefaultTypeDataType":0,"DefaultTypeKey":0,"DefaultTypeValue":"*",
   "Value":"Rental Application,*",
   "EffectiveTime":{"theDate":634609728000000000,"dateTimeKind":"Unspecified","dateString":"2012-01-01T00:00:00.0000000"},
   "ExpireTime":null}, ...]}
```

- Lookup key: (file key, category, setting). Each setting can have several rows targeted at different
  scopes (`DefaultTypeName` `all` rank 0, `market` rank 4 with `DefaultTypeValue` e.g. `Austin, TX`, ...).
  The effective value is the row with `EffectiveTime <= now < ExpireTime`, ordered by
  `DefaultTypeRank` descending then `EffectiveTime` descending.
- `SettingDataType` names: `bool`, `boolean`, `string`, `integer`, `int`, `long`, `float`, ...; byte
  arrays are base64.
- Every 120 s the client checks each file; if `<name>.configstaged` exists, it reloads the file, sends
  `configfile activate configfileid:<OID>` to the update manager and calls the registered activate action.

Categories present in the image:

| File | Categories (settings) |
|---|---|
| `system.config` | `General` (51 settings, e.g. `DefaultBundle`, `CertificateName`, `KioskDataServicesUrl`, `KioskDataServicesKey`, `TextToSpeechEnabled`, `GiftCardsEnabled`, `RecommendationOnPickup`, `UnloadDelayMinutes`, `VMZMerchFlowTimeOut`, `SendKioskAlertsToKioskServices`, ...), `Filters` (`alternate_start_view`), `Games`, `Culture`, `Documents`, `Personalization`, `QueueService` (`KioskAnalyticsMessagePriority`, `OvernightPriorityTimeRange`, `PeakPriorityOnlyTimeRange`, `UploadDiscImageMessagePriority`), `Fraud`, `DeviceService`, `ZeroTouch` |
| `kioskengine.config` | `Jarvis` (27 feature switches `Send...ToKioskClientService`, `UseAWSBrokerService`, `UseKCSPricing`, `UseKCSInCart`, `UseKCSStartScreen`, timeouts) |
| `kioskhealth.config` | `Kioskhealth` (28 health-monitor settings) |
| `marketing.config` | `Collections`, `Banner`, `MarketingGeneral`, `AB Testing`, `MarketingReturn`, `MarketingAd` |
| `versionsneeded.config` | `FullPath` (8 required component versions) |

Engine-level settings read from it: `system/Store/DefaultBundle` (the code's key; the image stores the
value under `General` — see Unknowns), `system/General/TextToSpeechEnabled`, `system/General/SendAppCrashAlert`,
`system/General/CertificateName`, `system/Filters/*`, `system/QueueService/*`,
`system/KioskEngineApiService/KioskEngineApiServiceBaseUrl` (default `http://localhost:9002`),
`system/KioskClientService/KioskClientServiceBaseUrl` (default `http://localhost:9001`),
`system/UpdateClientService/UpdateClientServiceBaseUrl` (default `http://localhost:6001`). Most other
keys are consumed by the rental code (`rental.md`). In this image the service URLs in `system.config`
have been re-pointed to a non-Redbox host (`<replacement host>`); credential values are not reproduced
here (`<redacted>`).

### 8.2 Registry settings (`IMachineSettingsStore`, `IUserSettingsStore`, preference pages)

Root `SOFTWARE\Redbox\REDS\Kiosk Engine` under HKLM (machine) or HKCU (user). `GetValue(path, key,
default)` / `SetValue` with change notifications to Lua (`kernel.registerconfigurationcallback`).
Preference pages (`IPreferenceService.AddPreferencePage(name, settingPath, displayPath, target, host)`)
are views over a registry sub-key; `target = LocalSystem` → HKLM. Keys used by engine and kernel
extensions:

| Sub-key | Value | Default | Meaning |
|---|---|---|---|
| `Core` | `BundleSearchPath` (string[]) | `..\bundles` | Bundle folders |
| `Core` | `KernelExtensionSearchPath` (string[]) | `..\kext` | Unused in 1.88 |
| `Core` | `DefaultBundle` | — | Written by `DefaultBundleName` setter; compared to `NONE` in `SelectBundle` |
| `Core` | `SchedulerEntries` | `[]` | Quartz entries (JSON) |
| `Core` | `BackgroundColor` | `153,153,152` | Host background |
| `Core` | `CheckCardReader` | true | Card-reader detection |
| `Core` | `DebuggerEnabled` | false | Lua debugger |
| `Core` | `EnableMouseCursor` | true | |
| `Core` | `NullKioskId` | 1 | |
| `Store` | `ID` | — | Kiosk/store number |
| `Store` | `Environment` | `Production` | Bundle filter `environment`; `Integration` enables test plans; non-`Production` enables F2/F5/F6 and test-card keys |
| `Store` | `Market`, `Banner`, `OutOfOrder`, `VerboseLogging`, `QlmDeck`, `WatchDogEnabled`, `RunInitialSync`, `InitialSyncType`, `SynchronizeTimeToServer` | | Read by bundles / Control Panel |
| `Remote Services\Store Services` | `HardwareServiceURL`, `HardwareServiceTimeout` | `rcp://localhost:7001`, 600000 | HAL |
| `Remote Services\Store Services` | `UpdateManagerServiceURL`, `UpdateManagerServiceTimeout` | `rcp://localhost:7004`, 600000 | Update manager |
| `Remote Services\Reservation Services` | `BrokerServicesURL`, `BrokerServicesTimeout` | `gtcp://blt.accessredbox.net:7890/RegistrationBroker/Default.rem`, 30000 | Legacy broker |
| `Local Services\Update Services` | `KioskEngineServiceUrl` | `rcp://localhost:8004` | Engine IPC listener |
| `Local Services\Hardware Services` | `LastUknownSyncDate` | "" | Sync processor |
| `Remote Services\Device Service Simulator` | `UseSimulatedCardReader`, `IsCardReaderConnected`, ... | false | Use `DeviceServiceClientHelperSimulator` |
| `Ide` | `RecentProjects`, `DefaultProjectLocation` (`C:\Projects`), `ShowChrome`, `ShowIDEInTaskBar`, `ShowKioskEngineInTaskBar` | | IDE |
| `Remote Services\{Health, Promotion, Loyalty, Personalization, Marketing Optin, Customer Offer, Authentication} Services` | `LiveCall`, `Simulate*`, ... | | Simulation switches used by rental services |

### 8.3 `kioskengine.exe.config`

Only `DevDataPath` (`C:\Program Files\Redbox\reds\Kiosk Engine\data`) and `DevBundlePath`, both used only
with a debugger attached; an empty WCF `basicHttpBinding`; binding redirects (Newtonsoft.Json 12,
System.Web.Http 5.2.7, ...).

---

## 9. Developer and technician applications

### 9.1 Host-form hot keys (`HostForm.OnKeyDown` / `OnKeyPress`)

| Key | Action | Restriction |
|---|---|---|
| Esc | Close the engine | none |
| `` ` `` | Show the bundle selector (`SelectBundleForm`; can edit search path and "break on start" in the debugger) | Non-rental bundle, or rental on `start_view`/`welcome_view`/`maintenance_view` |
| F1 | Screenshot to `..\screenshots` | |
| F2 | Manage views (view stack) | environment ≠ Production |
| F4 | Preferences dialog (`PreferencesManager`), after a password check | Only if the active bundle has a `config` table with column `F4PasswordHash` (the `cp` bundle does: `<redacted>`). The typed password is checked as `base64(SHA256(SHA256(UTF-8 password)))`. |
| F5 | IDE | environment ≠ Production |
| F6 | Manage timers | environment ≠ Production |
| F8 | Toggle touch screen | |
| F10 | Toggle an "ADA line" guide at y=286 | |
| F11 | Toggle mouse cursor | |
| F12 | From `product_detail_view`, push `title_details_dev_view` | |
| Home / End | Text-to-speech audio device connected / disconnected | |
| (number keys) | Inject canned test track data via `SendKeys` | environment ≠ Production and classic track parsing enabled |

### 9.2 IDE (`Redbox.KioskEngine.IDE`)

A WinForms IDE embedded in the engine (F5): project explorer over the active bundle, resource editors
(text with Lua/XML/HAL syntax files `Lua.syn`, `XML.syn`, `HALScript.syn`, table editor, bitmap editor),
"Build Bundle", Lua debugger (enable debugging, break all, step into/over/out, breakpoints, call stack,
locals, globals, immediate window, legacy debugger), timers, view stack, bundle filters, error list,
output, properties. `IdeService.OpenProject`/`Build` return empty error lists in 1.88 (stubs).
Preference pages in this assembly: `GeneralPreferencePage` (`Engine Core/General`, registry `Store`),
`IdePreferencePage`, `ResourcePreferencePage`, `DataStorePreferencePage`, `QueuePreferencePage`,
`QueuePrioritiesPreferencePage`, `ServerEnginePreferencePage` (`rcp://localhost:7008`, unused service).
The F4 `PreferencesManager` dialog shows all registered preference pages as a tree and has a button to
start `Redbox.KioskEngine.PreferenceManager.exe`.

### 9.3 Control Panel and Field Maintenance applications

Both are **application bundles** run by the same engine; the bundle switch is the only way between
them and the Rental Application.

**Control Panel Application** (`cp.bundle`, "Redbox Desktop", technician desktop):

- Entered via `engine load-control-panel` (update manager / KioskShell) or from the Rental/FMA bundles.
- `application_startup` calls `kernel.showlogin()` (rental kernel function) → `ControlPanelFunctions.ShowLogin`
  → WPF `control_panel_login_view`. Credentials are checked with
  `IAuthenticationServices.Authenticate(storeNumber, id, password, useLdapAuthentication: true, cb)`
  (implemented by rental on the Kiosk Client Service). Cancel → `ActivateDefaultBundle`.
  (The bundle also contains an older Lua login flow, commented out, that caches successful logins as
  local Windows accounts with a 14-day expiry via `kernel.securitycreateuser`.)
- `control_panel_view` (`Redbox.ControlPanel.UI/Views/ControlPanelViewUserControl.xaml`) buttons:
  Start Kiosk, Watch Dog toggle (registry `Store\WatchDogEnabled`), Secure Browser (URLs from the
  `approved_urls` table — Production: a field-visit form URL with `${StoreNumber}`), Start Field
  Maintenance (`Activate("Field Maintenance Application","*")`), HAL Tester, touchscreen toggle, ELO PCap
  control, folder shortcuts (C:\, Kiosk Logs, Kaseya config, camera folder under the `HalSvc$Default`
  service path, Redbox folder, engine logs), Windows tools (reboot, network, date/time, computer
  management, notepad, kill process, display, device manager, event viewer, add/remove programs, command
  prompt, control panel), maintenance-mode toggle (`IHardwareService.SuspendAllJobsForMaintenance` /
  resume), Configure Kiosk, Out of Order toggle, HAL service start/stop (`halsvc$default`), Close Kiosk.
  Each action is logged to `ControlPanelActionLog` and sent as an audit event
  (`ProcessFunctions.PerformActivityLogAndSendAuditEvent`).
- Idle timeout (Lua, 600000 ms) returns to the rental bundle unless `HALMGMTC` or `MSHALTESTER` is running.

**Field Maintenance Application** (`fm.bundle`, mostly Lua/GDI views plus `Redbox.FMA.UI`):
technician inventory work — login, main menu, inventory grid and filters, insert/remove discs, QLM
unload/thin, VMZ merchandising flow, sync scheduling (inventory, unknowns, redeployment, fraud),
job control (suspend/resume/trash), keypad/keyboard pop-ups, version info, touch-screen calibration,
HAL tester launch. It drives the robot through the hardware kernel functions. `Redbox.FMA.UI` adds WPF
views `select_planogram_view`, `show_planogram_view`, `fma_popup_message_view`, which list/save
planograms through `IKioskClientService.Planogram` (`GetActivePlanograms`, `SaveSelectedPlanogram`,
`SyncPlanograms(forceRefresh)`). Idle timeout (20000 ms in the script) sends an `FMA_Timeout` audit
event and runs `activate_rental`. Actions log to `FieldMaintenanceActionLog`.

### 9.4 Preference Manager (`Redbox.KioskEngine.PreferenceManager.exe`)

Stand-alone WPF tool (not loaded by the engine) for diagnostics. Pages (via `[PreferencePage]`):
Engine Core/Store and Versions (engine API `GET api/enginecore/getstoreinfo`,
`getkioskengineversions`), Kiosk Data Service/Health Service, Kiosk Client Service proxy/transaction
pages (login results, loyalty, marketing, personalization, promotion, store services, user
authentication, authorize results, after-swipe offers, gift-card authorize), Update Client Service pages
(configuration changes, reservations, ping service). It calls `http://localhost:9002` and
`http://localhost:9001` (hard-coded base URLs).

### 9.5 `redscc` — bundle compiler

`redscc.exe` ("REDS Bundle Compiler", 1.1.4.1, .NET 3.5; usage string calls it `kecc`):

```
kecc [--quiet] [-D:property=value ...] --output|-o[:|=]filename --path[:|=]bundle-path
```

1. Require `manifest.resource` in `--path` (`C003`, `C004`).
2. If `-D` properties are given, copy the manifest to `%TEMP%\manifest.resource` and overwrite the
   `value` attribute of matching `<property name=...>` elements (e.g. `-D:product_version=3.55.0.27`).
3. Parse every `*.resource` as XML (`C999` on failure).
4. Create the zip bundle: all `*.resource` files plus every other file under the path recursively except
   `*.resource` and `*.build` (`C120` if the file cannot be created).
5. Re-open the bundle, `LoadResources()`, and compile every `script` resource with
   `assert(loadfile(tmp))` in a Lua VM (`C671` compile error, `C672` warning for empty scripts).

Exit code 0 on success, 1 on errors.

### 9.6 Engine Web API (`http://localhost:9002`)

Self-hosted Web API with attribute routing and Swagger UI (Swashbuckle; `swagger` default routes). **No
authentication.** Responses (except `api/engine/*` and `api/kioskhealth/*`, which return their own
types) use:

```json
{ "Success": false, "Errors": [ { "Code": "...", "Message": "..." } ],
  "HostName": "<machine name>", "StatusCode": 200, "ResponseTime": 12, "Content": { ... } }
```

| Method | Route | Content / behaviour |
|---|---|---|
| POST | `api/engine/shutdown` | 202 `{"ProcessId": 1234}` and shuts down (or flags safe shutdown) after 100 ms; 500 `{"Error": "..."}` |
| GET | `api/engine/processid` | process id (int) |
| GET | `api/enginecore/getkioskengineversions` | `{ Assemblies: [{Name, Version}], Bundles: [{Name, Version}] }` for loaded `Redbox*`/`kioskengine` assemblies and all bundles |
| GET | `api/enginecore/getstoreinfo` | Store record (address, market, tax rates, service fee, `ReturnTime`, ...) from the rental data service |
| GET | `api/enginecore/getpreferencevalue?pageName=&settingName=` | Preference page value (string) |
| GET / POST | `api/enginecore/machinesettingvalue?settingPath=&settingName=[&value=]` | Read / write a registry machine setting |
| GET / POST | `api/enginecore/applicationstatevalue?settingName=[&value=]` | Read / write an `IApplicationState` value (rental) |
| GET | `api/configuration/settingvalues/changes` | Configuration changes from `IUpdateClientService` |
| GET | `api/profiledata/kioskproductids` | Product ids from the profile data |
| GET | `api/shoppingcart/loyaltytestshoppingcartitems` | Test helper |
| POST | `api/health/ping` | `IHealthServices.Ping(storeNumber)` |
| POST | `api/health/alert` | Sends a test alert (`QLM`/`TestSubAlertType`) |
| POST | `api/kioskhealth/ping`, GET `api/kioskhealth/getpingserviceinfo` | Ping service (rental) |
| GET | `api/reservation/getreservationservicestatus`, `getreservations`, `getreservabletitles` | Reservation service (rental) |
| POST | `api/reservation`, `api/reservation/cancelreservation?referenceNumber=`, `unregisterfrombrokerservice`, `registerwithbrokerservice` | Reservation service (rental) |

Clients: `UpdateClientService.API` (`api/engine/shutdown`, `api/engine/processid`), `KioskClientService.API`
(`api/profiledata/kioskproductids`, `api/shoppingcart/loyaltytestshoppingcartitems`), the Preference
Manager.

### 9.7 `WM_COPYDATA` commands

A `COPYDATASTRUCT.lpData` string `Name` or `Name:arg1|arg2` sent to the host form
(`CommandHelper.ProcessCommandMessage`):

| Command | Arguments | Action |
|---|---|---|
| `ShutdownEngine` | — | Exit or flag safe shutdown |
| `RestartBundle` | — | Re-activate current bundle if `CanRestart()` |
| `BringToFront` | — | Activate the main form |
| `ExecuteScript` | file paths | `ExecuteChunk(File.ReadAllText(path))` for each |
| `Activate` | `Name,Version` | Queue a bundle switch |
| `ActivateDefaultBundle` | — | Queue switch to default bundle |
| `ExportQueue` | file path | Export the message queue as XML |

No sender of these messages was found in the decompiled sources.

---

## 10. Interface to the rental application

The rental application is not a plug-in loaded by discovery; its assemblies are referenced directly by
`kioskengine` and started in `EngineApplication.InitializeRedboxRental()`. The interface consists of:

- **Services** placed in `ServiceLocator` by the engine for rental to use (`IKernelService`,
  `IResourceBundleService`, `IViewService`, `ICallbackService`, `IQueueService`, `IConfigurationService`,
  `IMachineSettingsStore`, `IHardwareService`, `IReservationServices`, `IUpdateManagerService`,
  `IEnvironmentNotificationService`, track-data services, ...) and by rental for the engine to use
  (`IRedboxRental.IsRentalSessionActive` for `CanSwitch`/`CanShutdown`, `IHealthServices` for crash
  alerts, `IApplicationState` for maintenance mode/stand-alone, `IConfiguration` for URLs and feature
  flags, `IStoreManager.KioskId`, `IKioskClientService`, `IAnalyticsService`, `IDataService`,
  `IReservationService`, `IEngineApplicationService`, ...).
- **Kernel functions** from `Redbox.Rental.Kernel`, registered at each activation under the label
  "Rental Kernel".
- **WPF views** registered by `OrchestrationService.RegisterViewDefinitions()` and the style dictionaries
  of `Redbox.Rental.UI`, `Redbox.ControlPanel.UI`, `Redbox.FMA.UI`; themed controls registered by
  `ThemeService` from the same three assemblies (`[ThemedControl]`).
- **The `ra-2.x.bundle`** (`Rental Application`), whose Lua startup script initialises the store manager,
  timers, sync jobs and shows the first view.
- **Queue processors** registered by rental services (`KcsUpdateMerchStatusMessageProcessorService`) and
  queued message types (`AddKioskSessionJson`).

---

## 11. Error handling, logging and recovery

- Errors are carried as `ErrorList` of `Error{Code, Description, Details, IsWarning}`. Notable codes:
  `INSTANCE` (second instance), `E005` (command line), `E006` (WPF dictionaries), `E007`
  (rental init), `B001` (bundle dir missing), `B033` (startup script), `B110` (bad bundle specifier),
  `B122` (bundle type), `B125` (bundle not found), `E003`/`E004`/`E005` (manifest checks),
  `P999` (extension load), `USH000`–`USH003` (update host), `CS0001`–`CS0007` (configuration service),
  `E246`/`BSH666` (switch blocked), `H034` (vend without barcodes), `Q997`–`Q999` (queue), `V002`
  (view background image), `C003`/`C004`/`C120`/`C671`/`C672`/`C999` (redscc).
- Start-up errors are shown in `ErrorForm`; bundle switch errors too, after which the previous bundle is
  re-queued.
- Unhandled UI-thread exceptions: notify extensions (`HandleHostCrash`), send an `ApplicationCrash`
  alert, abandon shopping sessions, show a message box. Process-level unhandled exceptions write
  `UnhandledException.txt` and exit; the next start reports it (3.1).
- Lua errors are logged with the resource name, or break into the IDE debugger when attached.
- Logging (`bin\log4net.config`, rolling 1 MB files, date-suffixed, unlimited backups):

  | Logger | File | Layout |
  |---|---|---|
  | root (INFO) | `logs\kioskengine.log` | `%d|%t|%-5p|%m%n` |
  | root, ERROR+ | `logs\error\kioskengine-error.log` | |
  | `CardReaderLog` | `logs\UI\card-reader.log` | `%d |%m%n` |
  | `UserLoginLog` | `logs\UI\user-login.log` | |
  | `ControlPanelActionLog` | `logs\UI\CP-Actvity.log` | |
  | `FieldMaintenanceActionLog` | `logs\UI\FMA-activity.log` | |
  | `TestServices` | `logs\Test\testsvcs.log` | |

  The top-level `KioskLogs/` folder of the image holds HAL logs (`hal.md`); its
  `IPCFramework/IPCClientErrors-kioskengine-<pid>.log` files are written by the IPC client inside the
  engine process and show `ConnectionRefused` (10061) errors (target service not running).
- Proxies log full JSON requests/responses with sensitive fields masked (`IObfuscationService`,
  `ObfuscateMessageData`), and execution times.

---

## 12. Source map

| Topic | Files |
|---|---|
| Entry point, crash marker | `src/KioskEngine/kioskengine/Program.cs` |
| Initialisation, idle loop, bundle switch trigger, shutdown | `src/KioskEngine/kioskengine/EngineApplication.cs` |
| Hot keys, `WM_COPYDATA` | `src/KioskEngine/kioskengine/HostForm.cs`, `CommandHelper.cs`, `Options.cs` |
| Bundles | `src/KioskEngine/Redbox.KioskEngine.Environment/ResourceBundleService.cs`; format in `src/Common/Redbox.REDS.Framework/{ResourceBundle,ManifestInfo,BundleCollection,BundleSpecifier,ResourceLookup}.cs` |
| Extensions | `src/KioskEngine/Redbox.KioskEngine.Environment/{KernelExtensionService,KernelExtension}.cs`, `src/KioskEngine/Redbox.KioskEngine.ComponentModel/IKernelExtensionHost.cs` |
| Lua host | `src/KioskEngine/Redbox.KioskEngine.Kernel/{KernelService,KernelFunctionRegistryService}.cs`, `.../ComponentModel/KernelFunctionAttribute.cs` |
| Core kernel functions | `src/KioskEngine/Redbox.KioskEngine.Kernel/*Functions.cs` |
| Shutdown/restart rules | `src/KioskEngine/Redbox.KioskEngine.Kernel/EnvironmentFunctions.cs` |
| Views | `src/KioskEngine/Redbox.KioskEngine.Environment/{ViewService,ViewFrame,WPFViewFrame,ViewStack}.cs`, `src/KioskEngine/Redbox.KioskEngine.Drawing/{RenderingService,Scene,Actor}.cs` |
| Callbacks, timers, tweens, scheduler | `src/KioskEngine/Redbox.KioskEngine.Environment/{CallbackService,TimerService,TweenService,SchedulerService}.cs` |
| Queue | `src/KioskEngine/Redbox.KioskEngine.Environment/{QueueService,QueueServiceVistaDBData,SchemaMigrator}.cs`, `Migrations.message.data.0000001.sql`, `.../ComponentModel/QueueServiceConstants.cs` |
| Data stores | `src/KioskEngine/Redbox.KioskEngine.Environment/{DataStoreService,DataStoreProvider,VistaDbDataStoreProvider,ProductLookupDataStoreProvider,DataStoreCache}.cs` |
| Settings / preferences | `src/KioskEngine/Redbox.KioskEngine.Environment/{MachineSettingsStore,UserSettingsStore,PreferenceService,PreferencePage}.cs` |
| Configuration files | `src/KioskServices/Redbox.UpdateServices.Proxy/ConfigurationService.cs`, `src/UpdateManager/Redbox.UpdateManager.Client/ConfigurationClient.cs` |
| Hardware extension | `src/KioskServices/Redbox.HardwareServices.KernelExtension/HardwareServicesHost.cs`, `src/KioskServices/Redbox.HardwareServices.Proxy/HardwareService.cs`, `HardwareAlertManagerService.cs`, `Sync*Processor.cs`, `src/KioskServices/Redbox.HardwareServices.Kernel/*.cs` |
| Update extension, IPC listener | `src/KioskServices/Redbox.UpdateServices.KernelExtension/{UpdateServicesHost,EngineCommand}.cs`, `src/KioskServices/Redbox.UpdateServices.Proxy/UpdateManagerServiceProxy.cs`, `src/KioskServices/Redbox.UpdateServices.Kernel/*.cs` |
| Broker extension | `src/KioskServices/Redbox.BrokerServices.KernelExtension/BrokerServicesHost.cs`, `src/KioskServices/Redbox.BrokerServices.Proxy/*.cs`, `src/KioskEngine/Redbox.Services.KioskBrokerServices.KioskShared/**` |
| Kiosk Data Service upload | `src/KioskServices/Redbox.KioskDataServices.Proxy/*.cs` |
| Legacy Kiosk Services (1.81) | `src/KioskServices/Redbox.KioskServices.{Kernel,KernelExtension,Proxy,Proxy.ComponentModel}`, `src/KioskServices/Redbox.KioskServices.ComponentModel/IMessageService.cs` |
| Web API | `src/KioskEngine/Redbox.KioskEngine.API/**`, `src/KioskEngine/Redbox.KioskEngine.ComponentModel/ApiServiceBaseResponse.cs` |
| IDE, preference dialogs, bundle picker | `src/KioskEngine/Redbox.KioskEngine.IDE/*.cs`, `src/KioskEngine/Redbox.KioskEngine.Environment/IdeService.cs` |
| Control Panel UI | `src/KioskEngine/Redbox.ControlPanel.UI/**` |
| FMA UI | `src/KioskEngine/Redbox.FMA.UI/**` |
| Preference Manager | `src/KioskEngine/Redbox.KioskEngine.PreferenceManager/**` |
| Bundle compiler | `src/KioskEngine/redscc/{Program,Options}.cs` |
| Card data encryption key selection | `src/KioskEngine/Redbox.KioskEngine.Environment/CardDataReader/TrackDataEncryptionService.cs` |

---

## 13. Unknowns and inferences

- **`DefaultBundle` key mismatch**: `ResourceBundleService.DefaultBundleName` reads
  `system` / category `Store` / setting `DefaultBundle`, but the image's `system.config` has it under
  category `General`. `ConfigurationClient` matches the category exactly (no fallback), so the
  hard-coded default `"Rental Application,*"` is what is actually used; both resolve to the same bundle
  here.
- **Startup scripts of libraries**: `KernelService.LoadStartupScript` overwrites the remembered startup
  script name for each script it loads, so if a required library bundle declared its own
  `startup_script` the *last* one would be executed instead of the application's. No shipped library
  has one, so the effect is not observable in the image.
- **Registry location** on 64-bit Windows (`WOW6432Node`) is inferred from the x86 build; no registry
  export is in the image. Actual registry values (store ID, environment, overrides such as the
  `127.0.0.1` HAL URL) are only visible indirectly in logs.
- **`WM_COPYDATA` sender**: no component in the decompiled sources sends these commands; probably an
  older KioskShell/update manager or support tool.
- **IPC commands without handlers** (`engine load-bundle`, `start`, `execute-script`, `export-queue`,
  `export-sessions`, `show-offline-screen`, `bring-to-front`) are sent by the update-manager client but
  not implemented in 1.88; their behaviour (probably "unknown command" error) depends on IPC.Framework.
  `engine reset-bundle` is declared with a non-standard signature; whether it dispatches is unverified.
- **Lua version**: the log prints "Lua compiler version: Not Installed, architecture: Not Installed"
  for the `LuaVirtualMachine.Compiler` info; the Lua runtime itself works. See `common-libraries.md`.
- **`IPCServiceHost.Create` numeric arguments** (`3, 3, 8192, 64, 5`) are passed through to
  IPC.Framework; their meaning (thread counts, buffer size, backlog, ...) is not documented here.
- **Store certificate path** for card encryption (`data\certs\<CertificateName>.cer`) is inferred from
  file names; `GetStoreCertificate` was not read in full.
- **Broker server side**: the reservation broker server and its registration semantics are known only
  from the client interfaces; whether any production broker still accepted `gtcp` connections is unknown.
- **Kiosk Data Service response schema** beyond the `success` key is not visible (the logged requests in
  the image failed with exceptions).
- The image is not pristine: re-pointed service URLs in `system.config` and `.mod`/`.orig` bundle and
  binary files indicate local modification; descriptions of "real values" refer to this image.
