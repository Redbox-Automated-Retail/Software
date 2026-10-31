# Kiosk Engine releases (2014–2024)

Every release of the Kiosk Engine package (`kop-v2-engine`) that could be rebuilt from the archived update-store
snapshots ([snapshots.md](snapshots.md), method in [version-history.md](version-history.md)). Each is decompiled
with the same ILSpy settings as `src/` and committed, dated, to the local branch **`history/engine`**: one commit
per release, oldest first, so `git log -p history/engine` shows what every release changed. `src/` on `main` is
the last one, Calvin 1.88.0.80.

```
git log --format="%ad %s" --date=short history/engine     # the releases
git diff <older> <newer> -- src/Rental                       # what changed between two of them
```

Each commit has a `RELEASE.md` listing every engine file of that release with its version and SHA-1;
`tools/history/export_release.py` copies those original assemblies out of the rebuilt store.

## Reading the list

- **Release names** are the store's revision labels (`kop-v2-engine-<name>`), written readably: `venomSR2`
  becomes `Venom SR2 (1.30.0.26)`, `KISS1.1` becomes `KISS 1.1 (1.44.1.3)`; the commit body keeps the original
  label. Unlabelled revisions that brought new code are releases too: `1.7.0.9 (no code name)`.
- **Tags**: each named release has a local annotated tag `engine/<Code-Name>` (`engine/Venom-SR2`) on its last
  commit; a version that never got a code name is tagged `engine/<version>`. The other branches follow the same
  pattern (`hal/Caddyshack`, `update-client/1.2.29.0`).
- **Dates** are when the kiosk downloaded the release, taken from the snapshot zips, so they are "on the kiosk
  by" dates. Where a release survives only in a later snapshot, it gets its successor's date.
- **Order** is the store's revision order, and the `kioskengine` version never goes down along it. Unlabelled
  releases with the same version as their predecessor changed other assemblies or scripts.
- Only revisions whose files have content in some snapshot can be rebuilt. The store shipped most
  intermediate builds squashed into the next labelled release, so between two code names there are often
  revisions that exist only as placeholders (see [version-history.md](version-history.md)).
- **Stale** files: when a release lists a file version that no snapshot holds, the last recoverable version is
  used and `RELEASE.md` marks it `stale`.
- **Removed files**: the store never lists a file as removed; packages delete obsolete files from their Lua
  activation scripts (`kernel.filedelete`). A file leaves the branch once a script that deletes it is in
  effect, unless the store lists it again later. Files that went some other way stay: the 2024 image has no
  `Kiosk Engine\kext` folder at all, but the store still lists the 2014–2016 `kext` files, so the
  `Redbox.DVD.*` shells and two old `kext` copies of `Redbox.HAL.Client` / `Redbox.UpdateManager.Client`
  remain in every later release.

## Code names

The labels run through the alphabet several times:

| Years | Theme | Engine releases |
|---|---|---|
| 2014–2015 | films | Braveheart, Dracula, The-Martian (HAL: Caddyshack, X-Men) |
| 2016–2018 | comic-book characters, A→Z | Batman, HawkeyeSR, Penguin, SupermanSR, venomSR2, YodaSR3, ZeldaSR3 (HAL: Hawkeye, JubileeSR, Legion, Superman; update manager: Catwoman) |
| 2019–2020 | bands, K→Y | KISS1.1, OasisSR3, PrimusSR4, SublimeSR, VanHalenSR2, YesSR3 (HAL: QueenSR6, U2) |
| 2022–2023 | cars, L→Z | LincolnSR3, McLaren, Nova, Omni, Pinto, Quattro, Ranchero, Studebaker, Tesla, Ultima, Vega, Willys, Xterra, Yugo, Z |
| 2023–2024 | cartoon characters, A→C | Aquaman2023, Bullwinkle, Calvin |

`SR`, `SR2`, … are service releases of the same name. The Kiosk Client Service and Update Client Service mostly
follow the engine's names; the Update Client Service also has its own car-part names (Exhaust, Fuse, Flywheel,
Gearbox) and Donatello.

## Eras

### 1. Lua engine with DVD services (2014; 1.3.x)

Braveheart (1.3.7.10) and Dracula (1.3.9.4). A WinForms host (`kioskengine.exe`) running the Lua rental
application from bundles, with kernel extensions `Redbox.DVD.{Broker,Hardware,Kiosk,Update,Ticket}Services`
that each bundle their own kernel, extension and proxy code, `redscc` for the REDS command channel, and
`Redbox.KioskEngine.ComponentModel`. Dracula is a small fix release. `Redbox.DVD.TicketServices` (event
tickets, see [unreleased-features.md](unreleased-features.md)) is in both; an activation script dated June
2014 deletes it, so no later release has it.

### 2. .NET rental libraries (2015–2016; 1.7–1.13)

The-Martian (1.9.0.9) and Batman (1.13.0.19), with unlabelled 1.7.0.9 builds before them. The rental logic
starts moving from Lua into .NET: `Redbox.Rental.{Data,Kernel,Model,Services}` appear in 1.7.0.9. Batman adds
`Controllers` and `Views` to `Redbox.Rental.Services` (the first .NET screens, still WinForms-hosted).

### 3. WPF rental UI (2016–2017; 1.18–1.25)

HawkeyeSR (1.18.0.18) brings **`Redbox.Rental.UI`**, the WPF user interface (about 1,400 files: views, controls,
styles), replacing `Redbox.Rental.Services/Views`. Penguin (1.25.0.4) adds marketing and configuration services and
the KioskServices model in the component model.

### 4. Engine split (2017–2018; 1.25–1.34)

An unlabelled 1.25.0.4 revision ships the shared libraries (`Redbox.Core`, `Compression`, `IPC.Framework`,
`Macros`, `USB`, ...) as separate assemblies instead of merged copies. **SupermanSR** (1.27.0.85) then breaks
the monolithic engine apart: `Redbox.KioskEngine.{Kernel,Environment,IDE,Drawing}` and
`Redbox.{Broker,Hardware,Kiosk,Update}Services.{Kernel,KernelExtension,Proxy,Proxy.ComponentModel}` replace the
`Redbox.DVD.*` services (the store keeps listing their last builds; see "Removed files" above). venomSR2 (1.30.0.26) adds the control panel
(`Redbox.ControlPanel.UI`, `Redbox.Controls`) and loyalty, personalization, promotions and product pricing in
the rental model. YodaSR3 (1.32.1.79) and ZeldaSR3 (1.34.0.17) grow the shopping cart, promotions and health
reporting.

### 5. Kiosk data, fraud and EMV (2019–2020; 1.44–1.57)

KISS1.1 (1.44.1.3) adds `Redbox.FraudServices.Proxy` (disc-read tracking), `Redbox.KioskDataServices.Proxy`
(kiosk telemetry), reservations, analytics and the first `DeviceService.Client` / `.ComponentModel` (the EMV
card-reader service). OasisSR3 (1.48.0.85) reads card data through the Device Service (track data, card data
reader). PrimusSR4 (1.49.0.110) adds the e-mail opt-in campaign. SublimeSR (1.52.1.2) starts talking to the new
**Kiosk Client Service** (products, ads, transactions) and moves hardware services into the component model.
VanHalenSR2 (1.55.0.46) adds `Rental.Services/App`; YesSR3 (1.57.3.9) is the last release before an 18-month
gap in the snapshots.

### 6. Local services and monthly car releases (2022–2023; 1.71–1.85)

LincolnSR3 (1.71.0.21) is the next snapshot and a big step: `Redbox.KioskEngine.API` (a local HTTP API used by
the Kiosk Client Service), `Redbox.KioskEngine.PreferenceManager` (a separate settings tool),
`Redbox.FMA.UI` (field-maintenance planogram screens), the **Update Client Service** integration, **Redbox+**
(RedboxPlus), **Starz**, reservations and planograms. From McLaren (1.72) to Z (1.85) a release ships roughly
every month under a car name, each with `SR` follow-ups: mostly UI, configuration and Kiosk Client Service
changes. Vega/VegaSR2 (1.81) trims the KioskServices proxy API (the 1.81.0.27 libraries are what
`src/KioskServices/Redbox.KioskServices.{Kernel,KernelExtension,Proxy}` build against). Willys (1.82.0.20) adds
authentication, health and store services; Yugo (1.84) dual-kiosk support; Z (1.85) removes the e-mail opt-in
service.

### 7. Wind-down (2023–2024; 1.86–1.88)

Aquaman2023 (1.86.0.57) removes Starz and guts the fraud services (about 10,000 lines deleted). Bullwinkle
(1.87.0.77) and Calvin (1.88.0.80, May 2024) are the last releases; Calvin is the engine in the 2024 kiosk
image and in `src/`.

## Every release

Generated from `history/engine`: number of first-party assemblies, lines added/removed against the previous
release (decompiled source, so renames by the decompiler count too), assemblies added or removed, and feature
folders (namespaces) added or removed.

| # | Date | Release | kioskengine | Assemblies | Lines +/− | What changed (assemblies, feature folders) |
|---|---|---|---|---|---|---|
| 0 | 2014-02-03 | *no code name* | – | 7 | +0 / −0 | new: `Redbox.Core`, `Redbox.HAL.Client`, `Redbox.Lua`, `Redbox.REDS.Framework`, `Redbox.Services.KioskBrokerServices.KioskShared`, `Redbox.UpdateManager.Client`, `redscc` |
| 1 | 2014-02-03 | **Braveheart** | 1.3.7.10 | 14 | +197208 / −4 | new: `Redbox.DVD.BrokerServices`, `Redbox.DVD.HardwareServices`, `Redbox.DVD.KioskServices`, `Redbox.DVD.TicketServices`, `Redbox.DVD.UpdateServices`, `Redbox.KioskEngine.ComponentModel`, `kioskengine` |
| 2 | 2014-02-28 | **Dracula** | 1.3.9.4 | 14 | +93 / −66 |  |
| 3 | 2015-12-18 | *no code name* | 1.3.9.4 | 14 | +2657 / −1797 | + `Redbox.HAL.Client/Configuration` |
| 4 | 2015-12-18 | *no code name* | 1.3.9.4 | 14 | +12 / −9 |  |
| 5 | 2015-12-18 | *no code name* | 1.3.9.4 | 14 | +14 / −10 |  |
| 6 | 2015-12-18 | *no code name* | 1.7.0.9 | 14 | +1684 / −1898 |  |
| 7 | 2015-12-18 | *no code name* | 1.7.0.9 | 17 | +15506 / −10569 | new: `Redbox.Rental.Data`, `Redbox.Rental.Kernel`, `Redbox.Rental.Model`, `Redbox.Rental.Services`; removed: `Redbox.DVD.TicketServices` |
| 8 | 2015-12-18 | **The Martian** | 1.9.0.9 | 17 | +3556 / −2710 |  |
| 9 | 2016-04-01 | *no code name* | 1.9.0.9 | 17 | +3010 / −1143 | + `Redbox.HAL.Client/Component` |
| 10 | 2016-04-01 | **Batman** | 1.13.0.19 | 17 | +2305 / −251 | + `Redbox.Rental.Services/Controllers`, `Redbox.Rental.Services/Views` |
| 11 | 2016-10-28 | *no code name* | 1.13.0.19 | 17 | +228 / −274 |  |
| 12 | 2016-10-28 | *no code name* | 1.13.0.19 | 17 | +271 / −225 |  |
| 13 | 2016-10-28 | *no code name* | 1.13.0.19 | 17 | +225 / −271 |  |
| 14 | 2016-10-28 | *no code name* | 1.13.0.19 | 17 | +14 / −10 |  |
| 15 | 2016-10-28 | *no code name* | 1.13.0.19 | 17 | +2026 / −1186 |  |
| 16 | 2016-10-28 | **Hawkeye SR** | 1.18.0.18 | 18 | +71664 / −1303 | new: `Redbox.Rental.UI`; − `Redbox.Rental.Services/Views` |
| 17 | 2017-07-26 | *no code name* | 1.18.0.18 | 18 | +45 / −17 |  |
| 18 | 2017-07-26 | *no code name* | 1.18.0.18 | 18 | +16 / −16 |  |
| 19 | 2017-07-26 | **Penguin** | 1.25.0.4 | 18 | +11543 / −5934 | + `Redbox.KioskEngine.ComponentModel/KioskServices`, `Redbox.Rental.Model/Marketing`, `Redbox.Rental.Services/Configuration`, `Redbox.Rental.Services/Marketing`, `Redbox.Rental.UI/Models` |
| 20 | 2017-10-13 | *no code name* | 1.25.0.4 | 18 | +13 / −13 |  |
| 21 | 2017-10-13 | *no code name* | 1.25.0.4 | 33 | +804 / −25278 | new: `Redbox.Command.Tokenizer`, `Redbox.Compression`, `Redbox.GetOpts`, `Redbox.IPC.Framework`, `Redbox.JSONPrettyPrinter`, `Redbox.KioskServices.ComponentModel`, `Redbox.Log.Framework`, `Redbox.Macros`, `Redbox.Macros.Functions`, `Redbox.ProductLookupCatalog`, `Redbox.Tokenizer.Framework`, `Redbox.USB`, `Redbox.UpdateManager.ComponentModel`, `Redbox.UpdateService.Client`, `Redbox.UpdateService.Model`; + `Redbox.DVD.KioskServices/Kernel`, `Redbox.DVD.KioskServices/KernelExtension`, `Redbox.DVD.KioskServices/Proxy`, `Redbox.DVD.UpdateServices/ComponentModel`, `Redbox.DVD.UpdateServices/Redbox.IPC.Framework.Messaging`, `redscc/Redbox.GetOpts`; − `Redbox.DVD.KioskServices/ComponentModel`, `Redbox.DVD.KioskServices/Redbox.JSONPrettyPrinter`, `Redbox.DVD.KioskServices/Redbox.KioskServices.ComponentModel`, `Redbox.DVD.KioskServices/Redbox.KioskServices.Kernel`, `Redbox.DVD.KioskServices/Redbox.KioskServices.KernelExtension`, `Redbox.DVD.UpdateServices/Messaging`, `Redbox.DVD.UpdateServices/NamedPipes`, `Redbox.DVD.UpdateServices/Redbox.Command.Tokenizer` … |
| 22 | 2017-10-13 | *no code name* | 1.25.0.4 | 33 | +10 / −10 |  |
| 23 | 2017-10-13 | *no code name* | 1.25.0.4 | 33 | +11 / −11 |  |
| 24 | 2017-10-13 | *no code name* | 1.25.0.4 | 33 | +10 / −13 |  |
| 25 | 2017-10-13 | **Superman SR** | 1.27.0.85 | 53 | +24107 / −150457 | new: `Redbox.BrokerServices.Kernel`, `Redbox.BrokerServices.KernelExtension`, `Redbox.BrokerServices.Proxy`, `Redbox.BrokerServices.Proxy.ComponentModel`, `Redbox.HardwareServices.Kernel`, `Redbox.HardwareServices.KernelExtension`, `Redbox.HardwareServices.Proxy`, `Redbox.HardwareServices.Proxy.ComponentModel`, `Redbox.KioskEngine.Drawing`, `Redbox.KioskEngine.Environment`, `Redbox.KioskEngine.IDE`, `Redbox.KioskEngine.Kernel`, `Redbox.KioskServices.Kernel`, `Redbox.KioskServices.KernelExtension`, `Redbox.KioskServices.Proxy`, `Redbox.KioskServices.Proxy.ComponentModel`, `Redbox.UpdateServices.Kernel`, `Redbox.UpdateServices.KernelExtension`, `Redbox.UpdateServices.Proxy`, `Redbox.UpdateServices.Proxy.ComponentModel`; + `Redbox.KioskEngine.ComponentModel/TextToSpeech`, `Redbox.Rental.Model/Browse`, `Redbox.Rental.Model/KioskEvents`, `Redbox.Rental.Model/KioskHealth`, `Redbox.Rental.Model/Pricing`, `Redbox.Rental.Model/ShoppingCart`, `Redbox.Rental.Services/KioskEvents`, `Redbox.Rental.Services/KioskHealth` …; − `Redbox.DVD.BrokerServices/Redbox.BrokerServices.Kernel`, `Redbox.DVD.BrokerServices/Redbox.BrokerServices.KernelExtension`, `Redbox.DVD.BrokerServices/Redbox.BrokerServices.Proxy`, `Redbox.DVD.BrokerServices/Redbox.BrokerServices.Proxy.ComponentModel`, `Redbox.DVD.HardwareServices/Kernel`, `Redbox.DVD.HardwareServices/KernelExtension`, `Redbox.DVD.HardwareServices/Proxy`, `Redbox.DVD.KioskServices/Kernel` … |
| 26 | 2018-02-15 | *no code name* | 1.27.0.85 | 53 | +42 / −39 |  |
| 27 | 2018-02-15 | **Venom SR2** | 1.30.0.26 | 55 | +34055 / −2925 | new: `Redbox.ControlPanel.UI`, `Redbox.Controls`; + `Redbox.KioskServices.Proxy/Request`, `Redbox.KioskServices.Proxy/Requests`, `Redbox.Rental.Model/Loyalty`, `Redbox.Rental.Model/Personalization`, `Redbox.Rental.Model/Promotion`, `Redbox.Rental.Services/Loyalty`, `Redbox.Rental.Services/Personalization`, `Redbox.Rental.Services/ProductPricing.Model` … |
| 28 | 2018-04-27 | *no code name* | 1.30.0.26 | 55 | +40 / −36 |  |
| 29 | 2018-04-27 | *no code name* | 1.30.0.26 | 55 | +83 / −34 |  |
| 30 | 2018-04-27 | *no code name* | 1.30.0.26 | 55 | +34 / −83 |  |
| 31 | 2018-04-27 | **Yoda SR3** | 1.32.1.79 | 55 | +11954 / −4307 |  |
| 32 | 2018-05-21 | *no code name* | 1.32.1.79 | 55 | +86 / −37 |  |
| 33 | 2018-05-21 | **Zelda SR3** | 1.34.0.17 | 55 | +3405 / −1680 |  |
| 34 | 2019-04-05 | *no code name* | 1.34.0.17 | 55 | +8203 / −5046 | − `Redbox.HAL.Client/Configuration` |
| 35 | 2019-04-05 | *no code name* | 1.34.0.17 | 56 | +84 / −31 | new: `Redbox.KioskDataServices.Proxy.ComponentModel` |
| 36 | 2019-04-05 | *no code name* | 1.34.0.17 | 57 | +182 / −1 | new: `Redbox.KioskDataServices.Proxy` |
| 37 | 2019-04-05 | *no code name* | 1.34.0.17 | 57 | +69 / −33 |  |
| 38 | 2019-04-05 | *no code name* | 1.34.0.17 | 57 | +42 / −49 |  |
| 39 | 2019-04-05 | *no code name* | 1.34.0.17 | 59 | +2123 / −30 | new: `DeviceService.Client`, `DeviceService.ComponentModel` |
| 40 | 2019-04-05 | **KISS 1.1** | 1.44.1.3 | 61 | +35677 / −7354 | new: `Redbox.FraudServices.Proxy`, `Redbox.FraudServices.Proxy.ComponentModel`; + `Redbox.KioskEngine.ComponentModel/FraudServices`, `Redbox.KioskEngine.Environment/FraudServices`, `Redbox.Rental.Model/Analytics`, `Redbox.Rental.Model/Local`, `Redbox.Rental.Model/Reservation`, `Redbox.Rental.Services/Analytics` |
| 41 | 2019-09-16 | *no code name* | 1.44.1.3 | 61 | +65 / −48 |  |
| 42 | 2019-09-16 | *no code name* | 1.44.1.3 | 61 | +63 / −50 |  |
| 43 | 2019-09-16 | *no code name* | 1.44.1.3 | 61 | +1932 / −110 | + `DeviceService.ComponentModel/Analytics`, `DeviceService.ComponentModel/Bluefin`, `DeviceService.ComponentModel/FileUpdate`, `DeviceService.ComponentModel/KDS` |
| 44 | 2019-09-16 | **Oasis SR3** | 1.48.0.85 | 61 | +17613 / −4910 | + `Redbox.KioskEngine.ComponentModel/TrackData`, `Redbox.KioskEngine.Environment/CardDataReader`, `Redbox.KioskEngine.Environment/KioskServices` |
| 45 | 2019-11-05 | *no code name* | 1.48.0.85 | 61 | +161 / −79 |  |
| 46 | 2019-11-05 | **Primus SR4** | 1.49.0.110 | 61 | +7018 / −3415 | + `Redbox.Rental.Model/EmailOptInCampaign`, `Redbox.Rental.Services/EmailOptinCampaign` |
| 47 | 2020-02-26 | *no code name* | 1.49.0.110 | 61 | +59 / −46 |  |
| 48 | 2020-02-26 | *no code name* | 1.49.0.110 | 61 | +43 / −56 |  |
| 49 | 2020-02-26 | *no code name* | 1.49.0.110 | 61 | +56 / −43 |  |
| 50 | 2020-02-26 | **Sublime SR** | 1.52.1.2 | 61 | +12576 / −7446 | + `Redbox.KioskEngine.ComponentModel/HardwareServices`, `Redbox.Rental.Model/Ads`, `Redbox.Rental.Model/KioskClientService`, `Redbox.Rental.Model/Transaction`, `Redbox.Rental.Services/Ads`, `Redbox.Rental.Services/KioskClientService`; − `Redbox.KioskEngine.Kernel/Redbox.Rental.Kernel` |
| 51 | 2020-05-21 | *no code name* | 1.52.1.2 | 61 | +83 / −58 |  |
| 52 | 2020-05-21 | *no code name* | 1.52.1.2 | 61 | +38 / −38 |  |
| 53 | 2020-05-21 | *no code name* | 1.52.1.2 | 61 | +38 / −38 |  |
| 54 | 2020-05-21 | **Van Halen SR2** | 1.55.0.46 | 61 | +4827 / −765 | + `Redbox.Rental.Services/App` |
| 55 | 2020-08-04 | *no code name* | 1.55.0.46 | 61 | +48 / −45 |  |
| 56 | 2020-08-04 | **Yes SR3** | 1.57.3.9 | 61 | +1844 / −709 |  |
| 57 | 2022-02-18 | *no code name* | 1.57.3.9 | 61 | +109 / −47 |  |
| 58 | 2022-02-18 | **Lincoln SR3** | 1.71.0.21 | 64 | +42027 / −12188 | new: `Redbox.FMA.UI`, `Redbox.KioskEngine.API`, `Redbox.KioskEngine.PreferenceManager`; + `Redbox.KioskEngine.ComponentModel/ApiService`, `Redbox.KioskServices.Proxy.ComponentModel/Loyalty`, `Redbox.Rental.Model/EngineApplication`, `Redbox.Rental.Model/Planogram`, `Redbox.Rental.Model/UpdateClientService`, `Redbox.Rental.Services/RedboxPlus`, `Redbox.Rental.Services/Reservations`, `Redbox.Rental.Services/Starz` …; − `Redbox.KioskServices.Proxy/Requests` |
| 59 | 2022-03-28 | **McLaren** | 1.72.0.5 | 64 | +220 / −194 |  |
| 60 | 2022-03-28 | **Nova** | 1.73.0.11 | 64 | +679 / −425 |  |
| 61 | 2022-04-10 | **Nova SR** | 1.73.0.19 | 64 | +178 / −142 |  |
| 62 | 2022-04-26 | **Nova SR2** | 1.73.0.27 | 64 | +249 / −192 |  |
| 63 | 2022-05-09 | **Nova SR3** | 1.73.0.28 | 64 | +116 / −116 |  |
| 64 | 2022-05-23 | **Nova SR4** | 1.73.0.29 | 64 | +116 / −111 |  |
| 65 | 2022-05-30 | **Omni** | 1.74.0.24 | 64 | +764 / −401 |  |
| 66 | 2022-06-13 | **Omni SR** | 1.74.0.27 | 64 | +116 / −125 |  |
| 67 | 2022-06-26 | **Pinto** | 1.75.0.14 | 64 | +925 / −804 |  |
| 68 | 2022-07-13 | **Quattro** | 1.76.0.2 | 64 | +222 / −138 |  |
| 69 | 2022-07-25 | **Quattro SR** | 1.76.0.5 | 64 | +134 / −119 |  |
| 70 | 2022-09-27 | **Ranchero** | 1.77.0.12 | 64 | +746 / −625 |  |
| 71 | 2022-10-18 | **Ranchero SR** | 1.77.0.13 | 64 | +167 / −148 |  |
| 72 | 2022-11-15 | **Studebaker** | 1.78.0.6 | 64 | +1376 / −812 |  |
| 73 | 2023-01-29 | **Tesla** | 1.79.0.21 | 64 | +793 / −606 |  |
| 74 | 2023-03-05 | **Ultima** | 1.80.0.12 | 64 | +862 / −1284 |  |
| 75 | 2023-03-26 | **Vega** | 1.81.0.22 | 64 | +2384 / −7261 | + `Redbox.KioskEngine.PreferenceManager/TransactionService` |
| 76 | 2023-03-29 | **Vega SR** | 1.81.0.23 | 64 | +114 / −110 |  |
| 77 | 2023-04-18 | **Vega SR2** | 1.81.0.27 | 64 | +114 / −111 |  |
| 78 | 2023-04-25 | **Willys** | 1.82.0.20 | 64 | +7718 / −2799 | + `Redbox.KioskEngine.PreferenceManager/KioskDataService`, `Redbox.Rental.Model/Authentication`, `Redbox.Rental.Model/Health`, `Redbox.Rental.Services/Authentication`, `Redbox.Rental.Services/Health`, `Redbox.Rental.Services/StoreServices` |
| 79 | 2023-06-06 | **Xterra** | 1.83.0.17 | 64 | +1045 / −722 |  |
| 80 | 2023-08-01 | **Yugo** | 1.84.0.25 | 64 | +2039 / −379 |  |
| 81 | 2023-08-20 | **Yugo SR** | 1.84.1.2 | 64 | +2269 / −2009 |  |
| 82 | 2023-08-20 | **Yugo SR2** | 1.84.1.14 | 64 | +223 / −102 |  |
| 83 | 2023-10-01 | **Z** | 1.85.0.27 | 64 | +662 / −2434 | − `Redbox.Rental.Model/Marketing`, `Redbox.Rental.Services/EmailOptinCampaign` |
| 84 | 2023-11-14 | **Aquaman 2023** | 1.86.0.57 | 64 | +1771 / −9118 | − `Redbox.Rental.Services/Starz` |
| 85 | 2023-11-26 | **Aquaman 2023 SR** | 1.86.0.76 | 64 | +101 / −100 |  |
| 86 | 2023-11-29 | **Bullwinkle** | 1.87.0.77 | 64 | +1289 / −551 |  |
| 87 | 2024-05-07 | **Calvin** | 1.88.0.80 | 64 | +108 / −127 |  |
