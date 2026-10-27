using System.Collections.Generic;
using KioskClientService.API.App;
using KioskClientService.API.Services.ProxyApi;

namespace KioskClientService.API.Services.Pricing;

public class GetRentalPricingByPriceResponse : ApiBaseResponse, IMessageScrub
{
	public IList<RentalPricingByPrice> RentalPricing { get; set; }

	public new object Scrub()
	{
		return $"RentalPricing count: {RentalPricing?.Count}";
	}
}
