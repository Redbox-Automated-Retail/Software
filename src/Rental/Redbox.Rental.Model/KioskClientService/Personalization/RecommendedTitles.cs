using System.Collections.Generic;

namespace Redbox.Rental.Model.KioskClientService.Personalization;

public class RecommendedTitles
{
	public string QueryId { get; set; }

	public string OfferCode { get; set; }

	public List<TitleInfo> TitleIds { get; set; }
}
