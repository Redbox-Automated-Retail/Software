using System.Collections.Generic;
using KioskClientService.API.App;

namespace KioskClientService.API.Services.ProxyApi;

public class KioskRentalDefaultPricingResponse : ApiBaseResponse
{
	public IList<FormatPricing> FormatPricing { get; set; }

	public new object Scrub()
	{
		return $"FormatPricing count: {FormatPricing.Count}";
	}
}
