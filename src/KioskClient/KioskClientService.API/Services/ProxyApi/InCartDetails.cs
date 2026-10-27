using System.Collections.Generic;

namespace KioskClientService.API.Services.ProxyApi;

public class InCartDetails : BaseDetails
{
	public long CampaignInCartId { get; set; }

	public InCartType InCartType { get; set; }

	public decimal? Amount { get; set; }

	public string PromoCode { get; set; }

	public List<long> ExcludeTitles { get; set; }

	public InCartDetails()
	{
		base.CampaignType = CampaignType.InCart;
	}
}
