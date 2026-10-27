using System.Collections.Generic;
using KioskClientService.API.App;

namespace KioskClientService.API.Services.ProxyApi;

public class ProductByTitleIdsRequest : IMessageScrub
{
	public List<long> TitleIds { get; set; } = new List<long>();

	public object Scrub()
	{
		return string.Empty;
	}
}
