using Newtonsoft.Json;

namespace Redbox.Rental.Services.App;

public class ApiBaseResponse
{
	[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
	public string Error { get; set; }
}
