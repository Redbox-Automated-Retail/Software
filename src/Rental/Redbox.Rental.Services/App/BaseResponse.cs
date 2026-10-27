using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Redbox.Rental.Services.App;

public abstract class BaseResponse
{
	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
	public bool Success { get; set; }

	public IList<Error> Errors { get; set; } = new List<Error>();

	[JsonProperty(Required = Required.DisallowNull)]
	public string HostName => Environment.MachineName;

	public int StatusCode { get; set; }

	public int? ResponseTime { get; set; }

	protected BaseResponse(int statusCode)
	{
		StatusCode = statusCode;
	}
}
