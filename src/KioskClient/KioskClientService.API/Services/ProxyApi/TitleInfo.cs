using System.Collections.Generic;

namespace KioskClientService.API.Services.ProxyApi;

public class TitleInfo
{
	public string Id { get; set; }

	public string Type { get; set; }

	public List<string> Tags { get; set; }
}
