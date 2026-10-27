using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KioskClientService.API.App;
using KioskClientService.API.Models;
using KioskClientService.API.Services.Inventory.Models;
using KioskClientService.API.Services.Promotion;

namespace KioskClientService.API.Services.ProxyApi;

public interface IProxyApi
{
	Task<APIResponse<GetProxySubscriptionPlanResponse>> GetSubscriptionPlans();

	Task<APIResponse<CustomerProfileKioskLoginResponse>> Login(CustomerProfileLoginRequest request);

	Task<APIResponse<InstallerModels.PendingStatesResponse>> GetPendingStates();

	Task<APIResponse<InstallerModels.PendingBannersResponse>> GetPendingBanners(int stateid);

	Task<APIResponse<InstallerModels.PendingKiosksResponse>> GetPendingKiosks(short stateid, int bannerId);

	Task<APIResponse<InstallerModels.OpsMarketResponse>> GetOpsMarket(long kioskid);

	Task<APIResponse<KioskModels.KioskAuthenticateResponse>> Authenticate(KioskModels.KioskAuthenticateRequest request);

	Task<APIResponse<EmailConfirmationResponse>> EmailConfirmation(EmailConfirmationPayload payload);

	Task<APIResponse<KioskOptInResponse>> KioskOptIn(KioskOptInPayload payload);

	Task<APIResponse<GetProxyRedboxPlusPromoCampaignsResponse>> GetRedboxPlusPromoCampaigns();

	Task<APIResponse<PromotionCodeValidationResponse>> PromoCodeValidationAsync(PromotionCodeValidationPayload payload);

	Task<APIResponse<ServerLoyaltyResponse>> EstimateAccrualAsync(CartPayload payload);

	Task<APIResponse<ServerLoyaltyResponse>> EstimateRedemptionAsync(CartPayload payload);

	Task<APIResponse<KioskRentalDefaultPricingResponse>> GetRentalDefaultPricing(long kioskId);

	Task<APIResponse<KioskRentalPricingByPriceResponse>> GetRentalPricingByPrice(long kioskId);

	Task<APIResponse<KioskPurchasePricingByPriceResponse>> GetPurchasePricingByPrice(long kioskId);

	Task<APIResponse<UpdateAccountResponse>> UpdateAccountAsync(UpdateAccountPayload payload);

	Task<APIResponse<GetPlanogramsResponse>> GetPlanograms(long kioskId, DateTime? lastChange = null);

	Task<APIResponse<PostKioskCampaignsResponse>> PostKioskCampaigns(long kioskId, PostKioskCampaignsRequest request);

	Task<APIResponse<NearbyKiosksResponse>> GetNearbyKiosks(NearbyKiosksRequest request);

	Task<APIResponse<KioskTitlesCountResponse>> KioskTitlesCount(KiosksAndTitlesPayload payload);

	Task<APIResponse<ProductByTitleIdsResponse>> PostProductsByTitleIds(long kioskId, Guid messageId, ProductByTitleIdsRequest request);

	Task<APIResponse<ProductByTitleIdsResponse>> PostProductsByTitleIds(long kioskId, Guid messageId, List<long> titleIds);
}
