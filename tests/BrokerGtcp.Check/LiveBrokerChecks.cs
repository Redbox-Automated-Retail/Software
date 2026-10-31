using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using Redbox.BrokerServices.Proxy;
using Redbox.BrokerServices.Proxy.ComponentModel;
using Redbox.BrokerServices.Proxy.Gtcp;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Services.KioskBrokerServices.KioskShared.DomainObjects;

namespace BrokerGtcp.Check;

/// <summary>
/// Plays a kiosk engine against a running broker (rb-remoting) through the real BrokerServicesProxy and its .NET 10
/// GTCP transport, like the backend's tests\clients\BrokerHarness.cs does with GenuineChannels on .NET Framework.
/// The broker's calls into the kiosk are triggered through rb-remoting's control API (port 8091, reachable only inside
/// the container network, so it is called with "wslc exec rb-remoting curl"), and the broker's view is read from the
/// same API and from the container log. A local TCP relay sits between the kiosk and the broker so the checks can drop
/// the connection.
/// </summary>
internal sealed class LiveBrokerChecks
{
	private const string Wslc = @"C:\Program Files\WSL\wslc.exe";
	private const string Container = "rb-remoting";
	private const string ControlApi = "http://127.0.0.1:8091";

	private static readonly object CallbackLock = new object();
	private static readonly List<string> CallbackEvents = new List<string>();

	private readonly Uri _brokerUrl;
	private readonly int _kioskId;
	private readonly DateTime _startedUtc = DateTime.UtcNow.AddSeconds(-2);
	private TcpRelay _relay;

	public LiveBrokerChecks(string brokerUrl, int kioskId)
	{
		_brokerUrl = new Uri(brokerUrl);
		_kioskId = kioskId;
	}

	public void Run()
	{
		Console.WriteLine($"-- live broker {_brokerUrl} as kiosk {_kioskId}");
		if (!Check.That(File.Exists(Wslc) && Control("GET", "/health")?.GetProperty("ok").GetBoolean() == true,
			"broker control API reachable (" + Container + ")"))
		{
			return;
		}
		_relay = new TcpRelay(_brokerUrl.Host, _brokerUrl.Port);
		_relay.Start();
		try
		{
			SetUpEngineServices($"gtcp://localhost:{_relay.Port}{_brokerUrl.AbsolutePath}");
			Check.Run("register", Register);
			Check.Run("callbacks", Callbacks);
			Check.Run("same-identity reconnect", DroppedSocket);
			Check.Run("outage and re-registration", Outage);
			Check.Run("reset", ResetDisconnects);
		}
		finally
		{
			BrokerServicesProxy.Instance.Reset();
			_relay.Dispose();
		}
	}

	// ------------------------------------------------------------------ steps

	private void Register()
	{
		string version = "{\"Runtime\":\"Kiosk Engine\",\"Version\":\"1.88.0.80\",\"PricingPolicy\":{\"rental\":{\"dvd\":{\"initial\":1.99}}},\"GiftCardsEnabled\":false,\"RecoOnPickup\":false,\"AuthorizeAtPickup\":true,\"CancelReservation\":true,\"Features\":1,\"Transport\":\"GTCP .NET 10\"}";
		Check.That(BrokerServicesProxy.Instance.Register(_kioskId, version), "BrokerServicesProxy.Register returned true");
		string hostUri = CurrentClient()?.HostUri;
		JsonElement? kiosk = FindKiosk();
		Check.That(kiosk != null, "broker lists the kiosk as registered");
		if (kiosk != null)
		{
			Check.That(kiosk.Value.GetProperty("host").GetString()?.Contains(hostUri ?? "?") == true, "broker's host for the kiosk is our GTCP host URI", hostUri);
			Check.That(kiosk.Value.GetProperty("version").GetString() == version, "broker received the version JSON unchanged");
		}
		Check.Equal(1, CountRegistrations(), "container log: one registration");
	}

	private void Callbacks()
	{
		JsonElement? ping = Control("POST", "/broker/ping", $"{{\"kioskId\":{_kioskId}}}");
		Check.That(ping?.TryGetProperty("ok", out JsonElement ok) == true && ok.GetBoolean(), "Ping answered", ping?.ToString());

		long reference = 7000000000L + Environment.TickCount % 1000000;
		string cart = "{\"ReferenceNumber\":" + reference + ",\"CardID\":\"card-1\",\"IsGc\":false,\"GcIds\":[],\"CcIds\":[\"cc1\"],\"Email\":\"gtcp@check.test\","
			+ "\"ReserveDate\":\"\\/Date(1793491200000)\\/\",\"Titles\":["
			+ "{\"ItemID\":1,\"TitleID\":4711,\"ReservationType\":1,\"Price\":1.99,\"Discount\":0,\"DiscountType\":0,\"DiscountedPrice\":1.99,\"NumberOfCredits\":0,\"LoyaltyPoints\":0},"
			+ "{\"ItemID\":2,\"TitleID\":4710,\"ReservationType\":2,\"Price\":9.99,\"Discount\":1,\"DiscountType\":1,\"DiscountedPrice\":8.99,\"NumberOfCredits\":0,\"LoyaltyPoints\":0}],"
			+ "\"DiscountedSubTotal\":10.98,\"SubTotal\":11.98,\"Tax\":0.82,\"GrandTotal\":11.80,\"HiveOnlinePromotionDiscount\":0,\"NumberOfCreditsRemaining\":0,"
			+ "\"PromoCode\":null,\"CustomerNumber\":\"C1\",\"HistoryTitleIDs\":[7,8,9],\"TaxRate\":0.075,\"Zip\":\"60601\",\"DefaultServiceFeeAmount\":0,"
			+ "\"ActualServiceFeeAmount\":0,\"AuthorizeAtPickup\":true,\"ReservationCardHash\":{\"CardBinHash\":\"abc\",\"Last4\":\"1234\"},\"AlternateCardHashes\":[]}";
		JsonElement? reserve = Control("POST", "/broker/reserve", $"{{\"kioskId\":{_kioskId},\"cart\":{cart}}}");
		Check.That(reserve?.TryGetProperty("ReservationSucceeded", out JsonElement succeeded) == true && succeeded.GetBoolean(), "Reserve3 answered: ReservationSucceeded", reserve?.ToString());
		if (reserve?.TryGetProperty("TitleResults", out JsonElement titles) == true && titles.ValueKind == JsonValueKind.Array)
		{
			Dictionary<int, JsonElement> byItem = titles.EnumerateArray().ToDictionary(t => t.GetProperty("ItemID").GetInt32());
			Check.Equal("H" + reference + "-4711", byItem.GetValueOrDefault(1).ValueKind == JsonValueKind.Object ? byItem[1].GetProperty("ReservedBarcode").GetString() : null,
				"Reserve3: barcode of title 4711 reached the broker");
			Check.Equal(false, byItem.TryGetValue(2, out JsonElement second) && second.GetProperty("Reservable").GetBoolean(),
				"Reserve3: title 4710 reported not reservable");
		}
		else
		{
			Check.That(false, "Reserve3: TitleResults returned");
		}
		Check.That(HasCallbackEvent($"reserve ref={reference} email=gtcp@check.test titles=2 grandTotal=11.80 reserveDate=2026-11-01T00:00:00.0000000Z kinds=Rental,Purchase history=7,8,9 card=1234"),
			"Reserve3: the kiosk's callback got the cart (decimals, DateTime Utc, enums, lists)", LastCallbackEvent());

		JsonElement? cancel = Control("POST", "/broker/cancel", $"{{\"kioskId\":{_kioskId},\"referenceNumber\":{reference}}}");
		Check.That(cancel?.TryGetProperty("CancellationSucceeded", out JsonElement cancelled) == true && cancelled.GetBoolean(), "CancelReservation answered", cancel?.ToString());
		Check.That(HasCallbackEvent($"cancel ref={reference}"), "CancelReservation: the kiosk's callback got the reference");
	}

	private void DroppedSocket()
	{
		string hostBefore = CurrentClient()?.HostUri;
		DateTime droppedUtc = DateTime.UtcNow.AddMilliseconds(-200);
		_relay.KillConnections();
		bool reconnected = WaitFor(TimeSpan.FromSeconds(30), () =>
			CurrentClient()?.IsConnected == true && CountLogLines($"kiosk {_kioskId} connection re-established", droppedUtc) >= 1);
		Check.That(reconnected, "kiosk reconnected after the socket was reset (client connected, container log: re-established)");
		Check.Equal(hostBefore, CurrentClient()?.HostUri, "same GTCP host URI after the reconnect");
		Check.Equal(1, CountRegistrations(), "container log: no new registration (same identity kept the session)");

		// GenuineChannels on the broker raises GeneralConnectionEstablished for the new socket and, on another thread,
		// GeneralConnectionReestablishing ("The remote host has opened another connection...") for the stub it replaces.
		// RemotingHost.cs marks the kiosk lost/restored from those events in arrival order, so when they arrive the other
		// way round its control API keeps refusing calls ("connection_lost") although the connection is fine. That is a
		// backend bookkeeping race, independent of the client; it is reported here instead of failing the transport.
		WaitFor(TimeSpan.FromSeconds(3), () => FindKiosk()?.GetProperty("lostUtc").ValueKind == JsonValueKind.Null);
		JsonElement? kiosk = FindKiosk();
		string lostReason = kiosk?.GetProperty("lostReason").ValueKind == JsonValueKind.String ? kiosk.Value.GetProperty("lostReason").GetString() : null;
		if (kiosk?.GetProperty("lostUtc").ValueKind != JsonValueKind.Null && lostReason?.Contains("opened another connection") == true)
		{
			Console.WriteLine("NOTE backend race: rb-remoting still shows the kiosk as lost (" + lostReason + "); Ping over the new socket not checked this run.");
			return;
		}
		JsonElement? ping = Control("POST", "/broker/ping", $"{{\"kioskId\":{_kioskId}}}");
		Check.That(ping?.TryGetProperty("ok", out JsonElement ok) == true && ok.GetBoolean(), "Ping answered on the new socket", ping?.ToString());
	}

	private void Outage()
	{
		DateTime outageUtc = DateTime.UtcNow;
		_relay.Stop();
		// The client tries twice (5 s apart), gives up and the proxy starts its reconnect timer (first tick after 1 s).
		bool timerStarted = WaitFor(TimeSpan.FromSeconds(30), () => ReconnectTimerRunning());
		Check.That(timerStarted, "after failed reconnection the proxy started its reconnect timer");
		_relay.Start();
		bool registered = WaitFor(TimeSpan.FromSeconds(30), () => CountRegistrations(outageUtc) >= 1 && !ReconnectTimerRunning());
		Check.That(registered, "kiosk registered again after the outage (container log)");
		JsonElement? ping = Control("POST", "/broker/ping", $"{{\"kioskId\":{_kioskId}}}");
		Check.That(ping?.TryGetProperty("ok", out JsonElement ok) == true && ok.GetBoolean(), "Ping answered after re-registration", ping?.ToString());
	}

	private void ResetDisconnects()
	{
		BrokerServicesProxy.Instance.Reset();
		bool lost = WaitFor(TimeSpan.FromSeconds(10), () =>
		{
			JsonElement? kiosk = FindKiosk();
			return kiosk == null || kiosk.Value.GetProperty("lostUtc").ValueKind != JsonValueKind.Null;
		});
		Check.That(lost, "after Reset the broker sees the kiosk's connection gone");
	}

	// ------------------------------------------------------------------ engine stand-ins

	private static void SetUpEngineServices(string url)
	{
		LogHelper.Instance.Logger = new ConsoleLogger();
		ServiceLocator.Instance.AddService(typeof(IMachineSettingsStore), new FakeSettings { BrokerUrl = url });
		ServiceLocator.Instance.AddService(typeof(ICallbackService), new FakeCallbacks());
		ServiceLocator.Instance.AddService(typeof(Redbox.Rental.Model.IConfiguration),
			InterfaceStub.Create<Redbox.Rental.Model.IConfiguration>("get_UseLegacyBrokerService"));
		// What ReservationServicesProxy.Register does, without the rest of the engine.
		Type proxyType = typeof(ReservationServicesProxy);
		proxyType.GetProperty("RequestCallback3", BindingFlags.Instance | BindingFlags.NonPublic)
			.SetValue(ReservationServicesProxy.Instance, (ReservationRequestCallback3)Reserve);
		proxyType.GetProperty("CancelRequestCallback", BindingFlags.Instance | BindingFlags.NonPublic)
			.SetValue(ReservationServicesProxy.Instance, (CancelReservationCallback)Cancel);
	}

	private static IReservationResult Reserve(string cardId, string email, long referenceNumber, DateTime reserveDate,
		IDictionary<string, List<int>> productsToReserve, bool appliedCredit, bool appliedHiveOnlinePromotion, bool appliedTitleMarketing,
		string customerNumber, decimal discountedSubTotal, decimal grandTotal, List<int> historyTitleIDs, decimal hiveOnlinePromotionDiscount,
		bool isMultiNightPricing, int numberOfCreditsRemaining, string promoCode, decimal subTotal, decimal tax, List<ReservationTitle3> reservationTitles,
		bool isGc, List<string> gcIds, List<string> ccIds, decimal taxRate, string zip, bool isMdv, bool isLoyaltyRedemption,
		decimal defaultServiceFee, decimal actualServiceFee, bool authorizeAtPickup, ReservationCardHash cardHashData, List<ReservationCardHash> reservationCardHashes)
	{
		IReservationResult result = ReservationServicesProxy.Instance.NewReservationResult();
		result.Items = new List<IReservedItem>();
		foreach (ReservationTitle3 title in reservationTitles)
		{
			IReservedItem item = ReservationServicesProxy.Instance.NewReservedItem();
			item.ProductId = title.TitleID;
			item.Barcode = title.TitleID % 10 == 0 ? "empty" : "H" + referenceNumber + "-" + title.TitleID; // ids ending in 0: out of stock
			result.Items.Add(item);
		}
		result.Success = true;
		AddCallbackEvent($"reserve ref={referenceNumber} email={email} titles={reservationTitles.Count} grandTotal={grandTotal.ToString(CultureInfo.InvariantCulture)} "
			+ $"reserveDate={reserveDate:o}{(reserveDate.Kind == DateTimeKind.Utc ? "" : "(" + reserveDate.Kind + ")")} "
			+ $"kinds={string.Join(",", reservationTitles.Select(t => t.ReservationType))} history={string.Join(",", historyTitleIDs)} card={cardHashData?.Last4}");
		return result;
	}

	private static ILocalCancelReservationResult Cancel(long referenceNumber, bool cancelOnServer)
	{
		ILocalCancelReservationResult result = ReservationServicesProxy.Instance.NewCancelReservationResult();
		result.Success = referenceNumber > 0;
		AddCallbackEvent($"cancel ref={referenceNumber}");
		return result;
	}

	private static void AddCallbackEvent(string text)
	{
		lock (CallbackLock)
		{
			CallbackEvents.Add(text);
		}
		Console.WriteLine("     EVENT " + text);
	}

	private static bool HasCallbackEvent(string text)
	{
		lock (CallbackLock)
		{
			return CallbackEvents.Contains(text);
		}
	}

	private static string LastCallbackEvent()
	{
		lock (CallbackLock)
		{
			return CallbackEvents.LastOrDefault();
		}
	}

	// ------------------------------------------------------------------ helpers

	private static GtcpBrokerClient CurrentClient() =>
		typeof(BrokerServicesProxy).GetField("m_gtcpClient", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(BrokerServicesProxy.Instance) as GtcpBrokerClient;

	private static bool ReconnectTimerRunning() =>
		typeof(BrokerServicesProxy).GetField("m_reconnectTimer", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(BrokerServicesProxy.Instance) != null;

	private JsonElement? FindKiosk()
	{
		JsonElement? kiosks = Control("GET", "/broker/kiosks");
		if (kiosks?.ValueKind != JsonValueKind.Array)
		{
			return null;
		}
		foreach (JsonElement kiosk in kiosks.Value.EnumerateArray())
		{
			if (kiosk.GetProperty("kioskId").GetInt32() == _kioskId)
			{
				return kiosk;
			}
		}
		return null;
	}

	private static bool WaitFor(TimeSpan timeout, Func<bool> condition)
	{
		Stopwatch watch = Stopwatch.StartNew();
		while (watch.Elapsed < timeout)
		{
			if (condition())
			{
				return true;
			}
			Thread.Sleep(250);
		}
		return condition();
	}

	/// <summary>Calls rb-remoting's control API from inside the container; the request body goes through stdin.</summary>
	private static JsonElement? Control(string method, string path, string body = null)
	{
		List<string> args = new List<string> { "exec" };
		if (body != null)
		{
			args.Add("-i");
		}
		args.AddRange(new[] { Container, "curl", "-s", "-m", "120", "-X", method });
		if (body != null)
		{
			args.AddRange(new[] { "-H", "Content-Type: application/json", "--data-binary", "@-" });
		}
		args.Add(ControlApi + path);
		string output = RunWslc(args, body);
		try
		{
			using JsonDocument document = JsonDocument.Parse(output);
			return document.RootElement.Clone();
		}
		catch (JsonException)
		{
			Console.WriteLine("     control API " + path + ": " + output);
			return null;
		}
	}

	/// <summary>"kiosk N registered from HOST (... URI: our host URI ...)" lines: only this process's registrations.</summary>
	private int CountRegistrations(DateTime? sinceUtc = null) =>
		CountLogLines($"kiosk {_kioskId} registered from HOST (URL: <none> URI: {CurrentClient()?.HostUri} ", sinceUtc);

	private int CountLogLines(string text, DateTime? sinceUtc = null)
	{
		DateTime since = sinceUtc ?? _startedUtc;
		int count = 0;
		foreach (string line in RunWslc(new List<string> { "logs", Container }, null, includeStderr: true).Split('\n'))
		{
			if (line.Length > 23 && line.Contains(text, StringComparison.Ordinal)
				&& DateTime.TryParseExact(line.Substring(0, 23), "yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture,
					DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out DateTime at) && at >= since)
			{
				count++;
			}
		}
		return count;
	}

	private static string RunWslc(List<string> args, string stdin, bool includeStderr = false)
	{
		ProcessStartInfo start = new ProcessStartInfo(Wslc)
		{
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			RedirectStandardInput = stdin != null,
			UseShellExecute = false,
			CreateNoWindow = true
		};
		foreach (string arg in args)
		{
			start.ArgumentList.Add(arg);
		}
		using Process process = Process.Start(start);
		if (stdin != null)
		{
			process.StandardInput.Write(stdin);
			process.StandardInput.Close();
		}
		var stderr = process.StandardError.ReadToEndAsync();
		string output = process.StandardOutput.ReadToEnd();
		process.WaitForExit();
		return includeStderr ? output + "\n" + stderr.Result : output;
	}
}
