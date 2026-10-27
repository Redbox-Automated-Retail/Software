using Newtonsoft.Json;

namespace KioskClientService.API.App;

public class ErrorDetails
{
	public int StatusCode { get; set; }

	public string Message { get; set; }

	public override string ToString()
	{
		return JsonConvert.SerializeObject(this, Formatting.Indented);
	}
}
