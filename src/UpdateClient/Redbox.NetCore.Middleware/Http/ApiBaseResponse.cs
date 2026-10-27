using System;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Redbox.NetCore.Middleware.Http;

public class ApiBaseResponse
{
	[JsonIgnore]
	[Obsolete("Provided for backwards compatibility server side; change the server code to not set this property", false)]
	public bool Success { get; set; }

	[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
	public string Error { get; set; }

	[JsonIgnore]
	public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;

	public ApiBaseResponse()
	{
	}

	public ApiBaseResponse(HttpStatusCode statusCode)
	{
		StatusCode = statusCode;
	}

	public ObjectResult ToObjectResult()
	{
		return new ObjectResult(this)
		{
			StatusCode = (int)StatusCode
		};
	}
}
