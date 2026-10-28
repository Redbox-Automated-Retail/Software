# Archived store snapshots (`U:\data`)

24 `data-MMDDYYYY.zip` files, 2014-02-03 to 2024-05-07, plus the kiosk image's own store (2024-07). Each zip is
a copy of a `.store/.repository` folder (format: [update-store.md](update-store.md)); every zip's SHA-256
matches the hash posted with it. How they were rebuilt and what they contain release by release:
[version-history.md](version-history.md); the engine releases in detail: [engines.md](engines.md).

## What the zips are

In every zip, `.current` and `.active` are all zeros for every package: nothing in them was ever staged or
activated. They are **seed packages**, not copies of a running kiosk. The image's own store still has one:
`.store\.repository\data.zip` is byte-for-byte `data-02182022.zip` (same SHA-256), so that kiosk was set up
from the 2022-02-18 seed and updated itself from there. Each zip is therefore the state of Redbox's update
server on the date in its name: the newest release of every package on that day, with the full history of
each package's revision log. (How the seed is installed was not traced; the store installer's resources do
not mention `data.zip` in plain text.)

Consequences:

- A revision log in a zip is cumulative: the 2024 log still lists every engine revision since 2014. What
  changes from zip to zip is which revisions still have **content** (`.patch` files). Older zips keep the
  content of releases the newer ones only list.
- Releases between two snapshots that were superseded before the next snapshot exist only as placeholders,
  unless a later release was shipped as a delta against them.

## Snapshot by snapshot

Newest release in each zip (code name from the revision label; version of the main assembly). Packages are
the engine (`kop-v2-engine`, `kioskengine.exe`), HAL (`kop-hal-install`, `Redbox.HAL.Core`; MS HAL Tester and
HAL Utilities follow it), update manager (`updatemgrd`), Device Service (`DeviceService.WebApi`), Kiosk Client
Service and Update Client Service. Revisions = length of the engine's revision log.

| Snapshot | Engine | HAL | Update manager | Device Service | Kiosk Client | Update Client | Engine revisions |
|---|---|---|---|---|---|---|---|
| 2014-02-03 | Braveheart 1.3.7.10 | Caddyshack 1.0 1.3.0.84 | AI 2.8.0.5 | – | – | – | 2 |
| 2014-02-10 | Braveheart 1.3.7.10 | Caddyshack 1.0 1.3.0.84 | Alien 2.8.0.5 | – | – | – | 2 |
| 2014-02-28 | Dracula 1.3.9.4 | Caddyshack 1.0 1.3.0.84 | AI 2.8.0.5 | – | – | – | 3 |
| 2014-03-13 | Dracula 1.3.9.4 | Caddyshack 1.0 1.3.0.84 | AI 2.8.0.5 | – | – | – | 3 |
| 2015-12-18 | The-Martian 1.9.0.9 | X-Men 1.4.0.246 | The-Martian 2.8.0.23 | – | – | – | 37 |
| 2016-04-01 | Batman 1.13.0.19 | Batman 1.8.0.22 | The-Martian 2.8.0.23 | – | – | – | 44 |
| 2016-10-28 | HawkeyeSR 1.18.0.18 | Hawkeye 1.9.0.49 | Catwoman 3.0.0.43 | – | – | – | 59 |
| 2017-07-26 | Penguin 1.25.0.4 | Legion 1.10.0.32 | Catwoman 3.0.0.43 | – | – | – | 68 |
| 2017-10-13 | SupermanSR 1.27.0.85 | Superman 1.10.0.74 | Catwoman 3.0.0.43 | – | – | – | 87 |
| 2018-02-15 | venomSR2 1.30.0.26 | Superman 1.10.0.74 | Catwoman 3.0.0.43 | – | – | – | 99 |
| 2018-04-27 | YodaSR3 1.32.1.79 | Superman 1.10.0.74 | Catwoman 3.0.0.43 | – | – | – | 107 |
| 2018-05-21 | ZeldaSR3 1.34.0.17 | ZeldaSR2 1.10.2.5 | Catwoman 3.0.0.43 | – | – | – | 117 |
| 2018-06-15 | ZeldaSR3 1.34.0.17 | ZeldaSR2 1.10.2.5 | Catwoman 3.0.0.43 | – | – | – | 117 |
| 2019-04-05 | KISS1.1 1.44.1.3 | KISS 1.12.1.52 | Catwoman 3.0.0.43 | – | – | – | 170 |
| 2019-09-16 | OasisSR3 1.48.0.85 | OasisSR 1.12.1.56 | Catwoman 3.0.0.43 | OasisSR 1.3.0.64 | – | – | 188 |
| 2019-11-05 | PrimusSR4 1.49.0.110 | OasisSR 1.12.1.56 | Catwoman 3.0.0.43 | PrimusSR3 1.4.0.23 | – | – | 195 |
| 2020-02-26 | SublimeSR 1.52.1.2 | QueenSR6 1.12.1.64 | Catwoman 3.0.0.43 | Sublime 1.5.2.9 | Sublime 0.0.1.120 | – | 213 |
| 2020-05-21 | VanHalenSR2 1.55.0.46 | U2 1.12.1.65 | Catwoman 3.0.0.43 | VanHalen 1.5.3.6 | U2 1.0.11.0 | – | 228 |
| 2020-08-04 | YesSR3 1.57.3.9 | Yes 1.12.1.71 | Catwoman 3.0.0.43 | Yes 1.5.3.7 | Yes 1.0.12.0 | NEW 0.0.1.32 | 233 |
| 2022-02-18 | LincolnSR3 1.71.0.21 | Yes 1.12.1.71 | Catwoman 3.0.0.43 | 1.10.6.0 | LincolnSR3 1.0.108.0 | 210 1.2.29.0 | 292 |
| 2023-02-07 | Studebaker 1.78.0.6 | Yes 1.12.1.71 | Catwoman 3.0.0.43 | Lugnut 1.10.8.0 | 1189 1.0.133.0 | Flywheel 1.2.62.0 | 317 |
| 2023-07-04 | Xterra 1.83.0.17 | Yes 1.12.1.71 | Catwoman 3.0.0.43 | Lugnut 1.10.8.0 | Xterra 1.0.174.0 | Gearbox 1.2.69.0 | 331 |
| 2023-10-06 | Z 1.85.0.27 | Z 1.13.0.27 | Catwoman 3.0.0.43 | Z 1.10.13.0 | Z 1.0.186.0 | Z 1.2.70.0 | 349 |
| 2024-05-07 | Calvin 1.88.0.80 | Z 1.13.0.27 | Catwoman 3.0.0.43 | Z 1.10.13.0 | Calvin 1.0.207.0 | Z 1.2.70.0 | 364 |
| image 2024-07 | Calvin 1.88.0.80 | Z 1.13.0.27 | Catwoman 3.0.0.43 | Z 1.10.13.0 | Calvin 1.0.207.0 | Donatello 1.2.75.0 | 364 (active) |

Notes per snapshot:

- **2014-02-03, 2014-02-10**: the oldest. Engine Braveheart with the tickets library (`library-tickets.bundle`
  and `Redbox.DVD.TicketServices`); the update manager is labelled `AI` in one and `Alien` in the other
  (same 2.8.0.5 binaries).
- **2014-02-28, 2014-03-13**: Dracula, a small update of Braveheart (engine 1.3.9.4); otherwise the same.
- **2015-12-18**: The-Martian. 34 engine revisions since Dracula; the .NET rental libraries
  (`Redbox.Rental.*`) appear. HAL X-Men; the 1.3.0.119 camera plugins are in this zip.
- **2016-04-01**: Batman (engine 1.13, HAL 1.8.0.22).
- **2016-10-28**: HawkeyeSR, the first WPF rental UI; HAL Hawkeye; update manager 3.0 (Catwoman), which never
  changed again.
- **2017-07-26**: Penguin. **2017-10-13**: SupermanSR, the engine ships as many separate assemblies.
- **2018-02-15 / 04-27 / 05-21 / 06-15**: venomSR2, YodaSR3, ZeldaSR3 (the last two zips are identical in
  engine and HAL).
- **2019-04-05**: KISS1.1 (fraud services, reservations, KioskDataServices) and HAL KISS.
- **2019-09-16**: first Device Service (OasisSR) and the `EMV-Installer-scripts` label in `kop-scripts`.
- **2019-11-05, 2020-02-26**: PrimusSR4, SublimeSR; the first (0.0.1) Kiosk Client Service.
- **2020-05-21, 2020-08-04**: VanHalenSR2, YesSR3; the first (0.0.1) Update Client Service.
- **2022-02-18**: after an 18-month gap, LincolnSR3: Kiosk Client 1.0.108, Update Client 1.2.29, Device
  Service 1.10.6.
- **2023-02-07 … 2024-05-07**: the 2023 car-name releases (Studebaker, Xterra), Z and Calvin.
- **image**: same as 2024-05-07 except Update Client Donatello 1.2.75; the only snapshot where everything is
  activated.
