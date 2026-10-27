using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using KioskClientService.API.App;
using KioskClientService.API.Services.App;
using KioskClientService.API.Services.Assets;
using KioskClientService.API.Services.Configuration;
using KioskClientService.API.Services.Promotion;
using KioskClientService.API.Services.ProxyApi;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API.Services.Campaigns;

internal class CampaignService : ICampaignService
{
	private const string CampaignDataFileName = "Campaigns";

	private readonly ILogger<CampaignService> _logger;

	private readonly IStoreService _storeService;

	private readonly IPromotionService _promoService;

	private readonly IOptionsSnapshotKioskConfiguration _kioskConfiguration;

	private readonly IProxyApi _proxyApi;

	private readonly IPersistentDataCacheService _persistentDataCacheService;

	private readonly IMemoryCache _memoryCache;

	private readonly IAssetsService _assets;

	private static DateTime? _nextRefresh;

	public CampaignService(ILogger<CampaignService> logger, IStoreService storeService, IPromotionService promo, IOptionsSnapshotKioskConfiguration kioskConfiguration, IProxyApi proxyApi, IPersistentDataCacheService persistentDataCacheService, IMemoryCache memoryCache, IAssetsService assets)
	{
		_logger = logger;
		_storeService = storeService;
		_promoService = promo;
		_kioskConfiguration = kioskConfiguration;
		_proxyApi = proxyApi;
		_persistentDataCacheService = persistentDataCacheService;
		_memoryCache = memoryCache;
		_assets = assets;
		if (!_nextRefresh.HasValue)
		{
			int num = new Random().Next(60, Math.Max(2, _kioskConfiguration.Campaigns.RefreshInterval) * 60);
			_nextRefresh = DateTime.Now.AddSeconds(num);
			_logger.LogInfoWithSource($"Next Campaign refresh at: {_nextRefresh}", ".ctor");
		}
	}

	public async Task<ApiBaseResponse> RefreshIfNeeded()
	{
		if (_nextRefresh < DateTime.Now)
		{
			_nextRefresh = DateTime.Now.AddMinutes(_kioskConfiguration.Campaigns.RefreshInterval);
			_logger.LogInfoWithSource($"Running campaign refresh if needed. Next refresh at: {_nextRefresh}", "RefreshIfNeeded");
			return await Refresh();
		}
		return new ApiBaseResponse
		{
			StatusCode = HttpStatusCode.OK
		};
	}

	public async Task<ApiBaseResponse> Refresh()
	{
		_logger?.LogInfoWithSource("Started", "Refresh");
		try
		{
			if (_storeService.KioskId == 0L)
			{
				_logger?.LogErrorWithSource("KioskId must be > 0", "Refresh");
				return new ApiBaseResponse
				{
					StatusCode = HttpStatusCode.InternalServerError,
					Error = "KioskId is 0"
				};
			}
			CampaignData data = await ReadCampaign();
			PostKioskCampaignsRequest request = new PostKioskCampaignsRequest
			{
				LastSyncTime = data.LastSyncTime
			};
			APIResponse<PostKioskCampaignsResponse> aPIResponse = await _proxyApi.PostKioskCampaigns(_storeService.KioskId, request);
			if (!aPIResponse.IsSuccessStatusCode)
			{
				return new ApiBaseResponse
				{
					StatusCode = HttpStatusCode.InternalServerError,
					Error = "Error retrieving kiosk campaigns"
				};
			}
			if (aPIResponse.StatusCode == HttpStatusCode.AlreadyReported)
			{
				return new ApiBaseResponse
				{
					StatusCode = HttpStatusCode.AlreadyReported
				};
			}
			await ProcessResponse(aPIResponse.Response, data);
			return new ApiBaseResponse
			{
				StatusCode = HttpStatusCode.OK
			};
		}
		catch (Exception ex)
		{
			string text = "Exception occured retrieving kiosk capaigns";
			_logger.LogErrorWithSource(ex, text, "Refresh");
			return new ApiBaseResponse
			{
				StatusCode = HttpStatusCode.InternalServerError,
				Error = text
			};
		}
	}

	private async Task ProcessResponse(PostKioskCampaignsResponse response, CampaignData data)
	{
		if (response.InCarts != null)
		{
			data.InCarts.Clear();
			data.InCarts.AddRange(response.InCarts);
			_logger.LogInfoWithSource("Processed updates to Incarts", "ProcessResponse");
		}
		if (response.StartScreens != null)
		{
			IEnumerable<Asset> assets = response.StartScreens.GetAssets();
			_assets.BuildLocalPaths(assets);
			data.StartScreens.Clear();
			data.StartScreens.AddRange(response.StartScreens);
			await Task.WhenAll(_assets.Refresh(response.StartScreens), ValidatePromoCodes(response.StartScreens));
			_logger.LogInfoWithSource("Processed updates to StartScreens", "ProcessResponse");
		}
		data.LastSyncTime = response.LastSyncTime;
		await WriteCampaign(data);
	}

	private async Task<CampaignData> ReadCampaign()
	{
		PersistentDataWrapper<CampaignData> persistentDataWrapper = await _persistentDataCacheService.Read<CampaignData>("Campaigns", useCache: true);
		if (persistentDataWrapper == null || persistentDataWrapper.Data == null)
		{
			_logger?.LogInfoWithSource("Cache is not set", "ReadCampaign");
			return new CampaignData();
		}
		return persistentDataWrapper.Data;
	}

	private async Task<bool> WriteCampaign(CampaignData campaignData)
	{
		bool flag = await _persistentDataCacheService.Write(campaignData, "Campaigns");
		if (!flag)
		{
			_logger.LogInfoWithSource("Campaign data could not be written", "WriteCampaign");
		}
		_logger.LogInfoWithSource("Campaign data persisted", "WriteCampaign");
		return flag;
	}

	public bool DeleteCampaignData()
	{
		return _persistentDataCacheService.Delete("Campaigns");
	}

	public async Task<GetInCartResponse> GetInCart()
	{
		try
		{
			if (!_kioskConfiguration.Campaigns.InCartsActive)
			{
				return new GetInCartResponse
				{
					StatusCode = HttpStatusCode.NoContent,
					Error = "InCartsActive is false"
				};
			}
			KioskInCartDetails kioskInCartDetails = await GetActiveInCart();
			return new GetInCartResponse
			{
				InCart = kioskInCartDetails,
				StatusCode = ((kioskInCartDetails == null) ? HttpStatusCode.NotFound : HttpStatusCode.OK)
			};
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception occurred", "GetInCart");
			return new GetInCartResponse
			{
				StatusCode = HttpStatusCode.InternalServerError,
				Error = "Exception occurred"
			};
		}
	}

	private async Task<KioskInCartDetails> GetActiveInCart()
	{
		InCartDetails inCartDetails = (from c in await GetAllInCarts()
			where c.StartDate <= DateTime.Now && (!c.EndDate.HasValue || c.EndDate > DateTime.Now)
			orderby c.Rank descending, c.StartDate descending
			select c).FirstOrDefault();
		if (inCartDetails == null)
		{
			return null;
		}
		return KioskInCartDetails.FromInCartDetails(inCartDetails);
	}

	private async Task<List<InCartDetails>> GetAllInCarts()
	{
		CampaignData campaignData = await ReadCampaign();
		if (campaignData.InCarts != null)
		{
			return campaignData.InCarts;
		}
		return new List<InCartDetails>();
	}

	public async Task<GetStartScreenResponse> GetStartScreen()
	{
		try
		{
			if (!_kioskConfiguration.Campaigns.StartScreensActive)
			{
				return new GetStartScreenResponse
				{
					StatusCode = HttpStatusCode.NoContent,
					Error = "StartScreensActive is false"
				};
			}
			KioskStartScreenDetails kioskStartScreenDetails = await GetActiveStartScreen();
			return new GetStartScreenResponse
			{
				StartScreen = kioskStartScreenDetails,
				StatusCode = ((kioskStartScreenDetails == null) ? HttpStatusCode.NotFound : HttpStatusCode.OK)
			};
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception occurred", "GetStartScreen");
			return new GetStartScreenResponse
			{
				StatusCode = HttpStatusCode.InternalServerError,
				Error = ex.Message
			};
		}
	}

	private async Task<KioskStartScreenDetails> GetActiveStartScreen()
	{
		StartScreenDetails activeStartScreen = (from c in await GetAllStartScreens()
			where c.StartDate <= DateTime.Now && (!c.EndDate.HasValue || c.EndDate > DateTime.Now)
			orderby c.StartDate descending
			orderby c.Rank descending, c.StartDate descending
			select c).FirstOrDefault();
		if (activeStartScreen != null)
		{
			foreach (IControl control in activeStartScreen.Controls.Where((IControl c) => c.ControlType == ControlType.StartAsset).ToList())
			{
				if (!(control is StartAssetControl startAsset))
				{
					continue;
				}
				startAsset.Asset.Url = null;
				if (_assets.IsNotAValidAlreadyDownloadedAsset(startAsset.Asset.FullPath))
				{
					_logger.LogErrorWithSource($"Start asset control {control.ControlId} does not have a valid asset for: {startAsset.Asset.FullPath}, removing from start screen campaign", "GetActiveStartScreen");
					activeStartScreen.Controls.Remove(startAsset);
				}
				else if (startAsset.Target == AssetTarget.PromoCode)
				{
					PromotionCodeValidationResponse promotionCodeValidationResponse = await ValidatePromoCode(startAsset.TargetValue);
					if (promotionCodeValidationResponse != null && promotionCodeValidationResponse.Error == null)
					{
						startAsset.Validation = new StartScreenPromotionCodeValidationResponse(startAsset.TargetValue, promotionCodeValidationResponse);
						continue;
					}
					_logger.LogErrorWithSource($"Start asset control {control.ControlId} does not have a valid promo code: {startAsset.TargetValue}, removing from start screen campaign", "GetActiveStartScreen");
					activeStartScreen.Controls.Remove(startAsset);
				}
			}
		}
		return KioskStartScreenDetails.FromStartScreenDetails(activeStartScreen);
	}

	private async Task<List<StartScreenDetails>> GetAllStartScreens()
	{
		return (await ReadCampaign())?.StartScreens ?? new List<StartScreenDetails>();
	}

	private async Task ValidatePromoCodes(IEnumerable<StartScreenDetails> startScreenDetails)
	{
		HashSet<string> hashSet = new HashSet<string>();
		foreach (StartScreenDetails startScreenDetail in startScreenDetails)
		{
			IEnumerable<string> enumerable = from c in startScreenDetail.Controls
				where c.ControlType == ControlType.StartAsset && ((StartAssetControl)c).Target == AssetTarget.PromoCode
				select ((StartAssetControl)c).TargetValue;
			_logger.LogInfoWithSource($"campaign id: {startScreenDetail.CampaignId}, promos: {string.Join(',', enumerable)}", "ValidatePromoCodes");
			hashSet.UnionWith(enumerable);
		}
		List<Task> list = new List<Task>();
		foreach (string item in hashSet)
		{
			list.Add(ValidatePromoCode(item));
		}
		await Task.WhenAll(list);
	}

	private async Task<PromotionCodeValidationResponse> ValidatePromoCode(string code)
	{
		PromotionCodeValidationResponse response = null;
		if (_memoryCache.TryGetValue<PromotionCodeValidationResponse>(code, out var value))
		{
			_logger.LogInfoWithSource("code: " + code + ", memory: " + value.ToJson(), "ValidatePromoCode");
			response = value;
		}
		else
		{
			APIResponse<PromotionCodeValidationResponse> aPIResponse = await _promoService.ValidatePromotionCode(code);
			_logger.LogInfoWithSource($"code: {code}, live call: {aPIResponse.StatusCode}, {aPIResponse.Response.ToJson()}", "ValidatePromoCode");
			if (aPIResponse.StatusCode == HttpStatusCode.OK || aPIResponse.StatusCode == HttpStatusCode.UnprocessableEntity)
			{
				_memoryCache.Set(code, aPIResponse.Response, DateTimeOffset.Now.AddMinutes(_kioskConfiguration.Campaigns.PromoCodeCacheTTLMinutes));
				response = aPIResponse.Response;
			}
		}
		return response;
	}
}
