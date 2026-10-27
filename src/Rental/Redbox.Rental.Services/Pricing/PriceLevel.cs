using ProductPricing.Model;

namespace Redbox.Rental.Services.Pricing;

public class PriceLevel
{
	public PriceLevelType PriceLevelType { get; set; }

	public string PriceLevelTypeValue { get; set; }
}
