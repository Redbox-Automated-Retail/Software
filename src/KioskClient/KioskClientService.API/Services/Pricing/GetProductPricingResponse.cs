using KioskClientService.API.App;

namespace KioskClientService.API.Services.Pricing;

public class GetProductPricingResponse : ApiBaseResponse, IMessageScrub
{
	public ProductPricing ProductPricing { get; set; }

	public new object Scrub()
	{
		return this.ToJson().EllipseAt(80);
	}
}
