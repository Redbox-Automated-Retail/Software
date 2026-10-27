using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace UpdateClientService.API.Services.KioskEngine;

public class PerformShutdownResponse
{
	[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
	public string Error { get; set; }

	public int? ProcessId { get; set; }

	public ObjectResult ToObjectResult()
	{
		return new ObjectResult(this)
		{
			StatusCode = (string.IsNullOrWhiteSpace(Error) ? 200 : 500)
		};
	}
}
