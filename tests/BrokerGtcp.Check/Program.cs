using System;
using System.Globalization;

namespace BrokerGtcp.Check;

/// <summary>
/// BrokerGtcp.Check [--offline] [--url gtcp://localhost:7890/RegistrationBroker/Default.rem] [--kiosk 990012]
/// Offline checks always run; the live checks need the rb-remoting container (see LiveBrokerChecks).
/// Exit code: number of failed checks.
/// </summary>
internal static class Program
{
	private static int Main(string[] args)
	{
		bool offline = false;
		string url = "gtcp://localhost:7890/RegistrationBroker/Default.rem";
		int kioskId = 990012;
		for (int i = 0; i < args.Length; i++)
		{
			switch (args[i])
			{
				case "--offline":
					offline = true;
					break;
				case "--url":
					url = args[++i];
					break;
				case "--kiosk":
					kioskId = int.Parse(args[++i], CultureInfo.InvariantCulture);
					break;
				default:
					Console.Error.WriteLine("usage: BrokerGtcp.Check [--offline] [--url gtcp://host:port/RegistrationBroker/Default.rem] [--kiosk id]");
					return 2;
			}
		}
		CaptureChecks.Run();
		if (!offline)
		{
			new LiveBrokerChecks(url, kioskId).Run();
		}
		Console.WriteLine($"== {Check.Passed} passed, {Check.Failed} failed");
		return Check.Failed;
	}
}
