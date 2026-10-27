using System.Collections.Generic;

namespace Redbox.Rental.Model.KioskClientService.Promotion;

public class DiscountValidationCampaignTitlesResult
{
	public bool Include { get; set; }

	public List<long> Titles { get; set; }
}
