using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using KioskClientService.API.Ads;
using KioskClientService.API.App;
using KioskClientService.API.Services.Configuration;

namespace KioskClientService.API.Services.Ads.AdServer;

public class AdServerClient : IAdServerClient
{
	private readonly IHttpService _httpService;

	private readonly IOptionsSnapshotKioskConfiguration _kioskConfiguration;

	private readonly List<Header> _requestHeaders;

	public AdServerClient(IHttpService httpService, IStoreService store, IOptionsSnapshotKioskConfiguration optionsSnapshotKioskConfiguration)
	{
		_httpService = httpService;
		_kioskConfiguration = optionsSnapshotKioskConfiguration;
		KioskCloudClientConfiguration kioskCloudClientConfiguration = new KioskCloudClientConfiguration
		{
			APIKey = _kioskConfiguration.KioskClientAds.AdServerAPIKey,
			AuthHeader = _kioskConfiguration.KioskClientAds.AdServerAuthHeader
		};
		_requestHeaders = kioskCloudClientConfiguration.GetKioskCloudHeaders(null, store?.KioskId);
	}

	public async Task<APIResponse<AdResponse>> GetAd(AdLocation location)
	{
		return await _httpService.SendRequest<AdResponse>(_kioskConfiguration.KioskClientAds.AdServerUrl, $"api/ads/{location}", null, HttpMethod.Get, _requestHeaders);
	}

	public async Task<APIResponse<AdsResponse>> GetAds(List<AdLocation> locations)
	{
		string text = locations.ToQueryString("locations");
		return await _httpService.SendRequest<AdsResponse>(_kioskConfiguration.KioskClientAds.AdServerUrl, "api/ads" + text, null, HttpMethod.Get, _requestHeaders);
	}

	public async Task<APIResponse<AdsResponse>> CacheAds(List<AdLocation> locations)
	{
		string text = locations.ToQueryString("locations");
		return await _httpService.SendRequest<AdsResponse>(_kioskConfiguration.KioskClientAds.AdServerUrl, "api/ads/cache" + text, null, HttpMethod.Get, _requestHeaders);
	}

	public async Task<APIResponse<dynamic>> PostProofOfPlay(ProofOfPlayRequest proofOfPlay)
	{
		return await _httpService.SendRequest<object>(_kioskConfiguration.KioskClientAds.AdServerUrl, "api/ads/proofOfPlay", proofOfPlay, HttpMethod.Post, _requestHeaders);
	}

	public async Task<APIResponse<dynamic>> PostExpire(string id)
	{
		return await _httpService.SendRequest<object>(_kioskConfiguration.KioskClientAds.AdServerUrl, "api/ads/" + id + "/expire", null, HttpMethod.Post, _requestHeaders);
	}
}
