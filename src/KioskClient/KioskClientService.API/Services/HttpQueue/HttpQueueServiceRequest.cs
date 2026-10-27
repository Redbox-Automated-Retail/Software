using System;
using Newtonsoft.Json;

namespace KioskClientService.API.Services.HttpQueue;

public class HttpQueueServiceRequest
{
	public Guid Id { get; set; }

	public int Retries { get; set; }

	public HttpRequestMetadata Request { get; set; }

	[JsonIgnore]
	public string Filename => $"{Id}.json";
}
