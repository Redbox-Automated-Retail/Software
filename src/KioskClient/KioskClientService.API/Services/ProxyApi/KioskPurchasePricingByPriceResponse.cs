using System.Collections.Generic;
using KioskClientService.API.App;

namespace KioskClientService.API.Services.ProxyApi;

public class KioskPurchasePricingByPriceResponse : ApiBaseResponse
{
	public IList<PurchasePricingByPrice> PurchasePricing { get; set; }

	public new object Scrub()
	{
		return $"PurchasePricing count: {PurchasePricing.Count}";
	}
}
