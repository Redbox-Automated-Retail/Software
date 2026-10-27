using System.Net.Http;
using System.Threading.Tasks;
using Redbox.Core;
using Redbox.Rental.Model.UpdateClientService.Configuration;
using Redbox.Rental.Services.App;

namespace Redbox.KioskEngine.PreferenceManager.KioskEngineApiClient.Configuration;

public class KioskEngineApiConfigurationClient : KioskEngineApiBaseClient
{
	private static string _apiPath = "api/configuration";

	public static async Task<APIResponse<GetConfigurationChangesResponse>> GetConfigurationChanges()
	{
		IHttpService service = ServiceLocator.Instance.GetService<IHttpService>();
		HttpRequestMessage request = service.GenerateRequest(KioskEngineApiBaseClient.KioskEngineApiBaseUrl, _apiPath + "/settingvalues/changes", null, HttpMethod.Get);
		return await service.SendRequestAsync<GetConfigurationChangesResponse>(request, null, "GetConfigurationChanges", "c:\\build\\Redbox.KioskEngine.PreferenceManager\\KioskEngineApiClient\\Configuration\\KioskEngineApiConfigurationClient.cs");
	}
}
