using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using Newtonsoft.Json;

namespace Redbox.KioskEngine.ComponentModel;

public class ApiServiceBaseResponse<T>
{
	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
	[Required]
	public bool Success { get; set; }

	public IList<ResponseError> Errors { get; set; } = new List<ResponseError>();

	[JsonProperty(Required = Required.DisallowNull)]
	public string HostName => Environment.MachineName;

	[Required]
	public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;

	public int? ResponseTime { get; set; }

	public T Content { get; set; }

	public void SetResponseTime(int? responseTime)
	{
		ResponseTime = responseTime;
	}

	public ApiServiceBaseResponse(HttpStatusCode statusCode)
	{
		StatusCode = statusCode;
	}

	public ApiServiceBaseResponse(int statusCode)
	{
		StatusCode = (HttpStatusCode)statusCode;
	}

	public ApiServiceBaseResponse()
	{
	}
}
