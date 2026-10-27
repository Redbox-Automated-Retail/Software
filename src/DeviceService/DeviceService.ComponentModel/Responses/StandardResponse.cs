using System;
using System.Collections.Generic;
using System.Net;

namespace DeviceService.ComponentModel.Responses;

public class StandardResponse
{
	public bool Success { get; set; }

	public IList<Error> Errors { get; set; }

	public string HostName => Environment.MachineName;

	public int StatusCode { get; set; }

	public StandardResponse()
	{
		Errors = new List<Error>();
	}

	protected StandardResponse(int statusCode)
	{
		StatusCode = statusCode;
		Errors = new List<Error>();
	}

	public StandardResponse(Exception e)
	{
		HttpStatusCode httpStatusCode = (HttpStatusCode)(StatusCode = 500);
		Errors = new List<Error>
		{
			new Error
			{
				Code = httpStatusCode.ToString(),
				Message = e.Message
			}
		};
	}
}
