using System.Collections.Generic;
using KioskClientService.API.App;

namespace KioskClientService.API.Services.ProxyApi;

public class KioskRentalPricingByPriceResponse : ApiBaseResponse
{
	public IList<RentalPricingByPrice> RentalPricing { get; set; }

	public new object Scrub()
	{
		return $"RentalPricing count: {RentalPricing.Count}";
	}
}
