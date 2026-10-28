# Version history of the kiosk software (2014–2024)

This is a timeline of every release that can be rebuilt from the archived update-store snapshots, with how
much each one changed. It is used to choose which old versions are worth decompiling. The current source in
`src/` is the **newest** release of every package, so the newest row of each table is already in the
repository.

## Sources and method

- 24 archived store snapshots `U:\data\data-*.zip`, dated 2014-02-03 to 2024-05-07 (each zip is a kiosk's
  `.store/.repository` folder), plus the kiosk image's own `REDS/Update Manager/.store/.repository`
  (2024-07). The format is described in [update-store.md](update-store.md). Every zip's SHA-256 matches the hash
  posted with it.
- `tools/history/rebuild_store.py` merges the revision logs of all snapshots into one ordered history per
  package and rebuilds every file version it can: seeds are taken as they are, VCDIFF deltas are applied
  with the kiosk's own `xdelta3.exe` 3.0u, and **every** result's SHA-1 is checked against the change's
  `VersionHash`. Output (outside git): a content-addressed blob store and one index per package.
- `tools/history/history.py` compares every rebuilt version of a file with the previous one: for .NET
  assemblies the version and the sets of types and members (from metadata, by `tools/history/ApiSurface`;
  compiler-generated names are left out), for `.bundle` zips the entries; it writes a timeline and a
  one-line summary per release.
- `dvd-data` (the title database) was not rebuilt, as agreed.

### What was verified

- All 11 code/content packages rebuilt: **3,168 changes rebuilt and SHA-1-verified, 0 failures**. The other
  11,318 changes are *placeholders* (`Composite: true`): the revision lists the file but no snapshot holds its
  content, because the server shipped intermediate revisions squashed into the next labelled release. So the
  versions that can be recovered are, in practice, the **labelled releases** (code names such as `Dracula`,
  `Hawkeye`, `KISS`, `Calvin`) and some unlabelled revisions in between.
- 2,674 distinct file versions (2.3 GB) were recovered; 2,025 of them are .NET assemblies.
- Dates are the earliest modification time of the release's content files in the snapshot zips (the time
  the kiosk downloaded them), so they are "available on the kiosk by" dates, not build dates.
- Sizes are metadata counts, not decompiled diffs. They show *where* code changed and how much; the
  "feature" column is read from added/removed namespaces and type names.

### Releases per package

Releases that brought code (first-party assemblies or Lua bundles) and could be rebuilt, of which how many
carry a code name:

| Package | Releases | With code name |
|---|---|---|
| kop-v2-engine | 87 | 44 |
| kop-rental-bundles | 44 | 36 |
| kioskclient | 21 | 19 |
| kop-hal-install | 15 | 12 |
| kop-ms-hal-tester | 12 | 12 |
| kop-tools | 11 | 10 |
| updateclient | 10 | 8 |
| device-service | 9 | 7 |
| kop-update-manager | 6 | 2 |

The tables below summarise them; `summary.tsv` in the work directory lists every one.

## Classification

**Substantial** = adds or removes a feature, subsystem, protocol or assembly, or restructures the
architecture; also any version that fills a gap in this repository (an excluded project needs it) or holds
undeployed code. **Minor** = version bumps, bug fixes, small API changes. Each table lists the release label,
date, versions, size of change (first-party assemblies only: types/members added/removed) and the reason.

### Kiosk engine and rental application (`kop-v2-engine`)

The engine package holds `kioskengine.exe`, the WPF rental UI and all their libraries.

| # | Release | Date | Engine | Size | Class | Reason |
|---|---|---|---|---|---|---|
| E0 | Braveheart | 2014-02-03 | 1.3.7.10 | first (kioskengine 1,600 types) | **substantial** | Oldest engine found; an earlier build of the same code as Dracula, including `Redbox.DVD.TicketServices` 1.3.7.10. |
| E1 | Dracula | 2014-02-28 | 1.3.9.4 | +3 members | **substantial** | First engine in the original 13 snapshots: WinForms/Lua engine with `Redbox.DVD.{Broker,Hardware,Kiosk,Update,Ticket}Services` and `redscc`. Contains **`Redbox.DVD.TicketServices`** (event tickets, never in a later release). |
| – | (unlabelled) | 2015 | 1.4–1.8 | HAL.Client 1.4, KioskShared 1.4 | minor | Library refreshes. |
| E2 | The-Martian | 2015-12 | 1.9.0.9 | 4 new assemblies | **substantial** | `Redbox.Rental.{Services,Kernel,Data,Model}` split out of the engine: the start of the .NET rental application. |
| – | Batman | 2016-04 | 1.13.0.19 | +15 types in Rental.Services | minor | HAL.Client 1.7. |
| – | (unlabelled) | 2016 | 1.14.0.2 | +9/−6 members | minor | Last version of the `Redbox.DVD.*` services. |
| E3 | HawkeyeSR | 2016-10 | 1.18.0.18 | +1,690/−447 types | **substantial** | New WPF **`Redbox.Rental.UI`**; GenuineChannels remoting and LZMA merged into the engine; Rental controllers and views. |
| E4 | Penguin | 2017-07 | 1.25.0.4 | +415/−373 types, +7.7k/−6.5k members | borderline | Marketing and configuration services, KioskServices model; large churn inside `kioskengine`. |
| E5a | SupermanSR | 2017-10 | 1.27.0.85 | `kioskengine` −2,869 types | **substantial** | The architecture split below first ships here (found in the 2017-10 snapshot). |
| – | venomSR2 | 2018-02 | 1.30.0.26 | +87 types in Rental.UI, +110 in Rental.Services | borderline | Rental UI/services growth, KioskServices proxy. |
| E5 | YodaSR3 | 2018-04 | 1.32.1.79 | 23 new assemblies (relative to Penguin), `kioskengine` −2,869 types | **substantial** | Architecture split: the monolithic engine becomes `KioskEngine.{Kernel,Environment,IDE,Drawing}`, `{Hardware,Broker,Kiosk,Update}Services.{Kernel,KernelExtension,Proxy}`, `ControlPanel.UI`, `Controls`; `Redbox.DVD.*` removed. Shopping cart, pricing, promotions, loyalty, personalization, kiosk health/events, text-to-speech. |
| – | (unlabelled) | 2018-06 | 1.34.0.17 | +18/−4 types | minor | |
| E6 | KISS1.1 | 2019-04 | 1.44.1.3 | +269/−29 types, +6.1k members | **substantial** | New **FraudServices** proxy, **KioskDataServices** proxy, reservations, analytics, first `DeviceService.Client` (EMV card readers). |
| E7 | OasisSR3 | 2019-09 | 1.48.0.85 | +155/−28 types, +3.7k members | **substantial** | Card data reader / track data through the new Device Service (EMV rollout), analytics model growth. |
| – | PrimusSR4, SublimeSR | 2019-11, 2020-02 | 1.49.0.110, 1.52.1.2 | +18 to +40 types per assembly | borderline | Rental UI/services growth leading to E8. |
| E8 | VanHalenSR2 | 2020-05 | 1.55.0.46 | +291/−47 types, +5.1k members | **substantial** | Integration with the new **KioskClientService** (session, products, inventory, ads), email opt-in campaign, transactions. |
| – | YesSR3 | 2020-08 | 1.57.3.9 | +10 types | minor | |
| E9 | LincolnSR3 | 2022-02 | 1.71.0.21 | +523/−107 types, +10.2k members, 3 new assemblies | **substantial** | Integration with **UpdateClientService**; **Starz**, **Redbox+ (RedboxPlus)**, reservations, planogram; new `Redbox.KioskEngine.API` (local HTTP API), `Redbox.KioskEngine.PreferenceManager`, `Redbox.FMA.UI`. |
| – | McLaren … Ultima | 2022-03 – 2023-03 | 1.72 – 1.80 | each < 150 members | minor | Bug fixes; welcome-screen assets (1.75) and campaign (1.79) models. |
| E10 | Vega / VegaSR2 | 2023-03/04 | 1.81.0.22 / 1.81.0.27 | +45/−58 types | **substantial (gap)** | KioskServices proxy API trimmed. **1.81.0.27 is the engine the excluded `Redbox.KioskServices.Kernel/.KernelExtension/.Proxy` were built against** (see [decompilation.md](decompilation.md) "Known gaps"). |
| E11 | Willys | 2023-04 | 1.82.0.20 | +121/−29 types, +1.5k members | borderline | Store services, authentication, health, shopping-cart and fraud endpoints in the engine API; PreferenceManager +35 types; old BrokerServices kernel extension removed. |
| – | Xterra, Yugo, YugoSR/SR2, Z | 2023-06 – 2023-10 | 1.83 – 1.85 | each < 500 members | minor | Configuration, kiosk/“dual” client models (1.84), profile data API (1.85), email opt-in removed (1.85). |
| E12 | Aquaman2023 | 2023-11 | 1.86.0.57 | +13/−78 types, −2.4k members | **substantial (removal)** | **Fraud services and Starz removed** (`Redbox.FraudServices.Proxy` emptied, fraud API controllers and Starz services gone). |
| – | AquamanSR2023, Bullwinkle | 2023-11 | 1.86.0.76, 1.87.0.77 | +361/−220 members | minor | |
| – | Calvin | 2024-05 | 1.88.0.80 | IL −704 bytes | current | = `src/`. |

### HAL (`kop-hal-install`), MS HAL Tester (`kop-ms-hal-tester`), HAL Utilities (`kop-tools`)

The three packages are released together with the same HAL version.

| # | Release | Date | HAL | Size | Class | Reason |
|---|---|---|---|---|---|---|
| H1 | Caddyshack | 2014-02 | 1.3.0.84 | first (16 assemblies) | **substantial** | Oldest HAL, MSHALTester, HalUtilities; legacy DirectShow/ClearImage camera plugins. |
| H2 | (unlabelled) | 2015-12 | camera plugins 1.3.0.119 | +25/−9 members | **gap** | `Redbox.HAL.Camera.Framework`, `.DirectShow.Plugin`, `.ICameraPlugin.Vidcap`, `.ClearimageV1` **1.3.0.119** — the versions `src/HAL/Legacy` was built against (the rest of HAL stays 1.3.0.84 in this release). |
| H3 | X-Men | 2015-12 | 1.4.0.246 | +314/−242 types | **substantial** | New `Redbox.HAL.Configuration`, script natives, DataMatrix framework rewrite (ClearImage moved out). |
| H3a | Batman | 2016-04 | 1.8.0.22 | +83 to +413 types per assembly; MSHALTester +413 types | **substantial** | Large controller, script and MSHALTester growth; halmgmtc +145 types. |
| H4 | Hawkeye | 2016-10 | 1.9.0.49 | +663/−505 types; MSHALTester +500 types | **substantial** | New `Redbox.HAL.Component.Model`, new IPC framework (0.85), core descriptors; compression code moved out of Core. |
| H5 | Legion | 2017-07 | 1.10.0.32 (halmgmtc 1.10.0.14) | +205/−144 types | borderline | IPC framework 1.10 (sockets, pipes, client/server), data-storage table types. `halmgmtc` 1.10.0.14 = current `src/`. |
| – | Superman | 2018-04 | 1.10.0.74 | +76/−18 types | minor | Controller services, configuration additions. |
| H6 | ZeldaSR2 | 2018-06 | 1.10.2.5 | +122/−51 types | borderline | DirectShow camera code merged into the script framework. |
| H7 | KISS | 2019-04 | 1.12.1.52 | +960/−639 types, +13k members | **substantial** | Script framework pollers/services/sync, Component.Model restructured (services, threading, timers, tokenizer, compression), Cortex barcode decoder. |
| – | 1.12.1.56, QueenSR6, U2, Yes | 2019 – 2022 | 1.12.1.56 – .71 | < 50 members | minor | |
| – | Z | 2023-08 | 1.13.0.27 | +176/−153 types (renames) | current | = `src/`. |

### Update manager (`kop-update-manager`)

| # | Release | Date | Version | Size | Class | Reason |
|---|---|---|---|---|---|---|
| U1 | (first) | 2014-02 | updatemgrd/updatemgr-ui 2.8.0.5, update-console 2.6.0.26 | first | **substantial** | Older update manager with Windows Update Agent (`Redbox.WUA`) support and a messaging subsystem, both removed in 3.0. |
| – | The-Martian | 2015-12 | 2.8.0.23 | churn in merged libraries | minor | |
| – | Catwoman | 2016-10 | 3.0.0.43 | −1,376 types | current | = `src/` (WUA and messaging removed). |

### Device Service (`device-service`), first released 2019

| # | Release | Date | Version | Size | Class | Reason |
|---|---|---|---|---|---|---|
| D1 | OasisSR | 2019-09 | 1.3.0.64 | first | **substantial** | First Device Service (EMV card reader web API), with **Bluefin** P2PE client, KDS, `Status.Client`, `HttpHelper`. |
| – | PrimusSR3, Sublime | 2019-11, 2020-02 | 1.4.0.23, 1.5.2.9 | +4 types each | borderline | Most of D2's changes arrive here. |
| D2 | VanHalen | 2020-05 | 1.5.3.6 | +24/−15 types (from 1.3) | borderline | UC285 card reader proxy, card-reader state events. |
| – | Yes | 2020-08 | 1.5.3.7 | +3 members | minor | |
| D3 | (1.10.6.0) | 2022-02 | 1.10.6.0 | +27/−16 types, new `RBAInstaller` | **substantial** | Reader firmware/RKI installer (`RBAInstaller`), domain rework. |
| – | Lugnut, Z | 2022-10, 2023-08 | 1.10.8.0, 1.10.13.0 | version only | current | = `src/` (1.10.13.0). |

### Kiosk Client Service (`kioskclient`), first released 2020

| # | Release | Date | Version | Size | Class | Reason |
|---|---|---|---|---|---|---|
| K0 | Sublime | 2020-02 | 0.0.1.120 | first (167 types) | **substantial** | Pre-release KioskClientService (version 0.0.1). |
| K1 | (unlabelled) | 2020-05 | 1.0.11.0 | +28 types | **substantial** | First 1.0 KioskClientService. |
| – | Yes | 2020-08 | 1.0.12.0 | +2/−2 types | minor | |
| K2 | LincolnSR3 | 2022-02 | 1.0.108.0 | +639/−86 types, +7k members | **substantial** | Transactions, subscriptions, promotions, proxy API, validation. |
| – | McLaren, Nova | 2022-03 | 1.0.110 – 1.0.111 | < 5 members | minor | |
| – | Omni, Pinto, Quattro, 1189 | 2022-05 – 2022-10 | 1.0.116 – 1.0.133 | +32 to +67 types each | minor/borderline | Welcome-screen service (1.0.128, removed again in 1.0.174). |
| – | Tesla, Ultima, Vega | 2023-01 – 2023-03 | 1.0.151 – 1.0.160 | +24 to +107 types each | borderline | Campaigns (1.0.151), encryption service (1.0.154). |
| K3 | Willys | 2023-04 | 1.0.166.0 | +128/−38 types | **substantial** | Marketing, authentication, installer, assets and kiosk-engine-API services. |
| – | Xterra … Bullwinkle | 2023-06 – 2023-11 | 1.0.174 – 1.0.203 | +1 to +75 types each | minor/borderline | Session, kiosk/dual, product catalog, profile data, data tables. |
| – | Calvin | 2024-05 | 1.0.207.0 | | current | = `src/`. |

### Update Client Service (`updateclient`), first released 2022

| # | Release | Date | Version | Size | Class | Reason |
|---|---|---|---|---|---|---|
| C0 | updateclient-NEW | 2020-08 | 0.0.1.32 | first (60 types) | **substantial** | Pre-release UpdateClientService (version 0.0.1). |
| C1 | UpdateClient-210 | 2022-02 | 1.2.29.0 | +239/−20 types | **substantial** | First UpdateClientService (with `Redbox.NetCore.Logging`/`Middleware`). |
| – | Exhaust, Fuse | 2022-04/06 | 1.2.41, 1.2.42 | +55 types / −27 types | minor | Segment analytics. |
| C2 | Flywheel | 2022-11 | 1.2.62.0 | +195/−116 types | borderline | File-set download rework. |
| – | 1.2.69, Z | 2023 | 1.2.69, 1.2.70 | small | minor | Data-update service. |
| – | Donatello | 2024-06 | 1.2.75.0 | +80/−25 types | current | = `src/` (proxy API, kiosk certificate, broker). |

### Lua bundles (`kop-rental-bundles`, `kop-shared-bundles`) and scripts

These are not .NET: they are zips of Lua scripts and XML/image resources loaded by the engine. "Recovering"
them means extracting them, not decompiling.

| # | Bundle | Date | Size | Class | Reason |
|---|---|---|---|---|---|
| B1 | `library-tickets.bundle` | 2014-02-03 | 252 entries (49 Lua scripts, 46 images) | **substantial (undeployed)** | The tickets application. Shipped once, never updated, absent from later kiosks. |
| B2 | `ra-2.x.bundle`, `fm.bundle`, `cp.bundle`, `library-rental-common.bundle` | 2014-02 | 818 / 366 / 84 / 69 entries | **substantial** | The Lua rental application before the WPF UI. |
| – | `ra-2.x.bundle` changes | 2015-12 – 2018-04 | +61/−31 (The-Martian), +10/−16 (Batman), +19/−95 (Hawkeye), +16/−66 (PenguinSR), +37/−60 (Superman), +4/−41 (venomSR), −39 (YodaSR2) entries | borderline | The Lua app shrinking as the WPF UI takes over. |
| – | rental bundles 2019 – 2024 | | ≤ 4 entries per release | minor | |
| – | `kop-shared-bundles` (`content-*`) | 2014 – 2024 | 633 revisions | minor (content) | Pricing, promo, legal, marketing content. |
| – | `kop-scripts` | 2014 – 2022 | 11 Lua scripts | minor | Update-manager schedules/loader. |
| – | `dvd-data` | | | skipped | Title database. |

### Store installer (`Windows10-Store-Installer-3.1.0.48-setup.exe`)

A self-extracting installer (`host.exe` 2.0.0.69, built 2018-06; contents up to 2020-01) whose resources
are .NET Framework 4.7.2, Windows patches, Kaseya/VSA agent setup, NXLog, the Ingenico USB driver, NAnt and
these Redbox binaries:

| Binary | Version | In `src/`? |
|---|---|---|
| `installer-front-end.exe` | 3.0.1.9 | yes, same version |
| `KioskShell.exe` | 3.1.0.22 | yes, same version |
| `KioskAdmin.exe` (2012) | 1.0.2.4 | **no** (21 types) |
| `Redbox.NAnt.*.Tasks`, `Redbox.Nant.Tasks`, `Redbox.TimeKeeper`, `Redbox.TimeKeeperGateway` (2008) | 1.7.5.397 | **no** (8 small assemblies, 47 types) |

Its scripts contain plain-text credentials (written as `<redacted>` wherever they are mentioned); nothing from
the installer is committed.

## Reproducing

```
python -I tools/history/rebuild_store.py --work <work> --xdelta "<image>/REDS/Update Manager/bin/xdelta3.exe" U:/data/data-*.zip "<image>/REDS/Update Manager/.store/.repository"
dotnet build -c Release tools/history/ApiSurface -o <work>/apisurface
python -I tools/history/history.py --work <work> --apisurface <work>/apisurface/ApiSurface.exe
```

`<work>/summary.tsv` has one line per release; `<work>/timeline.md` lists every changed file.
