using System.Net.Http;
using System.Threading.Tasks;
using KioskClientService.API.App;
using KioskClientService.API.Services.ProfileData;

namespace KioskClientService.API.Services.UpdateClientServiceApi;

public class UpdateClientServiceClient : IUpdateClientServiceClient
{
	private readonly IHttpService _httpService;

	private readonly AppSettings _appSettings;

	public UpdateClientServiceClient(IHttpService httpService, IApplicationSettings applicationSettings)
	{
		_httpService = httpService;
		_appSettings = applicationSettings.AppSettings;
	}

	public async Task<APIResponse<GetRecordChangesResponse>> GetRecordChanges<T>(DataUpdateRequest request)
	{
		return await _httpService.SendRequest<GetRecordChangesResponse>(_appSettings.UpdateClientServiceUrl, "api/dataupdate/changes", request, HttpMethod.Post);
	}
}
