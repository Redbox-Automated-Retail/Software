# Assembly inventory

Every Redbox-authored .NET binary found in the kiosk image, and the project that rebuilds it.
Paths are relative to the root of the kiosk image. Where a binary exists both in `bin/` and
`bin/.staged_/`, the `.staged_` copy (the newest pending update) was decompiled.

## src/Common

| Project | Version | Target | Shipped as | Notes |
|---|---|---|---|---|
| `Redbox.Command.Tokenizer` | 1.0.1.3 | net46 | `REDS/Kiosk Engine/bin/Redbox.Command.Tokenizer.dll` |  |
| `Redbox.Compression` | 1.0.1.11 | net46 | `REDS/Kiosk Engine/bin/Redbox.Compression.dll` |  |
| `Redbox.Core` | 1.2.0.1 | net46 | `REDS/Kiosk Engine/bin/Redbox.Core.dll` |  |
| `Redbox.GetOpts` | 1.2.0.1 | net46 | `REDS/Kiosk Engine/bin/Redbox.GetOpts.dll` |  |
| `Redbox.IPC.Framework` | 1.3.0.1 | net46 | `REDS/Kiosk Engine/bin/Redbox.IPC.Framework.dll` |  |
| `Redbox.JSONPrettyPrinter` | 1.2.0.3 | net46 | `REDS/Kiosk Engine/bin/Redbox.JSONPrettyPrinter.dll` |  |
| `Redbox.Log.Framework` | 1.5.0.1 | net46 | `REDS/Kiosk Engine/bin/Redbox.Log.Framework.dll` |  |
| `Redbox.Lua` | 1.4.0.1 | net46 | `REDS/Kiosk Engine/bin/Redbox.Lua.dll` |  |
| `Redbox.Macros` | 1.2.0.1 | net46 | `REDS/Kiosk Engine/bin/Redbox.Macros.dll` |  |
| `Redbox.Macros.Functions` | 1.2.0.1 | net46 | `REDS/Kiosk Engine/bin/Redbox.Macros.Functions.dll` |  |
| `Redbox.ProductLookupCatalog` | 1.3.0.2 | net46 | `REDS/Kiosk Engine/bin/Redbox.ProductLookupCatalog.dll` |  |
| `Redbox.REDS.Framework` | 1.4.0.1 | net46 | `REDS/Kiosk Engine/bin/Redbox.REDS.Framework.dll` |  |
| `Redbox.Tokenizer.Framework` | 1.1.0.1 | net46 | `REDS/Kiosk Engine/bin/Redbox.Tokenizer.Framework.dll` |  |
| `Redbox.USB` | 1.3.0.1 | net46 | `REDS/Kiosk Engine/bin/Redbox.USB.dll` |  |

## src/DeviceService

| Project | Version | Target | Shipped as | Notes |
|---|---|---|---|---|
| `BluefinService.Client` | 1.0.0.0 | netcoreapp2.2 | `REDS/DeviceService/bin/BluefinService.Client.Dll` |  |
| `DeviceService.Client` | 1.10.0.4 | net461 | `REDS/Kiosk Engine/bin/DeviceService.Client.dll` |  |
| `DeviceService.Client.Core` | 1.10.13.0 | netcoreapp2.2 | `REDS/DeviceService/bin/DeviceService.Client.Core.dll` |  |
| `DeviceService.ComponentModel` | 1.10.13.0 | netstandard2.0 | `REDS/DeviceService/bin/DeviceService.ComponentModel.dll`<br>`REDS/Kiosk Engine/bin/DeviceService.ComponentModel.dll` |  |
| `DeviceService.Domain` | 1.10.13.0 | netcoreapp2.2 | `REDS/DeviceService/bin/DeviceService.Domain.dll` |  |
| `DeviceService.WebApi` | 1.10.13.0 | netcoreapp2.2 | `REDS/DeviceService/bin/DeviceService.WebApi.dll` |  |
| `HttpHelper` | 1.0.0.0 | netcoreapp2.2 | `REDS/DeviceService/bin/HttpHelper.dll` |  |
| `RBA_SDK_ComponentModel` | 1.10.13.0 | netstandard2.0 | `REDS/DeviceService/bin/RBA_SDK_ComponentModel.dll` |  |
| `RBA_SDK_STANDARD` | 1.10.13.0 | netstandard2.0 | `REDS/DeviceService/bin/RBA_SDK_STANDARD.dll` |  |
| `RBAInstaller` | 1.10.13.0 | netcoreapp2.2 | `REDS/DeviceService/bin/RBAInstaller.dll` |  |
| `Status.Client` | 1.0.0.0 | netcoreapp2.2 | `REDS/DeviceService/bin/Status.Client.dll` |  |

## src/HAL

| Project | Version | Target | Shipped as | Notes |
|---|---|---|---|---|
| `CameraTuner` | 1.13.0.27 | net35 | `halservice/bin/CameraTuner.exe` |  |
| `HAL.PackageInstaller` | 1.13.0.27 | net35 | `halservice/bin/HAL.PackageInstaller.exe` |  |
| `halmgmtc` | 1.10.0.14 | net35 | `halservice/bin/halmgmtc.exe` | older merged copies of Redbox libraries kept inline (incompatible with current versions) |
| `halsvc-win32` | 1.13.0.27 | net35 | `halservice/bin/halsvc-win32.exe` |  |
| `HalUtilities` | 1.13.0.27 | net40 | `HAL-Tools/HalUtilities.exe` |  |
| `Interop.ClearImage` |  | net35 |  | no standalone DLL shipped; recovered from ILMerged copies in `Redbox.HAL.DataMatrix.Framework` |
| `MSHALTester` | 1.13.0.27 | net35 | `MS HAL Tester/bin/MSHALTester.exe` |  |
| `Redbox.DirectShow` |  | net35 |  | no standalone DLL shipped; recovered from ILMerged copies in `Redbox.HAL.Script.Framework` |
| `Redbox.HAL.Client` | 1.13.0.27 | net35 | `REDS/Kiosk Engine/bin/Redbox.HAL.Client.dll` |  |
| `Redbox.HAL.Common.GUI.Functions` |  | net35 |  | no standalone DLL shipped; recovered from ILMerged copies in `MSHALTester` |
| `Redbox.HAL.Component.Model` | 1.13.0.27 | net35 | `halservice/bin/Redbox.HAL.Component.Model.dll` |  |
| `Redbox.HAL.Configuration` | 1.13.0.27 | net35 | `halservice/bin/Redbox.HAL.Configuration.dll` |  |
| `Redbox.HAL.Controller.Framework` | 1.13.0.27 | net35 | `halservice/bin/Redbox.HAL.Controller.Framework.dll` |  |
| `Redbox.HAL.Core` | 1.13.0.27 | net35 | `halservice/bin/Redbox.HAL.Core.dll` |  |
| `Redbox.HAL.DataMatrix.Framework` | 1.13.0.27 | net35 | `halservice/bin/Redbox.HAL.DataMatrix.Framework.dll` |  |
| `Redbox.HAL.DataStorage` | 1.13.0.27 | net35 | `halservice/bin/Redbox.HAL.DataStorage.dll` |  |
| `Redbox.HAL.DirectShow.Frame.Plugin` | 1.13.0.27 | net35 | `halservice/bin/camera.plugins/Redbox.HAL.DirectShow.Frame.Plugin.dll` |  |
| `Redbox.HAL.IPC.Framework` | 1.13.0.27 | net35 | `halservice/bin/Redbox.HAL.IPC.Framework.dll` |  |
| `Redbox.HAL.Script.Framework` | 1.13.0.27 | net35 | `halservice/bin/Redbox.HAL.Script.Framework.dll` |  |
| `Redbox.HAL.Service.Framework` | 1.13.0.27 | net35 | `halservice/bin/Redbox.HAL.Service.Framework.dll` |  |

## src/HAL/Legacy

| Project | Version | Target | Shipped as | Notes |
|---|---|---|---|---|
| `Redbox.HAL.Camera.Framework` | 1.3.0.119 | net35 | `halservice/bin/Redbox.HAL.Camera.Framework.dll` |  |
| `Redbox.HAL.ClearimageV1` | 1.3.0.119 | net35 | `halservice/bin/Redbox.HAL.ClearimageV1.dll` | older merged copies of Redbox libraries kept inline (incompatible with current versions) |
| `Redbox.HAL.DirectShow.Plugin` | 1.3.0.119 | net35 | `halservice/bin/camera.plugins/Redbox.HAL.DirectShow.Plugin.dll` | older merged copies of Redbox libraries kept inline (incompatible with current versions) |
| `Redbox.HAL.ICameraPlugin.Vidcap` | 1.3.0.119 | net35 | `halservice/bin/camera.plugins/Redbox.HAL.ICameraPlugin.Vidcap.dll` |  |

## src/KioskClient

| Project | Version | Target | Shipped as | Notes |
|---|---|---|---|---|
| `KioskClientService.API` | 1.0.207.0 | netcoreapp2.2 | `REDS/Kiosk Client/bin/KioskClientService.API.dll` |  |

## src/KioskEngine

| Project | Version | Target | Shipped as | Notes |
|---|---|---|---|---|
| `kioskengine` | 1.88.0.80 | net472 | `REDS/Kiosk Engine/bin/kioskengine.exe` |  |
| `Redbox.ControlPanel.UI` | 1.88.0.80 | net472 | `REDS/Kiosk Engine/bin/Redbox.ControlPanel.UI.exe` |  |
| `Redbox.Controls` | 1.88.0.80 | net472 | `REDS/Kiosk Engine/bin/Redbox.Controls.dll` |  |
| `Redbox.FMA.UI` | 1.72.0.5 | net472 | `REDS/Kiosk Engine/bin/Redbox.FMA.UI.dll` |  |
| `Redbox.KioskEngine.API` | 1.88.0.80 | net472 | `REDS/Kiosk Engine/bin/Redbox.KioskEngine.API.dll` |  |
| `Redbox.KioskEngine.ComponentModel` | 1.88.0.80 | net472 | `REDS/Kiosk Engine/bin/Redbox.KioskEngine.ComponentModel.dll` |  |
| `Redbox.KioskEngine.Drawing` | 1.88.0.80 | net472 | `REDS/Kiosk Engine/bin/Redbox.KioskEngine.Drawing.dll` |  |
| `Redbox.KioskEngine.Environment` | 1.88.0.80 | net472 | `REDS/Kiosk Engine/bin/Redbox.KioskEngine.Environment.dll` |  |
| `Redbox.KioskEngine.IDE` | 1.88.0.80 | net472 | `REDS/Kiosk Engine/bin/Redbox.KioskEngine.IDE.dll` |  |
| `Redbox.KioskEngine.Kernel` | 1.88.0.80 | net472 | `REDS/Kiosk Engine/bin/Redbox.KioskEngine.Kernel.dll` |  |
| `Redbox.KioskEngine.PreferenceManager` | 1.88.0.80 | net472 | `REDS/Kiosk Engine/bin/Redbox.KioskEngine.PreferenceManager.exe` |  |
| `Redbox.Services.KioskBrokerServices.KioskShared` | 0.0.0.56 | net46 | `REDS/Kiosk Engine/bin/Redbox.Services.KioskBrokerServices.KioskShared.dll` |  |
| `redscc` | 1.1.4.1 | net35 | `REDS/Kiosk Engine/bin/redscc.exe` | older merged copies of Redbox libraries kept inline (incompatible with current versions) |

## src/KioskServices

| Project | Version | Target | Shipped as | Notes |
|---|---|---|---|---|
| `Redbox.BrokerServices.Kernel` | 1.88.0.80 | net472 | `REDS/Kiosk Engine/bin/Redbox.BrokerServices.Kernel.dll` |  |
| `Redbox.BrokerServices.KernelExtension` | 1.88.0.80 | net472 | `REDS/Kiosk Engine/bin/Redbox.BrokerServices.KernelExtension.dll` |  |
| `Redbox.BrokerServices.Proxy` | 1.88.0.80 | net472 | `REDS/Kiosk Engine/bin/Redbox.BrokerServices.Proxy.dll` |  |
| `Redbox.BrokerServices.Proxy.ComponentModel` | 1.88.0.80 | net472 | `REDS/Kiosk Engine/bin/Redbox.BrokerServices.Proxy.ComponentModel.dll` |  |
| `Redbox.FraudServices.Proxy` | 1.88.0.80 | net472 | `REDS/Kiosk Engine/bin/Redbox.FraudServices.Proxy.dll` |  |
| `Redbox.FraudServices.Proxy.ComponentModel` | 1.88.0.80 | net472 | `REDS/Kiosk Engine/bin/Redbox.FraudServices.Proxy.ComponentModel.dll` |  |
| `Redbox.HardwareServices.Kernel` | 1.88.0.80 | net472 | `REDS/Kiosk Engine/bin/Redbox.HardwareServices.Kernel.dll` |  |
| `Redbox.HardwareServices.KernelExtension` | 1.88.0.80 | net472 | `REDS/Kiosk Engine/bin/Redbox.HardwareServices.KernelExtension.dll` |  |
| `Redbox.HardwareServices.Proxy` | 1.88.0.80 | net472 | `REDS/Kiosk Engine/bin/Redbox.HardwareServices.Proxy.dll` |  |
| `Redbox.HardwareServices.Proxy.ComponentModel` | 1.88.0.80 | net472 | `REDS/Kiosk Engine/bin/Redbox.HardwareServices.Proxy.ComponentModel.dll` |  |
| `Redbox.KioskDataServices.Proxy` | 1.88.0.80 | net472 | `REDS/Kiosk Engine/bin/Redbox.KioskDataServices.Proxy.dll` |  |
| `Redbox.KioskDataServices.Proxy.ComponentModel` | 1.88.0.80 | net472 | `REDS/Kiosk Engine/bin/Redbox.KioskDataServices.Proxy.ComponentModel.dll` |  |
| `Redbox.KioskServices.ComponentModel` | 2.6.0.11 | net452 | `REDS/Kiosk Engine/bin/Redbox.KioskServices.ComponentModel.dll` |  |
| `Redbox.KioskServices.Kernel` | 1.81.0.27 | net472 | `REDS/Kiosk Engine/bin/Redbox.KioskServices.Kernel.dll` |  |
| `Redbox.KioskServices.KernelExtension` | 1.81.0.27 | net472 | `REDS/Kiosk Engine/bin/Redbox.KioskServices.KernelExtension.dll` |  |
| `Redbox.KioskServices.Proxy` | 1.81.0.27 | net472 | `REDS/Kiosk Engine/bin/Redbox.KioskServices.Proxy.dll` |  |
| `Redbox.KioskServices.Proxy.ComponentModel` | 1.81.0.27 | net472 | `REDS/Kiosk Engine/bin/Redbox.KioskServices.Proxy.ComponentModel.dll` |  |
| `Redbox.UpdateServices.Kernel` | 1.88.0.80 | net472 | `REDS/Kiosk Engine/bin/Redbox.UpdateServices.Kernel.dll` |  |
| `Redbox.UpdateServices.KernelExtension` | 1.88.0.80 | net472 | `REDS/Kiosk Engine/bin/Redbox.UpdateServices.KernelExtension.dll` |  |
| `Redbox.UpdateServices.Proxy` | 1.88.0.80 | net472 | `REDS/Kiosk Engine/bin/Redbox.UpdateServices.Proxy.dll` |  |
| `Redbox.UpdateServices.Proxy.ComponentModel` | 1.88.0.80 | net472 | `REDS/Kiosk Engine/bin/Redbox.UpdateServices.Proxy.ComponentModel.dll` |  |

## src/Rental

| Project | Version | Target | Shipped as | Notes |
|---|---|---|---|---|
| `Redbox.Rental.Data` | 1.88.0.80 | net472 | `REDS/Kiosk Engine/bin/Redbox.Rental.Data.dll` |  |
| `Redbox.Rental.Kernel` | 1.88.0.80 | net472 | `REDS/Kiosk Engine/bin/Redbox.Rental.Kernel.dll` |  |
| `Redbox.Rental.Model` | 1.88.0.80 | net472 | `REDS/Kiosk Engine/bin/Redbox.Rental.Model.dll` |  |
| `Redbox.Rental.Services` | 1.88.0.80 | net472 | `REDS/Kiosk Engine/bin/Redbox.Rental.Services.dll` |  |
| `Redbox.Rental.UI` | 1.88.0.80 | net472 | `REDS/Kiosk Engine/bin/Redbox.Rental.UI.exe` |  |

## src/Installer

Not in the kiosk image: from `Windows10-Store-Installer-3.1.0.48-setup.exe` (see
[systems/store-installer.md](systems/store-installer.md)). "Shipped as" is the path inside the setup exe.

| Project | Version | Target | Shipped as | Notes |
|---|---|---|---|---|
| `host` | 2.0.0.69 | net461 | the setup exe itself (`host.exe`) | original .NET 2.0/3.5; older merged copies of Redbox libraries and the TaskScheduler wrapper kept inline; third-party merged code referenced instead |
| `KioskAdmin` | 1.0.2.4 | net45 | `KioskAdmin.zip/KioskAdmin.exe` | original .NET 2.0; passwords redacted; `TaskService.dll` (third-party) replaced by NuGet `TaskScheduler` |
| `Redbox.NAnt.Core` | 1.7.5.397 | net40 | `CustomNAntTasks.zip` | original .NET 1.1 |
| `Redbox.NAnt.CleanLogFiles.Tasks` | 1.7.5.397 | net40 | `CustomNAntTasks.zip` | original .NET 1.1 |
| `Redbox.NAnt.MessageQueuing.Tasks` | 1.7.5.397 | net40 | `CustomNAntTasks.zip` | original .NET 1.1 |
| `Redbox.NAnt.StuDiscs.Tasks` | 1.7.5.397 | net40 | `CustomNAntTasks.zip` | original .NET 1.1 |
| `Redbox.Nant.Tasks` | 1.7.5.397 | net40 | `CustomNAntTasks.zip` | original .NET 1.1 |
| `Redbox.NAnt.TimeKeeper.Tasks` | 1.7.5.397 | net40 | `CustomNAntTasks.zip` | original .NET 1.1 |
| `Redbox.TimeKeeper` | 1.7.5.397 | net35 | `CustomNAntTasks.zip` | original .NET 1.1 |
| `Redbox.TimeKeeperGateway` | 1.7.5.397 | net35 | `CustomNAntTasks.zip` | original .NET 1.1 |

The setup exe's other Redbox binaries are already in `src/`: `installer-front-end.exe` (3.0.1.9) and
`KioskShell.exe` (3.1.0.22) are byte-identical to the image's copies.

## src/Tools

| Project | Version | Target | Shipped as | Notes |
|---|---|---|---|---|
| `inventoryimport` | 1.0.0.0 | net35 | `REDS/bin/inventoryimport.exe` | older merged copies of Redbox libraries kept inline (incompatible with current versions) |
| `KioskShell` | 3.1.0.22 | net472 | `KioskShell/KioskShell.exe` |  |
| `kioskutil` | 2.5.0.13 | net35 | `REDS/bin/kioskutil.exe` | older merged copies of Redbox libraries kept inline (incompatible with current versions) |

## src/UpdateClient

| Project | Version | Target | Shipped as | Notes |
|---|---|---|---|---|
| `Redbox.NetCore.Logging` | 1.3.26.0 | netcoreapp2.1 | `REDS/Update Client/bin/Redbox.NetCore.Logging.dll` |  |
| `Redbox.NetCore.Middleware` | 1.0.140.0 | netcoreapp2.1 | `REDS/Update Client/bin/Redbox.NetCore.Middleware.dll` |  |
| `UpdateClientService.API` | 1.2.75.0 | netcoreapp2.2 | `REDS/Update Client/bin/UpdateClientService.API.dll` |  |

## src/UpdateManager

| Project | Version | Target | Shipped as | Notes |
|---|---|---|---|---|
| `installer-front-end` | 3.0.1.9 | net46 | `REDS/Update Manager/bin/installer-front-end.exe` |  |
| `Redbox.DeltaCompression` |  | net46 |  | no standalone DLL shipped; recovered from ILMerged copies in `updatemgrd` |
| `Redbox.UpdateManager.BITS` |  | net46 |  | no standalone DLL shipped; recovered from ILMerged copies in `updatemgrd` |
| `Redbox.UpdateManager.Client` | 3.2.0.2 | net46 | `REDS/Kiosk Engine/bin/Redbox.UpdateManager.Client.dll` |  |
| `Redbox.UpdateManager.ComponentModel` | 3.2.0.2 | net46 | `REDS/Kiosk Engine/bin/Redbox.UpdateManager.ComponentModel.dll` |  |
| `Redbox.UpdateManager.Environment` |  | net46 |  | no standalone DLL shipped; recovered from ILMerged copies in `updatemgrd` |
| `Redbox.UpdateManager.Kernel` |  | net46 |  | no standalone DLL shipped; recovered from ILMerged copies in `installer-front-end` |
| `Redbox.UpdateManager.Remoting` |  | net46 |  | no standalone DLL shipped; recovered from ILMerged copies in `updatemgrd` |
| `Redbox.UpdateManager.ServiceProxies` |  | net46 |  | no standalone DLL shipped; recovered from ILMerged copies in `update-console` |
| `Redbox.UpdateManager.TaskScheduler` |  | net46 |  | no standalone DLL shipped; recovered from ILMerged copies in `updatemgrd` |
| `Redbox.UpdateService.Client` | 3.0.0.5 | net46 | `REDS/Kiosk Engine/bin/Redbox.UpdateService.Client.dll`<br>`REDS/Update Manager/bin/Redbox.UpdateService.Client.dll` |  |
| `Redbox.UpdateService.Model` | 3.0.0.5 | net46 | `REDS/Kiosk Engine/bin/Redbox.UpdateService.Model.dll` |  |
| `TaskScheduler` |  | net46 |  | no standalone DLL shipped; recovered from ILMerged copies in `updatemgrd` |
| `update-console` | 3.0.0.43 | net46 | `REDS/Update Manager/bin/update-console.exe` |  |
| `updatemgr-ui` | 3.0.0.43 | net46 | `REDS/Update Manager/bin/updatemgr-ui.exe` |  |
| `updatemgrd` | 3.0.0.43 | net46 | `REDS/Update Manager/bin/updatemgrd.exe` |  |
