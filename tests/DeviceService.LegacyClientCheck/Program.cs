using System;
using DeviceService.Client;

namespace DeviceService.LegacyClientCheck;

// Usage: DeviceService.LegacyClientCheck.exe [hub url]   (default http://localhost:9000/CardReaderEvents)
// Each check sends a command over the hub and waits for the matching response event, so both directions of the
// JSON hub protocol are exercised. Exit code = number of failed checks.
internal static class Program
{
	private static int _failures;

	private static int Main(string[] args)
	{
		string url = args.Length > 0 ? args[0] : "http://localhost:9000/CardReaderEvents";
		DeviceServiceClient client = new DeviceServiceClient();
		Check("connect " + url, client.ConnectToDeviceService(url, 30000), null);
		Check("IsCardReaderConnected", client.IsCardReaderConnected(), null);
		var version = client.ValidateVersion();
		Check("ValidateVersion", version.DeviceServiceVersion != null && version.DeviceServiceVersion.Major > 0, $"compatible={version.IsCompatible} service={version.DeviceServiceVersion}");
		var health = client.GetUnitHealth();
		Check("GetUnitHealth", health != null, health == null ? null : $"{health.GetType().Name}");
#pragma warning disable CS0618 // SupportsEMV is obsolete in the client, but the engine still calls it.
		Check("SupportsEMV", client.SupportsEMV(), null);
#pragma warning restore CS0618
		client.DisconnectFromDeviceService();
		Console.WriteLine(_failures == 0 ? "ALL PASSED" : $"{_failures} FAILED");
		return _failures;
	}

	private static void Check(string name, bool ok, string detail)
	{
		if (!ok)
		{
			_failures++;
		}
		Console.WriteLine($"{(ok ? "PASS" : "FAIL")}  {name}{(detail == null ? "" : "  " + detail)}");
	}
}
