# Kiosk update store (version history)

The kiosk's update manager (`updatemgrd`) keeps every update it has received in
`REDS/Update Manager/.store/` in the kiosk image. It is the source for recovering **older versions** of the
Redbox assemblies. Older versions have already filled two gaps in this repository, and they could provide
source history and the libraries that some excluded projects need. The archived snapshots in `U:\data` and
this store have since been rebuilt in full; the result is in [version-history.md](version-history.md).

## Layout

```
.store/
  .repository/<package>/        one folder per update package
    <revision>-<SHA1(path)>.patch   content (see below)
    .revlog/<n>                 JSON revisions (file names, versions, install paths)
  .configfiles/ .data/ .filecache/ .filesets/ .storefiles/
```

Revision JSON:

```json
{ "Hash": "<revision hash>", "Label": null,
  "Changes": [ { "VersionHash": "<sha1>", "ContentHash": "<sha1>", "TargetName": "DeviceService.Client.Core.dll",
                 "TargetPath": "C:\\Program Files\\redbox\\REDS\\DeviceService\\bin", "IsSeed": true, "Composite": true } ] }
```

Every content file is named `<revision hash>-<SHA1 of "TargetPath\TargetName">.patch` (upper-case hex): one
file per changed file per revision. In this image 1,736 of the 3,274 `.patch` files match a revision still in
`.revlog`; the rest belong to revisions that were trimmed from the log (almost all in `dvd-data`). There are
three kinds of content:

- **VCDIFF deltas** (magic `D6 C3 C4`), applied with `xdelta3 -d -s` to the previous version of the same file.
- **Seeds**: complete files (they start with `MZ`, or are zip/gzip archives).
- Other: text files and scripts.

The complete protocol and store format, including how revisions are downloaded, staged and activated, is in
[systems/update-manager.md](systems/update-manager.md).

The code that writes and applies these is in this repository: `Redbox.UpdateManager.Environment` and
`Redbox.DeltaCompression`. An xdelta3 command-line build can also apply VCDIFF deltas.

## Packages (counts from the image used for this repo)

| Package | Revisions | Content files | VCDIFF | Complete binaries (MZ) | Distinct file names |
|---|---|---|---|---|---|
| kop-v2-engine | 364 | 1184 | 1051 | 103 | 130 |
| kop-shared-bundles | 635 | 61 | 48 | – (9 zip) | 10 |
| kop-rental-bundles | 182 | 110 | 100 | – (6 zip) | 7 |
| kop-hal-install | 74 | 87 | 21 | 27 | 63 |
| kioskclient | 73 | 69 | 20 | 40 | 46 |
| kop-ms-hal-tester | 58 | 8 | 1 | 1 | 4 |
| device-service | 55 | 103 | 14 | 67 | 86 |
| kop-tools | 55 | 11 | 1 | 3 | 7 |
| updateclient | 34 | 94 | 16 | 66 | 75 |
| kop-scripts | 17 | 18 | 4 | – | 11 |
| kop-update-manager | 15 | 20 | – | 11 | 17 |
| dvd-data | 4 | 1545 | 1540 | – | 2 |

## Already found among the complete binaries (no patching needed)

Engine and kernel extensions 1.71.0.21, HAL 1.12.1.71 (HalUtilities 1.12.1.65), DeviceService 1.10.6.0 and
1.5.0.59, KioskClientService.API 1.0.108, UpdateClientService.API 1.2.29, Redbox.Core 1.0.7.4, Lua 1.2.1.3,
Macros 1.0.2.15, IPC 1.1.1.2, UpdateManager 3.0.0.36. Two types used in this repository came from these
(see `docs/decompilation.md`).

## What rebuilding the history would give

- Source history for each assembly (decompile every version and commit them in order).
- `Redbox.KioskEngine.ComponentModel` 1.81, so `Redbox.KioskServices.Kernel/.KernelExtension/.Proxy`
  (excluded today) can be built against the engine version they were made for.
- HAL 1.3.0.119 for `src/HAL/Legacy` is **probably not** here (the oldest HAL in the store is 1.12). The
  2014 setup/data packages use the same store format and are the likely source.

Approach: parse `.revlog` to map hashes to file names and install paths; start from the complete files;
apply each file's `.patch` deltas in revision order; verify each result's SHA1 against the change's `VersionHash`; index the results by
assembly name and version.
