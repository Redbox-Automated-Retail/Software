// Smoke test: drives the real DeviceService IUC285Proxy against RBA_SDK_Simulator through every card flow.
// Run: dotnet run --project tests/RBA_SDK_Simulator.SmokeTest   (needs the x86 .NET 8 runtime). Exit code = failures.
using DeviceService.ComponentModel;
using DeviceService.ComponentModel.Requests;
using DeviceService.ComponentModel.Responses;
using DeviceService.Domain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RBA_SDK_Simulator;

var data = new SimulatorDataProvider { ResponseDelay = 20 };
var queue = new CommandQueue();
var sim = new RBA_API_Simulator(data, queue, new ConsoleLog<RBA_API_Simulator>(quiet: true));
var proxy = new IUC285Proxy(sim, new ConsoleLog<IUC285Proxy>(quiet: true), null, null, null, new Monitor());

int failures = 0;
void Check(string name, bool ok, string detail = "")
{
	Console.WriteLine($"{(ok ? "PASS" : "FAIL")}  {name} {detail}");
	if (!ok) failures++;
}

Check("Connect", proxy.Connect());
var unit = proxy.UnitData;
Check("UnitData", unit?.DeviceType == "iUC285", $"device={unit?.DeviceType} serial={unit?.UnitSerialNumber}");
var health = proxy.GetUnitHealth();
Check("Health", health?.DeviceName == "iUC285", $"reboots={health?.RebootCount}");
Check("SupportsEMV", proxy.SupportsEMV);
Check("CardInserted=Removed", proxy.CheckIfCardInserted() == InsertedStatus.Removed);

async Task<Base87CardReadModel> Read(SimulatorCommand cmd, DeviceInputType input, decimal amount = 1.50m)
{
	Base87CardReadModel result = null;
	queue.Enqueue(cmd);
	await proxy.ReadCard(new CardReadRequest { Amount = amount, InputType = input, Timeout = 30000, VasMode = VasMode.PayOnly },
		CancellationToken.None, m => result = m, null);
	return result;
}

string Describe(Base87CardReadModel m) => m == null ? "null" :
	$"{m.GetType().Name} status={m.Status} source={m.CardSource} {m.FirstSix}..{m.LastFour} exp={m.Expiry} errors=[{string.Join(",", m.Errors?.Select(e => e.Code) ?? [])}]";

var swipe = await Read(new SimulatorCommand { Type = SimulatorCommandType.Swipe }, DeviceInputType.MCS);
Check("Swipe (clear)", swipe is UnencryptedCardReadModel && swipe.FirstSix == "476173" && swipe.LastFour == "0010", Describe(swipe));

var encCard = SimulatedCard.Preset("Mastercard"); encCard.Encrypted = true;
var enc = await Read(new SimulatorCommand { Type = SimulatorCommandType.Swipe, Card = encCard }, DeviceInputType.MCS);
Check("Swipe (encrypted)", enc is EncryptedCardReadModel e1 && e1.FirstSix == "541333" && !string.IsNullOrEmpty(e1.KSN), Describe(enc));

var gift = await Read(new SimulatorCommand { Type = SimulatorCommandType.Swipe, Card = SimulatedCard.Preset("RedboxGiftCard") }, DeviceInputType.M);
Check("Swipe (gift card)", gift is UnencryptedCardReadModel g && g.CardType == CardType.RedboxGiftCard, Describe(gift));

var chip = await Read(new SimulatorCommand { Type = SimulatorCommandType.InsertChip }, DeviceInputType.MCS);
Check("Insert chip (approve)", chip is EMVCardReadModel && chip.FirstSix == "476173" && !chip.Errors.Any(), Describe(chip));
Check("Card inserted", proxy.CheckIfCardInserted() == InsertedStatus.Inserted);
queue.Enqueue(new SimulatorCommand { Type = SimulatorCommandType.RemoveCard });
Thread.Sleep(600);
Check("Card removed", proxy.CheckIfCardInserted() == InsertedStatus.Removed);

var declineCard = SimulatedCard.Preset("Discover"); declineCard.Decline = true;
var declined = await Read(new SimulatorCommand { Type = SimulatorCommandType.InsertChip, Card = declineCard }, DeviceInputType.MCS);
Check("Insert chip (decline)", declined?.Errors.Any(x => x.Code == "EMVCardDecline") == true, Describe(declined));
queue.Enqueue(new SimulatorCommand { Type = SimulatorCommandType.RemoveCard });
Thread.Sleep(600);

var tap = await Read(new SimulatorCommand { Type = SimulatorCommandType.Tap, Card = SimulatedCard.Preset("Mastercard") }, DeviceInputType.MCS);
Check("Tap (EMV contactless)", tap is EMVCardReadModel && tap.CardSource == CardSourceType.EMVContactless && tap.LastFour == "0434", Describe(tap));

var mobileCard = SimulatedCard.Preset("Visa"); mobileCard.MobileWallet = true;
var mobile = await Read(new SimulatorCommand { Type = SimulatorCommandType.Tap, Card = mobileCard }, DeviceInputType.MCS);
Check("Tap (mobile wallet)", mobile?.CardSource == CardSourceType.Mobile, Describe(mobile));

var msd = await Read(new SimulatorCommand { Type = SimulatorCommandType.TapMsd }, DeviceInputType.MCS);
Check("Tap (MSD)", msd?.CardSource == CardSourceType.MSDContactless && msd.FirstSix == "476173", Describe(msd));

var cancel = await Read(new SimulatorCommand { Type = SimulatorCommandType.Cancel }, DeviceInputType.MCS);
Check("Customer cancel", cancel?.Errors.Any(x => x.Code == "BadExitType") == true, Describe(cancel));

var bad = await Read(new SimulatorCommand { Type = SimulatorCommandType.BadRead }, DeviceInputType.MCS);
Check("Bad read", bad?.Errors.Any(x => x.Code == "BadExitType") == true, Describe(bad));

string v = null; Check("Config round-trip", proxy.WriteConfig("13", "14", "abc") && proxy.ReadConfig("13", "14", out v) && v == "abc", v);

data.Tampered = true;
sim.SimulateDisconnect();
Thread.Sleep(200);
Check("Disconnect noticed", !proxy.IsConnected);
sim.SimulateAttach();
proxy.Connect();
Thread.Sleep(500);
Check("Tamper detected", proxy.UnitData?.IsTampered == true, $"mfgSerial={proxy.UnitData?.ManufacturingSerialNumber} errorState={proxy.ErrorState}");

Console.WriteLine(failures == 0 ? "ALL PASSED" : $"{failures} FAILED");
Console.WriteLine("--- simulator history (last 15) ---");
foreach (var line in sim.History.TakeLast(15)) Console.WriteLine("  " + line);
return failures;

class Monitor : IOptionsMonitor<DeviceServiceConfig>
{
	public DeviceServiceConfig CurrentValue { get; } = new DeviceServiceConfig { EnableVasCommands = false };
	public DeviceServiceConfig Get(string name) => CurrentValue;
	public IDisposable OnChange(Action<DeviceServiceConfig, string> listener) => null;
}

class ConsoleLog<T>(bool quiet) : ILogger<T>
{
	public IDisposable BeginScope<TState>(TState state) => null;
	public bool IsEnabled(LogLevel logLevel) => !quiet || logLevel >= LogLevel.Warning;
	public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
	{
		if (IsEnabled(logLevel)) Console.WriteLine($"    [{logLevel}] {formatter(state, exception)}");
	}
}
