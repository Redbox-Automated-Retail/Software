using System.Collections.Generic;
using Redbox.Core;
using Redbox.Rental.Services.App;

namespace Redbox.Rental.Services.Pricing;

public class GetPurchasePricingByPriceResponse : ApiBaseResponse, IMessageScrub
{
	public IList<PurchasePricingByPrice> PurchasePricing { get; set; }

	public object Scrub()
	{
		return this.ToJson();
	}
}
