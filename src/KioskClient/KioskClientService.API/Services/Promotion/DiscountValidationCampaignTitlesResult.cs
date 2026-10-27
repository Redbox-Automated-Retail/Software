using System.Collections.Generic;

namespace KioskClientService.API.Services.Promotion;

public class DiscountValidationCampaignTitlesResult
{
	public bool Include { get; set; }

	public List<long> Titles { get; set; }
}
