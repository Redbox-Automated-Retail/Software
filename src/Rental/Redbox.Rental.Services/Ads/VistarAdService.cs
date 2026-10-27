using System.IO;
using System.Threading.Tasks;
using Redbox.Core;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Ads;
using Redbox.Rental.Model.KioskClientService;
using Redbox.Rental.Model.KioskClientService.Ads;
using Redbox.Rental.Services.Configuration;

namespace Redbox.Rental.Services.Ads;

public class VistarAdService : IVistarAdService, IAdService
{
	public IAd GetScreenAd(AdLocation adLocation)
	{
		IVistarAd vistarAd = null;
		IConfiguration instance = Redbox.Rental.Services.Configuration.Configuration.Instance;
		bool flag = true;
		if (instance.Current.MarketingAd.EnableVistarAds && flag)
		{
			IGetAdResponse getAdResponse = ServiceLocator.Instance.GetService<IKioskClientService>()?.Ads?.GetAd(adLocation);
			if (getAdResponse != null && getAdResponse.Success && getAdResponse.AdvertisementId != null && getAdResponse.AssetPath != null)
			{
				vistarAd = new VistarAd
				{
					AdvertisementId = getAdResponse?.AdvertisementId,
					ImageName = getAdResponse?.AssetPath,
					Height = (getAdResponse?.Height ?? 0),
					Width = (getAdResponse?.Width ?? 0),
					MinimumDisplay = 1000
				};
			}
		}
		if (flag)
		{
			LogHelper.Instance.Log($"VistarAdService.GetScreenAd: Location: {adLocation}, Advertisement Id: {vistarAd?.AdvertisementId}, Image Name: {Path.GetFileName(vistarAd?.ImageName)}");
		}
		else
		{
			LogHelper.Instance.Log($"VistarAdService.GetScreenAd location {adLocation} not supported.");
		}
		return vistarAd;
	}

	public async Task<IAd> GetScreenAdAsync(AdLocation adLocation)
	{
		IVistarAd ad = null;
		IConfiguration instance = Redbox.Rental.Services.Configuration.Configuration.Instance;
		bool locationSupported = adLocation != AdLocation.Welcome;
		if (instance.Current.MarketingAd.EnableVistarAds && locationSupported)
		{
			IGetAdResponse getAdResponse = await (ServiceLocator.Instance.GetService<IKioskClientService>()?.Ads?.GetAdAsync(adLocation)).ConfigureAwait(continueOnCapturedContext: false);
			if (getAdResponse != null && getAdResponse.Success && getAdResponse.AdvertisementId != null && getAdResponse.AssetPath != null)
			{
				ad = new VistarAd
				{
					AdvertisementId = getAdResponse?.AdvertisementId,
					ImageName = getAdResponse?.AssetPath,
					Height = (getAdResponse?.Height ?? 0),
					Width = (getAdResponse?.Width ?? 0),
					MinimumDisplay = 1000
				};
			}
		}
		if (locationSupported)
		{
			LogHelper.Instance.Log($"VistarAdService.GetScreenAd: Location: {adLocation}, Advertisement Id: {ad?.AdvertisementId}, Image Name: {Path.GetFileName(ad?.ImageName)}");
		}
		else
		{
			LogHelper.Instance.Log($"VistarAdService.GetScreenAd location {adLocation} not supported.");
		}
		return ad;
	}

	public bool RecordAdImpression(IAdImpression adImpression)
	{
		bool flag = false;
		IVistarAd vistarAd = adImpression?.Ad as IVistarAd;
		if (vistarAd != null && Redbox.Rental.Services.Configuration.Configuration.Instance.Current.MarketingAd.EnableVistarAds)
		{
			flag = ServiceLocator.Instance.GetService<IKioskClientService>()?.Ads?.SendProofOfPlay(adImpression) == true;
		}
		LogHelper.Instance.Log($"VistarAdService.CreateImpression: Advertisement Id: {vistarAd?.AdvertisementId}, Location: {adImpression?.AdLocation}, Result: {flag}");
		return flag;
	}

	public async Task<bool> RecordAdImpressionAsync(IAdImpression adImpression)
	{
		bool flag = false;
		IVistarAd vistarAd = adImpression?.Ad as IVistarAd;
		if (vistarAd != null && Redbox.Rental.Services.Configuration.Configuration.Instance.Current.MarketingAd.EnableVistarAds)
		{
			flag = await (ServiceLocator.Instance.GetService<IKioskClientService>()?.Ads?.SendProofOfPlayAsync(adImpression)).ConfigureAwait(continueOnCapturedContext: false);
		}
		LogHelper.Instance.Log($"VistarAdService.CreateImpression: Advertisement Id: {vistarAd?.AdvertisementId}, Location: {adImpression?.AdLocation}, Result: {flag}");
		return flag;
	}
}
