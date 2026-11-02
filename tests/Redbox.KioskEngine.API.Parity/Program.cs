using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using Redbox.Core;
using Redbox.KioskEngine.API;
using Redbox.Rental.Model;

namespace Redbox.KioskEngine.API.Parity;

// Usage: Redbox.KioskEngine.API.Parity.exe <port>
internal static class Program
{
	private static int Main(string[] args)
	{
		string baseUrl = "http://localhost:" + (args.Length > 0 ? args[0] : "19002");
		// Only the configuration (for the URL); every controller then takes its "service not found" path, which
		// exercises routing, binding, status codes and the JSON of both response styles.
		ServiceLocator.Instance.AddService(typeof(IConfiguration), Stub<IConfiguration>.Create(m => m.Name == "get_KioskEngineApiServiceBaseUrl" ? baseUrl : null));
#if NETFRAMEWORK
		// Web API in memory: the same routes, formatters and controllers as APIService's self-host, without the
		// HTTP.sys listener (which needs a URL reservation or admin rights).
		System.Web.Http.HttpConfiguration config = new System.Web.Http.HttpConfiguration();
		System.Web.Http.HttpConfigurationExtensions.MapHttpAttributeRoutes(config);
		config.EnsureInitialized();
		using HttpClient client = new HttpClient(new System.Web.Http.HttpServer(config)) { BaseAddress = new Uri(baseUrl) };
		GC.KeepAlive(typeof(APIService));
#else
		new APIService().Initialize();
		using HttpClient client = new HttpClient { BaseAddress = new Uri(baseUrl) };
		WaitUntilListening(client);
#endif
		Request(client, "GET", "api/engine/processid");
		Request(client, "POST", "api/engine/shutdown");
		Request(client, "POST", "api/health/ping");
		Request(client, "GET", "api/kioskhealth/getpingserviceinfo");
		Request(client, "GET", "api/enginecore/getstoreinfo");
		Request(client, "GET", "api/enginecore/machinesettingvalue?settingPath=Store&settingName=ID");
		Request(client, "GET", "api/enginecore/applicationstatevalue?settingName=x");
		Request(client, "GET", "api/configuration/settingvalues/changes");
		Request(client, "GET", "api/profiledata/kioskproductids");
		Request(client, "GET", "api/reservation/getreservations");
		Request(client, "GET", "api/reservation/getreservationservicestatus");
		Request(client, "POST", "api/reservation", "{\"ReferenceNumber\":\"123\"}");
		Request(client, "POST", "api/reservation/cancelreservation?referenceNumber=5");
		Request(client, "GET", "api/shoppingcart/loyaltytestshoppingcartitems");
		return 0;
	}

	private static void WaitUntilListening(HttpClient client)
	{
		for (int i = 0; i < 100; i++)
		{
			try
			{
				client.GetAsync("api/engine/processid").Result.Dispose();
				return;
			}
			catch (AggregateException)
			{
				Thread.Sleep(200);
			}
		}
	}

	private static void Request(HttpClient client, string method, string path, string? body = null)
	{
		HttpRequestMessage request = new HttpRequestMessage(new HttpMethod(method), path);
		if (body != null)
		{
			request.Content = new StringContent(body, Encoding.UTF8, "application/json");
		}
		using HttpResponseMessage response = client.SendAsync(request).Result;
		string content = response.Content.ReadAsStringAsync().Result;
		// The responses carry a measured response time; blank it so runs compare.
		content = System.Text.RegularExpressions.Regex.Replace(content, "\"ResponseTime\":\\d+", "\"ResponseTime\":0");
		Console.WriteLine($"### {method} {path}");
		Console.WriteLine($"{(int)response.StatusCode} {response.Content.Headers.ContentType}");
		Console.WriteLine(content);
	}
}

// Interface stub: every member returns the answer function's value (or default).
public class Stub<T> : DispatchProxy
{
	private Func<MethodInfo, object?> m_answer = null!; // set by Create

	public static T Create(Func<MethodInfo, object?> answer)
	{
		T proxy = Create<T, Stub<T>>();
		((Stub<T>)(object)proxy!).m_answer = answer;
		return proxy;
	}

	protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
	{
		object? value = m_answer(targetMethod!); // always passed by DispatchProxy
		if (value == null && targetMethod!.ReturnType.IsValueType && targetMethod.ReturnType != typeof(void))
		{
			return Activator.CreateInstance(targetMethod.ReturnType);
		}
		return value;
	}
}
