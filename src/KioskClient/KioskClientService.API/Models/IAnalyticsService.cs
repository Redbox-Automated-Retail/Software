using System;
using System.Threading.Tasks;
using KioskClientService.API.Ads;
using KioskClientService.API.App;
using KioskClientService.API.Services.Ads;
using KioskClientService.API.Services.Analytics;
using KioskClientService.API.Services.Analytics.KioskDataService;
using KioskClientService.API.Services.Analytics.Vistar.Request;
using KioskClientService.API.Vistar;

namespace KioskClientService.API.Models;

public interface IAnalyticsService
{
	Task ServiceEvent(KioskClientEvent eventType, DateTime? eventTime = null);

	Task VistarGetAd(AdLocation location, APIResponse<VistarGetAdResponse> response);

	Task VistarProofOfPlay(ProofOfPlayRequest request, ProofOfPlayResponse response, CachedAdvertisement ad = null);

	Task VistarExpire(VistarExpirationRequest request, ExpireResponse response, CachedAdvertisement ad, AdExpirationSource adExpirationSource);

	Task VistarCache(GetAssetsRequest request, CacheResponse result);

	Task DownloadAsset(APIResponse<byte[]> response);

	Task GetAdTimeoutEvent(GetAdTimeoutEvent timeoutEvent);

	void PurgeOldAnalytics(TimeSpan age);

	void PurgeServiceEvents();

	void SaveServiceEvent(ServiceEvent serviceEvent);

	ServiceEvent LoadServiceEvent(KioskClientEvent eventType);
}
