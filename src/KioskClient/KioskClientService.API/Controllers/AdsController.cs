using System.Collections.Generic;
using System.Dynamic;
using System.Net;
using System.Threading.Tasks;
using KioskClientService.API.Ads;
using KioskClientService.API.App;
using KioskClientService.API.Services.Ads;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API.Controllers;

[Route("api/ads")]
[ApiController]
public class AdsController : ControllerBase
{
	private readonly ILogger _logger;

	private readonly IAdService _adService;

	public AdsController(ILogger<AdsController> logger, IAdService adService)
	{
		_logger = logger;
		_adService = adService;
	}

	[HttpGet]
	public async Task<ActionResult<GetAdResponse>> GetAd(AdLocation location)
	{
		GetAdResponse getAdResponse = await _adService.GetAd(new GetAdRequest
		{
			AdLocation = location
		});
		dynamic baseResponse = GetBaseResponse(getAdResponse);
		baseResponse.AdLocation = getAdResponse.AdLocation;
		baseResponse.AdvertisementId = getAdResponse.AdvertisementId;
		baseResponse.AssetPath = getAdResponse.AssetPath;
		baseResponse.Height = getAdResponse.Height;
		baseResponse.MimeType = getAdResponse.MimeType;
		baseResponse.Width = getAdResponse.Width;
		ObjectResult objectResult = new ObjectResult(baseResponse);
		objectResult.StatusCode = baseResponse?.StatusCode;
		return objectResult;
	}

	[HttpPost]
	[Route("preload")]
	public async Task<ActionResult<List<GetAdsResponse>>> PostPreload([FromBody] List<AdLocation> locations)
	{
		_logger?.LogInformation("AdController.Preload - " + string.Join(',', locations));
		return (await _adService.GetAds(locations, isPreload: true)).ToObjectResult();
	}

	[HttpGet]
	[Route("cache")]
	public async Task<ActionResult<CacheResponse>> GetCache()
	{
		_logger?.LogInformation("AdController.GetCache");
		return (await _adService.Cache()).ToObjectResult();
	}

	[HttpPost]
	[Route("{advertisementId}/proofofplay")]
	public async Task<ActionResult<ProofOfPlayResponse>> PostProofOfPlay(string advertisementId, [FromBody] ProofOfPlayRequest proofOfPlayRequest)
	{
		_logger?.LogInformation("AdController.PostProofOfPlay");
		dynamic baseResponse = GetBaseResponse(await _adService.PostProofOfPlay(advertisementId, proofOfPlayRequest));
		ObjectResult objectResult = new ObjectResult(baseResponse);
		objectResult.StatusCode = baseResponse.StatusCode;
		return objectResult;
	}

	[HttpPost]
	[Route("{advertisementId}/expire")]
	public async Task<ActionResult<ExpireResponse>> PostExpire(string advertisementId)
	{
		_logger?.LogInformation("AdController.PostExpire");
		dynamic baseResponse = GetBaseResponse(await _adService.Expire(advertisementId));
		ObjectResult objectResult = new ObjectResult(baseResponse);
		objectResult.StatusCode = baseResponse.StatusCode;
		return objectResult;
	}

	private ExpandoObject GetBaseResponse<T>(T response) where T : ApiBaseResponse
	{
		dynamic val = new ExpandoObject();
		val.StatusCode = (int)(response?.StatusCode ?? HttpStatusCode.InternalServerError);
		val.Success = response != null && response.StatusCode == HttpStatusCode.OK;
		if (!string.IsNullOrEmpty(response?.Error))
		{
			val.Errors = new List<Error>
			{
				new Error
				{
					Message = response.Error
				}
			};
		}
		return val;
	}
}
