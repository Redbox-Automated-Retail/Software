using System.Net.Http;
using System.Threading.Tasks;
using Redbox.Core;
using Redbox.Rental.Model.Health;
using Redbox.Rental.Services.App;

namespace Redbox.KioskEngine.PreferenceManager.KioskEngineApiClient.Health;

public class KioskEngineApiHealthClient : KioskEngineApiBaseClient
{
	private static string _apiPath = "api/health";

	public static async Task<APIResponse<HealthPingResponse>> SendPing()
	{
		IHttpService service = ServiceLocator.Instance.GetService<IHttpService>();
		HttpRequestMessage request = service.GenerateRequest(KioskEngineApiBaseClient.KioskEngineApiBaseUrl, _apiPath + "/ping", null, HttpMethod.Post);
		return await service.SendRequestAsync<HealthPingResponse>(request, null, "SendPing", "c:\\build\\Redbox.KioskEngine.PreferenceManager\\KioskEngineApiClient\\Health\\KioskEngineApiHealthClient.cs");
	}

	public static async Task<APIResponse<HealthPingResponse>> SendAlert()
	{
		IHttpService service = ServiceLocator.Instance.GetService<IHttpService>();
		HttpRequestMessage request = service.GenerateRequest(KioskEngineApiBaseClient.KioskEngineApiBaseUrl, _apiPath + "/alert", null, HttpMethod.Post);
		return await service.SendRequestAsync<HealthPingResponse>(request, null, "SendAlert", "c:\\build\\Redbox.KioskEngine.PreferenceManager\\KioskEngineApiClient\\Health\\KioskEngineApiHealthClient.cs");
	}
}
