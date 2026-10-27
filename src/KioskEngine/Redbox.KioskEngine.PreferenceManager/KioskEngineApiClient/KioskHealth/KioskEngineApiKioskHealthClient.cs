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
		IHttpService service = ServiceLocator.Instance.GetService<IHttpService>();
		HttpRequestMessage request = service.GenerateRequest(KioskEngineApiBaseClient.KioskEngineApiBaseUrl, _apiPath + "/ping", null, HttpMethod.Post);
		return await service.SendRequestAsync<PingResponse>(request, null, "SendPing", "c:\\build\\Redbox.KioskEngine.PreferenceManager\\KioskEngineApiClient\\KioskHealth\\KioskEngineApiKioskHealthClient.cs");
	}

	public static async Task<APIResponse<PingServiceInfo>> GetPingServerInfo()
	{
		IHttpService service = ServiceLocator.Instance.GetService<IHttpService>();
		HttpRequestMessage request = service.GenerateRequest(KioskEngineApiBaseClient.KioskEngineApiBaseUrl, _apiPath + "/getpingserviceinfo", null, HttpMethod.Get);
		return await service.SendRequestAsync<PingServiceInfo>(request, null, "GetPingServerInfo", "c:\\build\\Redbox.KioskEngine.PreferenceManager\\KioskEngineApiClient\\KioskHealth\\KioskEngineApiKioskHealthClient.cs");
	}
}
