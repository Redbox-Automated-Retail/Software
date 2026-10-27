using System.Collections.Generic;

namespace KioskClientService.API.Services.ProxyApi;

public sealed class PersonalizationResponseError
{
	public string Code { get; set; }

	public string Description { get; set; }

	public List<string> Details { get; set; }

	public string Exception { get; set; }
}
