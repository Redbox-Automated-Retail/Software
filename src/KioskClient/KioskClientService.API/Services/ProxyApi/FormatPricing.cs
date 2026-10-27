using System.Collections.Generic;

namespace KioskClientService.API.Services.ProxyApi;

public class FormatPricing
{
	public PriceLevelType Name { get; set; }

	public IList<RentalPricing> Prices { get; set; }
}
