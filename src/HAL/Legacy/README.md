# Legacy HAL camera plugins (not built)

These four assemblies (version 1.3.0.119, built 2014) are still present in the update manager's staging
folders (`halservice/bin/.staged_` and `halservice/bin/camera.plugins/.staged_`), but not in the live `bin`
folders, and nothing in the current HAL (1.13) references them.

| Project | Shipped as |
|---|---|
| `Redbox.HAL.Camera.Framework` | `halservice/bin/.staged_/Redbox.HAL.Camera.Framework.dll` |
| `Redbox.HAL.ClearimageV1` | `halservice/bin/.staged_/Redbox.HAL.ClearimageV1.dll` |
| `Redbox.HAL.DirectShow.Plugin` | `halservice/bin/camera.plugins/.staged_/Redbox.HAL.DirectShow.Plugin.dll` |
| `Redbox.HAL.ICameraPlugin.Vidcap` | `halservice/bin/camera.plugins/.staged_/Redbox.HAL.ICameraPlugin.Vidcap.dll` |

They were compiled against `Redbox.HAL.Core` / `Redbox.HAL.IPC.Framework` **1.3.0.119**, whose API is
very different from 1.13 (`DeviceDescriptor`, `DeviceHelper`, `XmlConfiguration`, `ICameraPlugin` in
`Redbox.HAL.Core`, `AbstractPipeClientSession`, `IClearImageReader`, ...). Those 1.3 libraries are not in
the kiosk image or its update store (whose oldest HAL is 1.12.1.71), so these projects are kept as
decompiled source only and are **not part of `Redbox.sln`**.

They can be made to build by decompiling HAL 1.3.0.119 (for example from an older setup/data package)
into projects here and pointing these projects at them.
