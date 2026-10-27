using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.Rental.Services.App;

public class APIResponse<T>
{
	private LogHelper _logger;

	public HttpRequestMessage HttpRequest { get; set; }

	public HttpResponseMessage HttpResponse { get; set; }

	public T Response { get; set; }

	public string RequestContent { get; set; }

	public string ResponseContent { get; set; }

	public int? ResponseTime { get; set; }

	public int ResponseLength { get; set; }

	public string HostName => Environment.MachineName;

	public HttpStatusCode StatusCode { get; set; }

	public bool IsSuccessStatusCode
	{
		get
		{
			if (HttpResponse == null)
			{
				return false;
			}
			if (StatusCode >= HttpStatusCode.OK)
			{
				return StatusCode <= (HttpStatusCode)299;
			}
			return false;
		}
	}

	public Exception Exception { get; set; }

	public List<Error> Errors { get; set; } = new List<Error>();

	public APIResponse(HttpRequestMessage request, LogHelper logger)
	{
		HttpRequest = request;
		_logger = logger;
	}

	public void SetAPIResponse(HttpResponseMessage responseMessage, string content, int requestTime)
	{
		HttpResponse = responseMessage;
		Response = default(T);
		StatusCode = HttpResponse?.StatusCode ?? HttpStatusCode.InternalServerError;
		ResponseContent = content;
		ResponseTime = requestTime;
		ResponseLength = content?.Length ?? 0;
		try
		{
			if (!string.IsNullOrWhiteSpace(content) && IsSuccessStatusCode)
			{
				Response = JsonConvert.DeserializeObject<T>(content);
				ResponseContent = ((!(Response is IMessageScrub)) ? Response.ToJson() : (Response as IMessageScrub)?.Scrub()?.ToString());
			}
		}
		catch (Exception exception)
		{
			SetException(exception, requestTime);
		}
	}

	public void SetAPIResponse(HttpResponseMessage responseMessage, T content, int requestTime)
	{
		HttpResponse = responseMessage;
		Response = default(T);
		StatusCode = HttpResponse?.StatusCode ?? HttpStatusCode.InternalServerError;
		ResponseTime = requestTime;
		byte[] obj = content as byte[];
		ResponseLength = ((obj != null) ? obj.Length : 0);
		try
		{
			if (ResponseLength != 0)
			{
				Response = content;
				ResponseContent = Response.ToJson();
			}
		}
		catch (Exception exception)
		{
			SetException(exception, requestTime);
		}
	}

	public void SetException(Exception exception, int requestTime)
	{
		ResponseTime = requestTime;
		StatusCode = HttpStatusCode.InternalServerError;
		Exception = exception;
		string message = $"StatusCode: {HttpStatusCode.InternalServerError}, Message: {exception.GetFullMessage()}";
		Errors.Add(new Error
		{
			Code = StatusCode.ToString(),
			Message = message
		});
		_logger?.LogException(message, exception);
	}

	public void Log(bool logRequest, bool logResponse)
	{
		if (HttpRequest != null)
		{
			_logger?.LogInformation($"Request: {HttpRequest.Method},Uri: {HttpRequest.RequestUri}, Status: {StatusCode}({(int)StatusCode}), Time:{ResponseTime}, Length:{ResponseLength}");
			if (logRequest && RequestContent != null)
			{
				_logger?.LogInformation("Request Content: " + RequestContent);
			}
			if (logResponse && ResponseContent != null)
			{
				_logger?.LogInformation("Response Content: " + ResponseContent);
			}
		}
	}
}
