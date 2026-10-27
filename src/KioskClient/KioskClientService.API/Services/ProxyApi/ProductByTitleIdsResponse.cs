using System.Collections.Generic;
using KioskClientService.API.App;

namespace KioskClientService.API.Services.ProxyApi;

public class ProductByTitleIdsResponse : ApiBaseResponse, IMessageScrub
{
	public List<Product> Products { get; set; } = new List<Product>();

	public override object Scrub()
	{
		return string.Empty;
	}
}
