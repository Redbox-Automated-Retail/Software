using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using KioskClientService.API.App;
using KioskClientService.API.Services.Configuration;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API.Services.Analytics.KioskDataService;

public class KioskDataServiceClient : IKioskDataServiceClient
{
	private readonly IHttpService _httpService;

	private readonly ILogger<KioskDataServiceClient> _logger;

	private readonly IKioskConfiguration _kioskConfiguration;

	private readonly List<Header> _requestHeaders;

	public KioskDataServiceClient(IStoreService store, IHttpService httpService, ILogger<KioskDataServiceClient> logger, IOptionsSnapshotKioskConfiguration optionsSnapshotKioskConfiguration)
	{
		_httpService = httpService;
		_logger = logger;
		_kioskConfiguration = optionsSnapshotKioskConfiguration;
		if (_kioskConfiguration.KioskClientDefault.KioskDataServiceTimeout != 0)
		{
			_httpService.Timeout = _kioskConfiguration.KioskClientDefault.KioskDataServiceTimeout;
		}
		KioskCloudClientConfiguration kioskCloudClientConfiguration = new KioskCloudClientConfiguration
		{
			APIKey = _kioskConfiguration.KioskClientDefault.KioskDataServiceAPIKey,
			AuthHeader = _kioskConfiguration.KioskClientDefault.KioskDataServiceAuthHeader
		};
		_requestHeaders = kioskCloudClientConfiguration.GetKioskCloudHeaders(null, store?.KioskId);
	}

	public async Task<APIResponse<AnalyticsResponse>> PostKioskClientEvent(KioskClientEventRequest request)
	{
		return await _httpService.SendRequest<AnalyticsResponse>(_kioskConfiguration.KioskClientDefault.KioskDataServiceUrl, "api/KioskData/KioskClientServiceEvent", request, HttpMethod.Post, _requestHeaders);
	}
}
