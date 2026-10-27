using System.Collections.Generic;
using KioskClientService.API.Services.App;
using KioskClientService.API.Services.ProxyApi;

namespace KioskClientService.API.Services.Pricing;

public class DefaultPricingData : IPersistentData
{
	public List<FormatPricing> FormatPricing { get; set; }
}
