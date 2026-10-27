using System.ComponentModel.DataAnnotations;
using KioskClientService.API.App;
using KioskClientService.API.Services.ProxyApi;

namespace KioskClientService.API.Services.Pricing;

public class ProductPriceLevelPricingRequest : IMessageScrub
{
	public PriceLevelType PriceLevelType { get; set; }

	public decimal RentalTaxRate { get; set; }

	public decimal PurchaseTaxRate { get; set; }

	[Required(ErrorMessage = "State is required")]
	public string State { get; set; }

	public object Scrub()
	{
		return this.ToJson();
	}
}
