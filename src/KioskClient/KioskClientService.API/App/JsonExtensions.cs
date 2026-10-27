using Newtonsoft.Json;

namespace KioskClientService.API.App;

public static class JsonExtensions
{
	public static string ToJsonIndented(this object obj)
	{
		return JsonConvert.SerializeObject(obj, Formatting.Indented, new JsonSerializerSettings
		{
			NullValueHandling = NullValueHandling.Ignore
		});
	}

	public static string ToJson(this object obj)
	{
		return JsonConvert.SerializeObject(obj, Formatting.None, new JsonSerializerSettings
		{
			NullValueHandling = NullValueHandling.Ignore
		});
	}
}
