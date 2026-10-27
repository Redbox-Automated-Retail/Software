using System;
using Newtonsoft.Json.Linq;

namespace KioskClientService.API.Services.Campaigns;

internal static class JTokenExtensions
{
	public static JToken GetCaseInsensitveKeyValue(this JToken source, string key)
	{
		return (source as JObject).GetValue(key, StringComparison.InvariantCultureIgnoreCase);
	}
}
