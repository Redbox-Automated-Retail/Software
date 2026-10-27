using System.Collections.Generic;
using KioskClientService.API.Services.ProxyApi;

namespace KioskClientService.API.Services.Campaigns;

public class KioskInCartDetails
{
	public InCartType InCartType { get; set; }

	public decimal? Amount { get; set; }

	public string PromoCode { get; set; }

	public List<long> ExcludeTitles { get; set; }

	public static KioskInCartDetails FromInCartDetails(InCartDetails inCartDetails)
	{
		KioskInCartDetails kioskInCartDetails = new KioskInCartDetails
		{
			InCartType = inCartDetails.InCartType,
			Amount = inCartDetails.Amount,
			PromoCode = inCartDetails.PromoCode
		};
		if (inCartDetails.ExcludeTitles != null && inCartDetails.ExcludeTitles.Count > 0)
		{
			kioskInCartDetails.ExcludeTitles = new List<long>();
			kioskInCartDetails.ExcludeTitles.AddRange(inCartDetails.ExcludeTitles);
		}
		return kioskInCartDetails;
	}
}
