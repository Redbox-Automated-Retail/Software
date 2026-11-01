using System.Net.Http;
using System.Threading.Tasks;
using Redbox.Core;
using Redbox.Rental.Model.UpdateClientService.KioskHealth;
using Redbox.Rental.Services.App;

namespace Redbox.KioskEngine.PreferenceManager.KioskEngineApiClient.KioskHealth;

public class KioskEngineApiKioskHealthClient : KioskEngineApiBaseClient
{
	private static string _apiPath = "api/kioskhealth";

	public static async Task<APIResponse<PingResponse>> SendPing()
	{
		IHttpService httpService = ServiceLocator.Instance.GetService<IHttpService>();
		HttpRequestMessage request = httpService.GenerateRequest(KioskEngineApiBaseClient.KioskEngineApiBaseUrl, _apiPath + "/ping", null, HttpMethod.Post);
		return await httpService.SendRequestAsync<PingResponse>(request, null, "SendPing", "c:\\build\\Redbox.KioskEngine.PreferenceManager\\KioskEngineApiClient\\KioskHealth\\KioskEngineApiKioskHealthClient.cs");
	}

	public static async Task<APIResponse<PingServiceInfo>> GetPingServerInfo()
	{
		IHttpService httpService = ServiceLocator.Instance.GetService<IHttpService>();
		HttpRequestMessage request = httpService.GenerateRequest(KioskEngineApiBaseClient.KioskEngineApiBaseUrl, _apiPath + "/getpingserviceinfo", null, HttpMethod.Get);
		return await httpService.SendRequestAsync<PingServiceInfo>(request, null, "GetPingServerInfo", "c:\\build\\Redbox.KioskEngine.PreferenceManager\\KioskEngineApiClient\\KioskHealth\\KioskEngineApiKioskHealthClient.cs");
	}
}
