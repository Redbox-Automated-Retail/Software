using System.Collections.Generic;
using KioskClientService.API.App;
using KioskClientService.API.Services.ProxyApi;

namespace KioskClientService.API.Services.Pricing;

public class GetRentalDefaultPricingResponse : ApiBaseResponse, IMessageScrub
{
	public IList<FormatPricing> FormatPricing { get; set; }

	public new object Scrub()
	{
		return $"ForrmatPricing count: {FormatPricing?.Count}";
	}
}
