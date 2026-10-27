using System.Collections.Generic;

namespace KioskClientService.API.Services.ProxyApi;

public class KioskClientRecommendedTitles
{
	public string QueryId { get; set; }

	public string OfferCode { get; set; }

	public List<KioskClientTitleInfo> TitleIds { get; set; }
}
