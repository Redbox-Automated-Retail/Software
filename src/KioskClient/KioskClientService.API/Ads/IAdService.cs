using System.Collections.Generic;
using System.Threading.Tasks;
using KioskClientService.API.App;
using KioskClientService.API.Services.Ads;

namespace KioskClientService.API.Ads;

public interface IAdService
{
	Task<ApplicationResponse> StartApplication();

	Task<ApplicationResponse> EndApplication();

	SessionResponse StartSession();

	SessionResponse EndSession();

	Task<GetAdResponse> GetAd(GetAdRequest request);

	Task<GetAdsResponse> GetAds(List<AdLocation> locations, bool isPreload = false);

	Task<GetAdsResponse> GetAds(string viewName, bool isPreload = false);

	Task<ProofOfPlayResponse> PostProofOfPlay(string advertisementId, ProofOfPlayRequest proofOfPlayRequest);

	Task<ExpireResponse> Expire(string advertisementId);

	Task<CacheResponse> Cache();
}
