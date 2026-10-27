using System;
using System.Net;
using System.Threading.Tasks;
using Redbox.Core;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Ads;
using Redbox.Rental.Model.KioskClientService.Ads;
using Redbox.Rental.Services.Configuration;

namespace Redbox.Rental.Services.KioskClientService.Ads;

public class KioskClientServiceAds : BaseKioskClientService, IKioskClientServiceAds
{
	private const string _apiPath = "api/ads";

	private const string _analyticsPath = "api/analytics";

	private const int _defaultTimeout = 500;

	private const int _longTimeout = 2000;

	public IGetAdResponse GetAd(AdLocation adLocation)
	{
		int timeoutSetting = Redbox.Rental.Services.Configuration.Configuration.Instance.Current.MarketingAd.VistarGetAdTimeout;
		WebResult<GetAdResponse> webResult = ExecuteWebRequest<GetAdResponse>(new Uri(string.Format("{0}/{1}?location={2}", base.KioskClientServiceBaseUrl, "api/ads", adLocation)), null, "GET", timeoutSetting);
		LogResponse($"KioskClientServiceAds.GetAd for ad location: {adLocation}", webResult);
		if (webResult.ExceptionStatus == WebExceptionStatus.Timeout)
		{
			Task.Run(() => PostGetAdTimeoutEvent(adLocation, timeoutSetting));
		}
		return webResult?.Response;
	}

	public async Task<IGetAdResponse> GetAdAsync(AdLocation adLocation)
	{
		WebResult<GetAdResponse> webResult = await ExecuteAsyncWebRequest<GetAdResponse>(new Uri(string.Format("{0}/{1}?location={2}", base.KioskClientServiceBaseUrl, "api/ads", adLocation)), null, "GET", Redbox.Rental.Services.Configuration.Configuration.Instance.Current.MarketingAd.VistarGetAdTimeout).ConfigureAwait(continueOnCapturedContext: false);
		LogResponse($"KioskClientServiceAds.GetAd for ad locataion: {adLocation}", webResult);
		return webResult?.Response;
	}

	public async Task<bool> ExpireAd(string advertisementId)
	{
		WebResult<BaseResponse> webResult = await ExecuteAsyncWebRequest<BaseResponse>(new Uri(base.KioskClientServiceBaseUrl + "/api/ads/" + advertisementId + "/expire"), null, "POST", 180000).ConfigureAwait(continueOnCapturedContext: false);
		LogResponse("KioskClientServiceAds.ExpireAd for advertisement id: " + advertisementId, webResult);
		return webResult?.Response?.Success == true;
	}

	public bool SendProofOfPlay(IAdImpression adImpression)
	{
		bool result = false;
		if (adImpression != null)
		{
			if (adImpression.StartDisplayTime.HasValue)
			{
				ProofOfPlayRequest requestPayload = new ProofOfPlayRequest
				{
					DisplayedTime = adImpression.StartDisplayTime.Value,
					DisplayedMilliseconds = adImpression.DisplayMilliseconds
				};
				IVistarAd vistarAd = adImpression?.Ad as IVistarAd;
				WebResult<BaseResponse> webResult = ExecuteWebRequest<BaseResponse>(new Uri(base.KioskClientServiceBaseUrl + "/api/ads/" + vistarAd?.AdvertisementId + "/proofofplay"), requestPayload, "POST", 2000);
				LogResponse($"KioskClientServiceAds.ProofOfPlay for advertisement id: {vistarAd?.AdvertisementId}, Display Duration: {adImpression?.DisplayMilliseconds}, ", webResult);
				result = webResult?.Response?.Success == true;
			}
			else
			{
				LogHelper.Instance.Log("Error in KioskClientServiceAds.SendProofOfPlay. StartDisplayTime is null.");
			}
		}
		else
		{
			LogHelper.Instance.Log("Error in KioskClientServiceAds.SendProofOfPlay. AdImpression is null.");
		}
		return result;
	}

	public async Task<bool> SendProofOfPlayAsync(IAdImpression adImpression)
	{
		bool result = false;
		if (adImpression != null)
		{
			if (adImpression.StartDisplayTime.HasValue)
			{
				ProofOfPlayRequest requestPayload = new ProofOfPlayRequest
				{
					DisplayedTime = adImpression.StartDisplayTime.Value,
					DisplayedMilliseconds = adImpression.DisplayMilliseconds
				};
				IVistarAd vistarAd = adImpression?.Ad as IVistarAd;
				WebResult<BaseResponse> webResult = await ExecuteAsyncWebRequest<BaseResponse>(new Uri(base.KioskClientServiceBaseUrl + "/api/ads/" + vistarAd?.AdvertisementId + "/proofofplay"), requestPayload, "POST", 2000).ConfigureAwait(continueOnCapturedContext: false);
				LogResponse($"KioskClientServiceAds.ProofOfPlay for advertisement id: {vistarAd?.AdvertisementId}, Display Duration: {adImpression?.DisplayMilliseconds}, ", webResult);
				result = webResult?.Response?.Success == true;
			}
			else
			{
				LogHelper.Instance.Log("Error in KioskClientServiceAds.SendProofOfPlay. StartDisplayTime is null.");
			}
		}
		else
		{
			LogHelper.Instance.Log("Error in KioskClientServiceAds.SendProofOfPlay. AdImpression is null.");
		}
		return result;
	}

	private bool PostGetAdTimeoutEvent(AdLocation adLocation, int timeoutSetting)
	{
		GetAdTimeoutEvent requestPayload = new GetAdTimeoutEvent(adLocation, timeoutSetting);
		return ExecuteWebRequest<BaseResponse>(new Uri(base.KioskClientServiceBaseUrl + "/api/analytics/getadtimeoutevent"), requestPayload, "POST")?.Response?.Success == true;
	}
}
