using System.Threading.Tasks;
using KioskClientService.API.Services.ProxyApi;

namespace KioskClientService.API.Services.Pricing;

public interface IPricingService
{
	Task RefreshPricing();

	Task RefreshDefaultPricing();

	Task RefreshRentalPricingByPrice();

	Task RefreshPurchasePricingByPrice();

	Task RefreshIfNeeded();

	Task RefreshDefaultPricingIfNeeded();

	Task RefreshRentalPricingByPriceIfNeeded();

	Task RefreshPurchasdePricingByPriceIfNeeded();

	Task<GetRentalDefaultPricingResponse> GetRentalDefaultPricing();

	Task<GetRentalPricingByPriceResponse> GetRentalPricingByPrice();

	Task<GetPurchasePricingByPriceResponse> GetPurchasePricingByPrice();

	void DeleteAllPricing();

	void DeleteRentalDefaultPricing();

	void DeleteRentalPricingByPrice();

	void DeletePurchasePricingByPrice();

	Task<GetProductPricingResponse> GetProductPricing(long productId, PriceLevelType priceLevelType, decimal rentalTaxRate, decimal purchaseTaxRate, string state);

	Task<GetProductPricingResponse> GetProductDefaultPricing(PriceLevelType priceLevelType, decimal rentalTaxRate, decimal purchaseTaxRate, string state);
}
