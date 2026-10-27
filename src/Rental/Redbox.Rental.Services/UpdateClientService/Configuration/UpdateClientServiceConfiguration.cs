using System.Net.Http;
using System.Threading.Tasks;
using Redbox.Core;
using Redbox.Rental.Model.UpdateClientService.Configuration;
using Redbox.Rental.Services.App;

namespace Redbox.Rental.Services.UpdateClientService.Configuration;

public class UpdateClientServiceConfiguration : BaseUpdateClientService, IUpdateClientServiceConfiguration
{
	public const string _apiPath = "api/configuration";

	public async Task<GetConfigurationChangesResponse> GetConfigurationChanges()
	{
		LogHelper.Instance.Log("UpdateClientServiceConfiguration.GetConfigurationChanges");
		IHttpService service = ServiceLocator.Instance.GetService<IHttpService>();
		HttpRequestMessage request = service.GenerateRequest(base.UpdateClientServiceBaseUrl, "api/configuration/settingvalues/changes", null, HttpMethod.Get);
		APIResponse<HttpResponseMessage> aPIResponse = await service.SendRequestAsync<HttpResponseMessage>(request, 20000, "GetConfigurationChanges", "c:\\build\\Redbox.Rental.Services\\UpdateClientService\\Configuration\\UpdateClientServiceConfiguration.cs");
		return new GetConfigurationChangesResponse
		{
			StatusCode = aPIResponse.StatusCode,
			Success = (aPIResponse?.IsSuccessStatusCode ?? false)
		};
	}
}
