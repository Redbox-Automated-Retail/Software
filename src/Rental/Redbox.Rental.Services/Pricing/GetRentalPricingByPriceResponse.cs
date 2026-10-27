using System.Collections.Generic;
using Redbox.Core;
using Redbox.Rental.Services.App;

namespace Redbox.Rental.Services.Pricing;

public class GetRentalPricingByPriceResponse : ApiBaseResponse, IMessageScrub
{
	public IList<RentalPricingByPrice> RentalPricing { get; set; }

	public object Scrub()
	{
		return this.ToJson();
	}
}
