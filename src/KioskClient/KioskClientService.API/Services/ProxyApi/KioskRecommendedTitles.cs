using System.Collections.Generic;

namespace KioskClientService.API.Services.ProxyApi;

public class KioskRecommendedTitles
{
	public string QueryId { get; set; }

	public string OfferCode { get; set; }

	public List<KioskTitleInfo> Products { get; set; }
}
