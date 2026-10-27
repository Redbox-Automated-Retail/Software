using System;
using System.Net.Http;
using System.Threading.Tasks;
using KioskClientService.API.App;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API.Services.KioskEngineApi;

public class KioskEngineApiProfileDataClient : KioskEngineApiBaseClient, IKioskEngineApiProfileDataClient
{
	private readonly IHttpService _httpService;

	private ILogger<KioskEngineApiProfileDataClient> _logger;

	private static string _apiPath = "api/profiledata";

	public KioskEngineApiProfileDataClient(ILogger<KioskEngineApiProfileDataClient> logger, IHttpService httpService)
	{
		_httpService = httpService;
		_logger = logger;
	}

	public async Task<APIResponse<GetKioskProductIdsResponse>> GetKioskProductIds()
	{
		try
		{
			return await _httpService.SendRequest<GetKioskProductIdsResponse>(KioskEngineApiBaseClient.KioskEngineApiBaseUrl, _apiPath + "/kioskproductids", null, HttpMethod.Get, null, logResponseContent: false);
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception while getting kiosk product ids", "GetKioskProductIds");
			return null;
		}
	}
}
