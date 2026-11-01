using System.Threading.Tasks;
using Redbox.Core;
using Redbox.Rental.Model.Ads;

namespace Redbox.Rental.Services.Ads;

public class AdService : IAdService
{
	private IVistarAdService _vistarAdService;

	public AdService(IVistarAdService vistarAdService)
	{
		_vistarAdService = vistarAdService;
	}

	public IAd GetScreenAd(AdLocation adLocation)
	{
		LogHelper.Instance.Log("AdService.GetScreenAd - started");
		IAd ad = _vistarAdService?.GetScreenAd(adLocation);
		bool hasResult = ad != null;
		LogHelper.Instance.Log($"AdService.GetScreenAd - finished HasResult: {hasResult}");
		return ad;
	}

	public async Task<IAd> GetScreenAdAsync(AdLocation adLocation)
	{
		return await (_vistarAdService?.GetScreenAdAsync(adLocation)).ConfigureAwait(continueOnCapturedContext: false);
	}

	public bool RecordAdImpression(IAdImpression adImpression)
	{
		bool result = false;
		if (adImpression?.Ad is IVistarAd && _vistarAdService != null)
		{
			result = _vistarAdService.RecordAdImpression(adImpression);
		}
		else
		{
			LogHelper.Instance.Log($"Error in AdService.CreateImpression.  Invalid adImpression type: {adImpression?.Ad?.GetType()}");
		}
		return result;
	}

	public async Task<bool> RecordAdImpressionAsync(IAdImpression adImpression)
	{
		bool result = false;
		if (adImpression?.Ad is IVistarAd)
		{
			result = await (_vistarAdService?.RecordAdImpressionAsync(adImpression)).ConfigureAwait(continueOnCapturedContext: false);
		}
		else
		{
			LogHelper.Instance.Log($"Error in AdService.CreateImpression.  Invalid adImpression type: {adImpression?.Ad?.GetType()}");
		}
		return result;
	}
}
