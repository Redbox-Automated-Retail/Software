using System.Collections.Generic;
using KioskClientService.API.Services.App;
using KioskClientService.API.Services.ProxyApi;

namespace KioskClientService.API.Services.Pricing;

public class RentalPricingByPriceData : IPersistentData
{
	public IList<RentalPricingByPrice> RentalPricing { get; set; }
}
