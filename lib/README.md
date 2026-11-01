# Third-party binaries

Libraries the decompiled projects reference that have no matching NuGet package. Each is the exact
binary shipped in the kiosk image. They are third-party software under their own licenses; some
(VistaDB, SmartPropertyGrid) are commercial.

| File | Version | Source in the image | Used by |
|---|---|---|---|
| `Common.Logging/1.2.0.0/Common.Logging.dll` | 1.2.0.0 | `REDS/Kiosk Engine/bin` | dependency of Quartz 1.0.2.3 |
| `GenuineChannels/GenuineChannels.dll` | 2.5.9.5 | `REDS/Kiosk Engine/bin` | Redbox.BrokerServices.Proxy |
| `InputSimulator/InputSimulator.dll` | 0.1.0.0 | `REDS/Kiosk Engine/bin` | kioskengine |
| `Interop/Interop.NetFwTypeLib.dll` | 1.0.0.0 | `REDS/Kiosk Engine/bin` | kioskengine (Windows Firewall COM interop) |
| `Jayrock/Jayrock.Json.dll` | 0.9.8316.0 | `REDS/Kiosk Engine/bin` | Redbox.JSONPrettyPrinter, Redbox.KioskDataServices.Proxy, update-console |
| `log4net/1.2.9/log4net.dll` | 1.2.9.0 | `halservice/bin` | HAL projects |
| `PowerCollections/PowerCollections.dll` | 1.0.2141 | `REDS/Kiosk Engine/bin` | Redbox.REDS.Framework, HAL script framework, redscc, halmgmtc |
| `Quartz/1.0.2.3/Quartz.dll` | 1.0.2.3 | `REDS/Kiosk Engine/bin` | Redbox.KioskEngine.Environment |
| `SevenZip/SevenZip.dll` | 1.0.0.0 | `REDS/Kiosk Engine/bin` | Redbox.Compression, update manager (LZMA SDK) |
| `SharpZipLib/0.85.5/ICSharpCode.SharpZipLib.dll` | 0.85.5.452 | `REDS/Kiosk Engine/bin` | many |
| `Skybound.VisualTips/Skybound.VisualTips.dll` | 1.0.2.0 | `REDS/Kiosk Engine/bin` | Redbox.KioskEngine.IDE |
| `SmartPropertyGrid/SmartPropertyGrid.dll` | 3.1.0.107 | `REDS/Kiosk Engine/bin` | Redbox.KioskEngine.IDE |
| `VistaDB/VistaDB.NET20.dll` | 3.5.1.84 | `halservice/bin` | Redbox.HAL.DataStorage, Redbox.KioskEngine.Environment, Redbox.Rental.Data |
| `WeifenLuo/WeifenLuo.WinFormsUI.Docking.dll` | 2.5.0.15095 | `REDS/Kiosk Engine/bin` | Redbox.KioskEngine.IDE |
