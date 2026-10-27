using System.Collections.Generic;

namespace Redbox.Rental.Model.KioskClientService.Campaign;

public class KioskInCartDetails
{
	public InCartType InCartType { get; set; }

	public decimal? Amount { get; set; }

	public string PromoCode { get; set; }

	public List<long> ExcludeTitles { get; set; } = new List<long>();
}
