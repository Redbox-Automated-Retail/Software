using System.Collections.Generic;
using Redbox.Core;
using Redbox.Rental.Services.App;

namespace Redbox.Rental.Services.Pricing;

public class GetRentalDefaultPricingResponse : ApiBaseResponse, IMessageScrub
{
	public IList<FormatPricing> FormatPricing { get; set; }

	public object Scrub()
	{
		return this.ToJson();
	}
}
