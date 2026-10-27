using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using KioskClientService.API.App;
using KioskClientService.API.Services.Analytics.Vistar.Summaries;
using KioskClientService.API.Services.Configuration;

namespace KioskClientService.API.Services.Analytics.Vistar;

public class AdServerAnalyticsClient : IAdServerAnalyticsClient
{
	private readonly IHttpService _httpService;

	private readonly IOptionsSnapshotKioskConfiguration _kioskConfiguration;

	private readonly List<Header> _requestHeaders = new List<Header>();

	public AdServerAnalyticsClient(IStoreService store, IHttpService httpService, IOptionsSnapshotKioskConfiguration optionsSnapshotKioskConfiguration)
	{
		_httpService = httpService;
		_kioskConfiguration = optionsSnapshotKioskConfiguration;
		if (_kioskConfiguration.KioskClientAds.AdServerTimeout != 0)
		{
			_httpService.Timeout = _kioskConfiguration.KioskClientAds.AdServerTimeout;
		}
		KioskCloudClientConfiguration kioskCloudClientConfiguration = new KioskCloudClientConfiguration
		{
			APIKey = _kioskConfiguration.KioskClientAds.AdServerAPIKey,
			AuthHeader = _kioskConfiguration.KioskClientAds.AdServerAuthHeader
		};
		_requestHeaders = kioskCloudClientConfiguration.GetKioskCloudHeaders(null, store?.KioskId);
	}

	public async Task<APIResponse<AnalyticsResponse>> PostGetAdAnalytics(VistarGetAdSummary request)
	{
		return await _httpService.SendRequest<AnalyticsResponse>(_kioskConfiguration.KioskClientAds.AdServerUrl, "api/analytics/get_ad", request, HttpMethod.Post, _requestHeaders);
	}

	public async Task<APIResponse<AnalyticsResponse>> PostProofOfPlayAnalytics(VistarProofOfPlaySummary request)
	{
		return await _httpService.SendRequest<AnalyticsResponse>(_kioskConfiguration.KioskClientAds.AdServerUrl, "api/analytics/proof_of_play", request, HttpMethod.Post, _requestHeaders);
	}

	public async Task<APIResponse<AnalyticsResponse>> PostExpireAnalytics(VistarExpireSummary request)
	{
		return await _httpService.SendRequest<AnalyticsResponse>(_kioskConfiguration.KioskClientAds.AdServerUrl, "api/analytics/expire", request, HttpMethod.Post, _requestHeaders);
	}

	public async Task<APIResponse<AnalyticsResponse>> PostCacheAnalytics(VistarCacheSummary request)
	{
		return await _httpService.SendRequest<AnalyticsResponse>(_kioskConfiguration.KioskClientAds.AdServerUrl, "api/analytics/cache", request, HttpMethod.Post, _requestHeaders);
	}

	public async Task<APIResponse<AnalyticsResponse>> PostGetAssetAnalytics(VistarGetAssetSummary request)
	{
		return await _httpService.SendRequest<AnalyticsResponse>(_kioskConfiguration.KioskClientAds.AdServerUrl, "api/analytics/get_asset", request, HttpMethod.Post, _requestHeaders);
	}

	public async Task<APIResponse<AnalyticsResponse>> PostDownloadAssetAnalytics(VistarDownloadAssetSummary request)
	{
		return await _httpService.SendRequest<AnalyticsResponse>(_kioskConfiguration.KioskClientAds.AdServerUrl, "api/analytics/download_asset", request, HttpMethod.Post, _requestHeaders);
	}

	public async Task<APIResponse<AnalyticsResponse>> PostGetAdTimeoutEventAnalytics(GetAdTimeoutEventSummary request)
	{
		return await _httpService.SendRequest<AnalyticsResponse>(_kioskConfiguration.KioskClientAds.AdServerUrl, "api/analytics/get_ad_timeout_event", request, HttpMethod.Post, _requestHeaders);
	}
}
