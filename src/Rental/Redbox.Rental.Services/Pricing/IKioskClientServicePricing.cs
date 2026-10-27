using ProductPricing.Model;

namespace Redbox.Rental.Services.Pricing;

public interface IKioskClientServicePricing
{
	bool IsKCSPricingEnabled { get; }

	void CachePricing();

	ProductPricing GetLocalProductPricing(long productId, PriceLevelType priceLevelType, decimal rentalTaxRate, decimal purchaseTaxRate, string state);

	ProductPricing GetLocalProductDefaultPricing(PriceLevelType priceLevelType, decimal rentalTaxRate, decimal purchaseTaxRate, string state);
}
