using Redbox.Core;
using Redbox.Rental.Services.App;

namespace Redbox.Rental.Services.Pricing;

public class GetProductPricingResponse : ApiBaseResponse, IMessageScrub
{
	public ProductPricing ProductPricing { get; set; }

	public object Scrub()
	{
		return this.ToJson();
	}
}
