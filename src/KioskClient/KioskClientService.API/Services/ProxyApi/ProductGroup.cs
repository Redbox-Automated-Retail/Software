using System.Collections.Generic;

namespace KioskClientService.API.Services.ProxyApi;

public class ProductGroup
{
	public long ProductGroupId { get; set; }

	public Dictionary<string, long> ProductIds { get; set; } = new Dictionary<string, long>();
}
