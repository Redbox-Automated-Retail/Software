using System.Collections.Generic;

namespace Redbox.Rental.Model.KioskClientService.CustomerProfile;

public class KioskRecommendedTitles
{
	public string QueryId { get; set; }

	public string OfferCode { get; set; }

	public List<KioskTitleInfo> Products { get; set; }
}
