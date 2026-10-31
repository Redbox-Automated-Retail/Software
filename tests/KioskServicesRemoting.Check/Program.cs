using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web;
using Redbox.Remoting.Nrbf;

namespace KioskServicesRemoting.Check;

internal static class Program
{
	// The interface KioskServicesProxy calls (Redbox.KioskServices.ComponentModel 2.6.0.11, as on 1.8x kiosks).
	private const string MessageServiceType = "Redbox.KioskServices.ComponentModel.IMessageService, Redbox.KioskServices.ComponentModel, Version=2.6.0.11, Culture=neutral, PublicKeyToken=null";

	private static int _failures;

	private static int Main(string[] args)
	{
		string url = args.Length > 0 ? args[0] : "http://127.0.0.1/KioskServices/MessageService.rem";
		Compare(url, "GetPendingStates", "{\"MessageType\":\"GetPendingStates\",\"KioskId\":990013,\"MessageId\":\"6f1c2d3e-0000-4000-8000-000000000001\"}");
		Compare(url, "KioskPing", "{\"MessageType\":\"KioskPing\",\"KioskId\":990013,\"EngineVersion\":\"1.88.0.80\",\"BundleVersion\":\"check\",\"MessageId\":\"6f1c2d3e-0000-4000-8000-000000000002\"}");
		Compare(url, "unknown type", "{\"MessageType\":\"NoSuchMessage\",\"KioskId\":990013}");
		Compare(url, "non-ASCII", "{\"MessageType\":\"NoSuchMessage\",\"KioskId\":990013,\"Name\":\"Jos\u00e9 \u00fc \u6f22\"}");
		Console.WriteLine(_failures == 0 ? "ALL PASSED" : $"{_failures} FAILED");
		return _failures;
	}

	private static void Compare(string url, string name, string json)
	{
		try
		{
			string remoting = (string)RemotingHttpClient.Invoke(url, MessageServiceType, "Process", new object[] { json }, TimeSpan.FromSeconds(30));
			string rest = Rest(url, json);
			bool same = Normalize(remoting) == Normalize(rest);
			Report(same, $"{name}: Remoting and REST answers {(same ? "match" : "differ")}", same ? Short(remoting) : $"remoting={Short(remoting)} rest={Short(rest)}");
		}
		catch (Exception e)
		{
			Report(false, name, e.Message);
		}
	}

	// KioskServicesProxy's REST mode: payload=<url-encoded JSON>, answer = URL-encoded JSON.
	private static string Rest(string url, string json)
	{
		using HttpClient client = new HttpClient();
		using FormUrlEncodedContent content = new FormUrlEncodedContent(new Dictionary<string, string> { ["payload"] = json });
		using HttpResponseMessage response = client.PostAsync(url, content).GetAwaiter().GetResult();
		return HttpUtility.UrlDecode(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());
	}

	// Answers carry a server time (a Redbox.Core DateTime object) or a duration; blank them so the two transports compare.
	private static string Normalize(string s) => Regex.Replace(s ?? "", "\\\\?/Date\\(\\d+\\)\\\\?/|\"ServerTime\":\\s*\\{[^}]*\\}|\"(Duration|ResponseTime|ServerTime)\":\\s*[^,}]+", "#");

	private static string Short(string s) => s == null ? "null" : s.Length > 140 ? s.Substring(0, 140) + "..." : s;

	private static void Report(bool ok, string name, string detail)
	{
		if (!ok)
		{
			_failures++;
		}
		Console.WriteLine($"{(ok ? "PASS" : "FAIL")}  {name}  {detail}");
	}
}
