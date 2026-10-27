using Newtonsoft.Json;

namespace Redbox.Rental.Model.EngineApplication;

public class PerformShutdownResponse
{
	[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
	public string Error { get; set; }

	public int? ProcessId { get; set; }
}
