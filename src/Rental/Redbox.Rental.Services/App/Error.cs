using Newtonsoft.Json;

namespace Redbox.Rental.Services.App;

public class Error
{
	[JsonProperty(Required = Required.DisallowNull)]
	public string Code { get; set; }

	[JsonProperty(Required = Required.DisallowNull)]
	public string Message { get; set; }
}
