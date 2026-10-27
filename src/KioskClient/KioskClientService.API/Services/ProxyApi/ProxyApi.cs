using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using KioskClientService.API.App;
using KioskClientService.API.Models;
using KioskClientService.API.Services.Configuration;
using KioskClientService.API.Services.Inventory.Models;
using KioskClientService.API.Services.Promotion;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API.Services.ProxyApi;

internal class ProxyApi : IProxyApi
{
	private readonly IApplicationSettings _applicationSettings;

	private readonly IKioskConfiguration _kioskConfiguration;

	private readonly IHttpService _httpService;

	private readonly ILogger<IProxyApi> _logger;

	private KioskCloudClientConfiguration _kioskCloudClientConfiguration;

	private long _kioskId;

	private const string _cms = "api/cms";

	private const string _customerProfile = "api/customerprofile";

	private const string _installer = "api/installer";

	private const string _inventory = "api/inventory";

	private const string _kiosk = "api/kiosk";

	private const string _loyalty = "api/loyalty";

	private const string _marketing = "api/marketing";

	private const string _planogram = "api/planogram";

	private const string _pricing = "api/pricing";

	private const string _promotion = "api/promotion";

	private const string _subscription = "api/subscription";

	private const string _product = "api/product";

	private const string _proxyAuthHeader = "x-api-key";

	private string ProxyApiUrl
	{
		get
		{
			if (!string.IsNullOrEmpty(_kioskConfiguration.KioskClientProxyApi.ProxyApiUrl))
			{
				return _kioskConfiguration.KioskClientProxyApi.ProxyApiUrl;
			}
			return _applicationSettings.AppSettings.DefaultProxyServiceUrl;
		}
	}

	public ProxyApi(IStoreService store, IOptionsSnapshotKioskConfiguration optionsSnapshotKioskConfiguration, IApplicationSettings applicationSettings, IHttpService httpService, ILogger<IProxyApi> logger)
	{
		_kioskConfiguration = optionsSnapshotKioskConfiguration;
		_applicationSettings = applicationSettings;
		_httpService = httpService;
		_logger = logger;
		if (_kioskConfiguration.KioskClientProxyApi.ProxyApiTimeout != 0)
		{
			_httpService.Timeout = _kioskConfiguration.KioskClientProxyApi.ProxyApiTimeout;
		}
		_kioskCloudClientConfiguration = new KioskCloudClientConfiguration
		{
			APIKey = _kioskConfiguration.KioskClientProxyApi.ProxyApiKey,
			AuthHeader = "x-api-key"
		};
		_kioskId = store?.KioskId ?? 0;
	}

	public async Task<APIResponse<GetProxySubscriptionPlanResponse>> GetSubscriptionPlans()
	{
		string endpoint = "api/subscription/subscriptionplans";
		return await _httpService.SendRequest<GetProxySubscriptionPlanResponse>(ProxyApiUrl, endpoint, null, HttpMethod.Get, GetHeaders(_kioskId));
	}

	public async Task<APIResponse<CustomerProfileKioskLoginResponse>> Login(CustomerProfileLoginRequest request)
	{
		string endpoint = "api/customerprofile/login";
		return await _httpService.SendRequest<CustomerProfileKioskLoginResponse>(ProxyApiUrl, endpoint, request, HttpMethod.Post, _kioskCloudClientConfiguration.GetKioskCloudHeaders(request.MessageId, request.KioskId));
	}

	public async Task<APIResponse<InstallerModels.PendingStatesResponse>> GetPendingStates()
	{
		string endpoint = "api/installer/getpendingstates";
		return await _httpService.SendRequest<InstallerModels.PendingStatesResponse>(ProxyApiUrl, endpoint, null, HttpMethod.Get, _kioskCloudClientConfiguration.GetKioskCloudHeaders(null, _kioskId), logResponseContent: false);
	}

	public async Task<APIResponse<InstallerModels.PendingBannersResponse>> GetPendingBanners(int stateid)
	{
		string endpoint = "api/installer" + $"/getpendingbanners?stateid={stateid}";
		return await _httpService.SendRequest<InstallerModels.PendingBannersResponse>(ProxyApiUrl, endpoint, null, HttpMethod.Get, _kioskCloudClientConfiguration.GetKioskCloudHeaders(null, _kioskId), logResponseContent: false);
	}

	public async Task<APIResponse<InstallerModels.PendingKiosksResponse>> GetPendingKiosks(short stateid, int bannerid)
	{
		string endpoint = "api/installer" + $"/getpendingkiosks?stateid={stateid}&bannerid={bannerid}";
		return await _httpService.SendRequest<InstallerModels.PendingKiosksResponse>(ProxyApiUrl, endpoint, null, HttpMethod.Get, _kioskCloudClientConfiguration.GetKioskCloudHeaders(null, _kioskId), logResponseContent: false);
	}

	public async Task<APIResponse<InstallerModels.OpsMarketResponse>> GetOpsMarket(long kioskid)
	{
		string endpoint = "api/installer" + $"/getopsmarket?kioskid={kioskid}";
		return await _httpService.SendRequest<InstallerModels.OpsMarketResponse>(ProxyApiUrl, endpoint, null, HttpMethod.Get, _kioskCloudClientConfiguration.GetKioskCloudHeaders(null, kioskid), logResponseContent: false);
	}

	public async Task<APIResponse<KioskModels.KioskAuthenticateResponse>> Authenticate(KioskModels.KioskAuthenticateRequest request)
	{
		return await _httpService.SendRequest<KioskModels.KioskAuthenticateResponse>(ProxyApiUrl, "api/kiosk/authenticate", request, HttpMethod.Post, _kioskCloudClientConfiguration.GetKioskCloudHeaders(null, request.KioskId));
	}

	public async Task<APIResponse<EmailConfirmationResponse>> EmailConfirmation(EmailConfirmationPayload payload)
	{
		string endpoint = "api/marketing/emailconfirmation";
		return await _httpService.SendRequest<EmailConfirmationResponse>(ProxyApiUrl, endpoint, payload, HttpMethod.Post, _kioskCloudClientConfiguration.GetKioskCloudHeaders(payload.MessageId, payload.KioskId));
	}

	public async Task<APIResponse<KioskOptInResponse>> KioskOptIn(KioskOptInPayload payload)
	{
		string endpoint = "api/marketing/kioskoptin";
		return await _httpService.SendRequest<KioskOptInResponse>(ProxyApiUrl, endpoint, payload, HttpMethod.Post, _kioskCloudClientConfiguration.GetKioskCloudHeaders(payload.MessageId, payload.KioskId));
	}

	public async Task<APIResponse<GetProxyRedboxPlusPromoCampaignsResponse>> GetRedboxPlusPromoCampaigns()
	{
		string endpoint = "api/subscription/redboxpluspromocampaigns";
		return await _httpService.SendRequest<GetProxyRedboxPlusPromoCampaignsResponse>(ProxyApiUrl, endpoint, null, HttpMethod.Get, GetHeaders(_kioskId));
	}

	public async Task<APIResponse<ServerLoyaltyResponse>> EstimateAccrualAsync(CartPayload payload)
	{
		string endpoint = "api/loyalty/estimateaccrual";
		return await _httpService.SendRequest<ServerLoyaltyResponse>(ProxyApiUrl, endpoint, payload, HttpMethod.Post, _kioskCloudClientConfiguration.GetKioskCloudHeaders(payload.MessageId, payload.KioskId));
	}

	public async Task<APIResponse<ServerLoyaltyResponse>> EstimateRedemptionAsync(CartPayload payload)
	{
		string endpoint = "api/loyalty/estimateredemption";
		return await _httpService.SendRequest<ServerLoyaltyResponse>(ProxyApiUrl, endpoint, payload, HttpMethod.Post, _kioskCloudClientConfiguration.GetKioskCloudHeaders(payload.MessageId, payload.KioskId));
	}

	public async Task<APIResponse<UpdateAccountResponse>> UpdateAccountAsync(UpdateAccountPayload payload)
	{
		string endpoint = "api/loyalty/updateaccount";
		int hold = _httpService.Timeout;
		_httpService.Timeout = 60000;
		APIResponse<UpdateAccountResponse> result = await _httpService.SendRequest<UpdateAccountResponse>(ProxyApiUrl, endpoint, payload, HttpMethod.Post, _kioskCloudClientConfiguration.GetKioskCloudHeaders(payload.MessageId, payload.KioskId));
		_httpService.Timeout = hold;
		return result;
	}

	public async Task<APIResponse<PromotionCodeValidationResponse>> PromoCodeValidationAsync(PromotionCodeValidationPayload payload)
	{
		string endpoint = "api/promotion/promocodevalidation";
		return await _httpService.SendRequest<PromotionCodeValidationResponse>(ProxyApiUrl, endpoint, payload, HttpMethod.Post, _kioskCloudClientConfiguration.GetKioskCloudHeaders(payload.MessageId, payload.KioskId));
	}

	public async Task<APIResponse<KioskRentalDefaultPricingResponse>> GetRentalDefaultPricing(long kioskId)
	{
		string endpoint = "api/pricing" + $"/kiosk/{kioskId}/rental/default";
		return await _httpService.SendRequest<KioskRentalDefaultPricingResponse>(ProxyApiUrl, endpoint, null, HttpMethod.Get, GetHeaders(kioskId));
	}

	public async Task<APIResponse<KioskRentalPricingByPriceResponse>> GetRentalPricingByPrice(long kioskId)
	{
		string endpoint = "api/pricing" + $"/kiosk/{kioskId}/rentalbyprice";
		return await _httpService.SendRequest<KioskRentalPricingByPriceResponse>(ProxyApiUrl, endpoint, null, HttpMethod.Get, GetHeaders(kioskId));
	}

	public async Task<APIResponse<KioskPurchasePricingByPriceResponse>> GetPurchasePricingByPrice(long kioskId)
	{
		string endpoint = "api/pricing" + $"/kiosk/{kioskId}/purchasebyprice";
		return await _httpService.SendRequest<KioskPurchasePricingByPriceResponse>(ProxyApiUrl, endpoint, null, HttpMethod.Get, GetHeaders(kioskId));
	}

	public async Task<APIResponse<GetPlanogramsResponse>> GetPlanograms(long kioskId, DateTime? lastChange = null)
	{
		string text = (lastChange.HasValue ? lastChange.Value.ToString("yyyy-MM-ddTHH:mm:ss") : "null");
		string endpoint = "api/planogram/allcurrent?lastReportedTime=" + text;
		return await _httpService.SendRequest<GetPlanogramsResponse>(ProxyApiUrl, endpoint, null, HttpMethod.Get, GetHeaders(kioskId));
	}

	public async Task<APIResponse<PostKioskCampaignsResponse>> PostKioskCampaigns(long kioskId, PostKioskCampaignsRequest request)
	{
		string endpoint = "api/cms" + $"/campaign/kiosk/{kioskId}";
		return await _httpService.SendRequest<PostKioskCampaignsResponse>(ProxyApiUrl, endpoint, request, HttpMethod.Post, GetHeaders(kioskId));
	}

	public async Task<APIResponse<NearbyKiosksResponse>> GetNearbyKiosks(NearbyKiosksRequest request)
	{
		return await _httpService.SendRequest<NearbyKiosksResponse>(ProxyApiUrl, string.Format("{0}/{1}/nearby", "api/kiosk", request.KioskId), null, HttpMethod.Get, _kioskCloudClientConfiguration.GetKioskCloudHeaders(request.MessageId, request.KioskId).AddCompression());
	}

	public async Task<APIResponse<KioskTitlesCountResponse>> KioskTitlesCount(KiosksAndTitlesPayload payload)
	{
		return await _httpService.SendRequest<KioskTitlesCountResponse>(ProxyApiUrl, "api/inventory/kiosktitlescount", payload, HttpMethod.Post, _kioskCloudClientConfiguration.GetKioskCloudHeaders(payload.MessageId, payload.SourceKioskId).AddCompression());
	}

	public async Task<APIResponse<ProductByTitleIdsResponse>> PostProductsByTitleIds(long kioskId, Guid messageId, ProductByTitleIdsRequest request)
	{
		return await _httpService.SendRequest<ProductByTitleIdsResponse>(ProxyApiUrl, "api/product/bytitleids", request, HttpMethod.Post, _kioskCloudClientConfiguration.GetKioskCloudHeaders(messageId, kioskId));
	}

	public async Task<APIResponse<ProductByTitleIdsResponse>> PostProductsByTitleIds(long kioskId, Guid messageId, List<long> titleIds)
	{
		return await _httpService.SendRequest<ProductByTitleIdsResponse>(ProxyApiUrl, "api/product/bytitleids", new
		{
			TitleIds = titleIds
		}, HttpMethod.Post, _kioskCloudClientConfiguration.GetKioskCloudHeaders(messageId, kioskId));
	}

	private List<Header> GetHeaders(long kioskId)
	{
		List<Header> kioskCloudHeaders = _kioskCloudClientConfiguration.GetKioskCloudHeaders(Guid.NewGuid(), kioskId);
		return kioskCloudHeaders.AddCompression();
	}
}
