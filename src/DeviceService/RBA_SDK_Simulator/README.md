# RBA_SDK_Simulator (reconstructed)

**Not decompiled.** `RBA_SDK_Simulator.dll` was never shipped in the kiosk image. This is a new
implementation written against the contract that `DeviceService.WebApi` expects, so the device service can
run without an Ingenico iUC285 card reader attached.

## The contract it fulfils

`DeviceService.WebApi/Startup.cs` loads the simulator by reflection when `AppSettings:UseSimulator` is `true`:

```csharp
Assembly assembly = Assembly.LoadFrom(".\\RBA_SDK_Simulator.dll");
services.AddMvc().AddApplicationPart(assembly).AddControllersAsServices();
services.AddSingleton(assembly.GetType("RBA_SDK_Simulator.ISimulatorDataProvider"), assembly.GetType("RBA_SDK_Simulator.SimulatorDataProvider"));
services.AddSingleton(assembly.GetType("RBA_SDK_Simulator.ICommandQueue"), assembly.GetType("RBA_SDK_Simulator.CommandQueue"));
services.AddSingleton(typeof(IRBA_API), assembly.GetType("RBA_SDK_Simulator.RBA_API_Simulator"));
```

Those five type names, and the fact that the assembly carries MVC controllers, are everything known about
the original. The rest (behaviour, controller routes, data) is new.

| Type | Role |
|---|---|
| `RBA_API_Simulator : IRBA_API` | Replaces `RBA_SDK.RBA_API` (the P/Invoke wrapper around Ingenico's native `RBA_SDK.dll`). Emulates the RBA request/response parameters, the unsolicited terminal messages raised through `pinpadHandler`, and the card read / EMV message sequences `IUC285Proxy` drives. |
| `ISimulatorDataProvider` / `SimulatorDataProvider` | Terminal state: 07 unit data, 08 health stats, configuration (60/61), variables (28/29), files (62/65), tamper flag, default card. |
| `ICommandQueue` / `CommandQueue` | Customer actions waiting to happen: swipe, chip insert/remove, tap, cancel, bad read. |
| `SimulatorController` | REST API (served by the device service itself) to queue actions and change terminal state. |

## Using it

1. Build `RBA_SDK_Simulator.csproj` and copy `RBA_SDK_Simulator.dll` next to `DeviceService.WebApi.exe`
   (the service sets its working directory to its own folder at start-up).
2. In the device service's `appsettings.json` set `"AppSettings": { "UseSimulator": "true", ... }`.
3. Start the device service. It connects to the simulated terminal on any COM port name.
4. Drive it over HTTP (default service URL `http://localhost:9000`):

| Request | Effect |
|---|---|
| `GET api/Simulator` | State: connected, pending card read, card inserted, EMV step, queued actions, config, variables |
| `GET api/Simulator/History` | Last 200 SDK calls and terminal messages |
| `GET api/Simulator/Cards` | Preset test cards (Visa, Mastercard, Amex, Discover, RedboxGiftCard) |
| `POST api/Simulator/Swipe` | Magnetic stripe swipe |
| `POST api/Simulator/Insert` | Chip insert; full contact EMV flow (33.00 → 33.02 → 33.03 → 33.04 → 33.05) |
| `POST api/Simulator/Remove` | Remove the inserted chip card |
| `POST api/Simulator/Tap` | Contactless EMV tap |
| `POST api/Simulator/TapMsd` | Contactless magnetic-stripe-data tap |
| `POST api/Simulator/Cancel` / `BadRead` | Customer cancel / unreadable card |
| `DELETE api/Simulator/Queue` | Drop queued actions |
| `PUT api/Simulator/DefaultCard` | Card used when an action names none |
| `PUT api/Simulator/Tampered/{true\|false}` | Report a tamper (unit's manufacturing serial number becomes `ALERT`) on the next connect |
| `POST api/Simulator/Detach` / `Attach` | Unplug / plug in the terminal |
| `PUT api/Simulator/Config/{group}/{index}` and `PUT api/Simulator/Variable/{id}` | Preset configuration / variables (JSON string body) |
| `PUT api/Simulator/ResponseDelay/{ms}` | Delay before unsolicited terminal messages (default 100 ms) |

Card actions take an optional JSON body:
`{ "preset": "Mastercard", "decline": false, "mobileWallet": false, "encrypted": false }`
or `{ "card": { "pan": "...", "expiry": "YYMM", "aid": "A0000000031010", ... } }`.
Actions are queued and run when the service starts a card read (Remove runs as soon as a card is inserted),
so you can queue the swipe before the kiosk asks for a card.

## Behaviour and limits

- Swipes return OnGuard-format card data. By default it is the clear layout (tracks included). With
  `encrypted: true` it uses the encrypted layout with **dummy** KSN and cryptograms. These have the right
  shape but no processor can decrypt them.
- EMV reads return tags 4F, 50, 57, 5A, 5F20, 9F27, 8A and 9F6E (Visa/Mastercard mobile-wallet detection).
  `decline: true` makes the chip answer AAC at 33.03, which `IUC285Proxy` reports as `EMVCardDecline`.
- 97 Reboot disconnects for about 5 seconds and increments the reboot counter.
- VAS (Apple/Google wallet passes, message 17.6) is acknowledged but returns no passes. `IUC285Proxy` only
  enables VAS for specific RBA application builds and keys, which the simulated unit doesn't report.
- Only test card numbers are built in.

The smoke test in `tests/RBA_SDK_Simulator.SmokeTest` runs the real `IUC285Proxy` against the simulator
through every flow above.
