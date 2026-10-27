using ProductPricing.Model;
using Redbox.Core;
using Redbox.Rental.Services.App;

namespace Redbox.Rental.Services.Pricing;

public class ProductPricingRequest : IMessageScrub
{
	public long ProductId { get; set; }

	public PriceLevelType PriceLevelType { get; set; }

	public decimal RentalTaxRate { get; set; }

	public decimal PurchaseTaxRate { get; set; }

	public string State { get; set; }

	public object Scrub()
	{
		return this.ToJson();
	}
}
