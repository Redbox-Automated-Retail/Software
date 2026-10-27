using System.Collections.Generic;
using KioskClientService.API.Services.App;
using KioskClientService.API.Services.ProxyApi;

namespace KioskClientService.API.Services.Pricing;

public class PurchasePricingByPriceData : IPersistentData
{
	public IList<PurchasePricingByPrice> PurchasePricing { get; set; }
}
