using System.ComponentModel.DataAnnotations;
using KioskClientService.API.App;
using KioskClientService.API.Services.ProxyApi;

namespace KioskClientService.API.Services.Pricing;

public class ProductPricingRequest : IMessageScrub
{
	[Range(1, int.MaxValue, ErrorMessage = "ProductId value {0} is not within the valid range of {1} and {2}")]
	public long ProductId { get; set; }

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
