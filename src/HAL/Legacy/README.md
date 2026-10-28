# Legacy HAL camera plugins (own solution)

These four assemblies (version 1.3.0.119, built 2014) are still present in the update manager's staging
folders (`halservice/bin/.staged_` and `halservice/bin/camera.plugins/.staged_`), but not in the live `bin`
folders, and nothing in the current HAL (1.13) references them.

| Project | Shipped as |
|---|---|
| `Redbox.HAL.Camera.Framework` | `halservice/bin/.staged_/Redbox.HAL.Camera.Framework.dll` |
| `Redbox.HAL.ClearimageV1` | `halservice/bin/.staged_/Redbox.HAL.ClearimageV1.dll` |
| `Redbox.HAL.DirectShow.Plugin` | `halservice/bin/camera.plugins/.staged_/Redbox.HAL.DirectShow.Plugin.dll` |
| `Redbox.HAL.ICameraPlugin.Vidcap` | `halservice/bin/camera.plugins/.staged_/Redbox.HAL.ICameraPlugin.Vidcap.dll` |

They were compiled against `Redbox.HAL.Core` / `Redbox.HAL.IPC.Framework` **1.3.0.119**, whose API is very
different from 1.13 (`DeviceDescriptor`, `XmlConfiguration`, `ICameraPlugin` in `Redbox.HAL.Core`,
`AbstractPipeClientSession`, ...). 1.3.0.119 itself is not recoverable: the update-store history
([docs/version-history.md](../../../docs/version-history.md)) lists it only as placeholders. The nearest
recoverable version is **1.3.0.84** (HAL "Caddyshack 1.0", 2014), so this folder has:

| Project | Version | Source |
|---|---|---|
| `Redbox.HAL.Core` | 1.3.0.84 | branch `history/hal`, release Caddyshack 1.0 |
| `Redbox.HAL.IPC.Framework` | 1.3.0.84 | branch `history/hal`, release Caddyshack 1.0 |

Build them with their own solution (they reuse the assembly names of the 1.13 HAL, so they are **not part of
`Redbox.sln`**):

```
msbuild src/HAL/Legacy/HAL.Legacy.sln -restore -p:Configuration=Release
```

Changes from the decompiled source, all to make it compile:

- `Redbox.HAL.Core` 1.3.0.84: `ICameraPlugin.SupportsReset` added. 1.3.0.119 added it (the plugins implement
  and call it) and 1.4.0.246 still has it; the rest of the 1.3.0.84 interface matches what the plugins use.
  `SevenZipHelper` (merged into the original with the LZMA SDK) restored from the raw decompile.
- `Redbox.HAL.Camera.Framework`: casts to the base class (`((XmlConfiguration)this).`,
  `((AbstractPipeClientSession)x).`) removed and the finalizer's explicit `Finalize()` call dropped. ILSpy
  wrote these because it decompiled the plugin without its 1.3.0.119 references.
- `Redbox.HAL.ClearimageV1`: the COM classes implement the one-slot vtable gaps (`_VtblGap8`, ...) of their
  ClearImage interfaces as `extern` methods.
- `Redbox.HAL.ICameraPlugin.Vidcap`: `ICameraPlugin` written as `global::Redbox.HAL.Core.ICameraPlugin` (it
  clashes with the plugin's own namespace); `stdole` taken from the .NET Framework primary interop assemblies.
