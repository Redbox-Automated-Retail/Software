using System.Collections.Generic;

namespace KioskClientService.API.Services.ProxyApi;

public class RecommendedTitles
{
	public string QueryId { get; set; }

	public string OfferCode { get; set; }

	public List<TitleInfo> TitleIds { get; set; }
}
