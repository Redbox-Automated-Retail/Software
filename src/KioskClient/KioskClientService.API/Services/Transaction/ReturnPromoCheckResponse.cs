using KioskClientService.API.App;

namespace KioskClientService.API.Services.Transaction;

public class ReturnPromoCheckResponse : ApiBaseResponse
{
	public PromotionInfo Promotion { get; set; }

	public bool IsLastDisc { get; set; }

	public override object Scrub()
	{
		return this.ToJsonIndented();
	}
}
