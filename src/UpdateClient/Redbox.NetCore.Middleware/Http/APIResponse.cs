using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Redbox.NetCore.Logging.Abstractions;
using Redbox.NetCore.Middleware.Extensions;

namespace Redbox.NetCore.Middleware.Http;

public class APIResponse<T>
{
	private readonly ILogger _logger;

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

	public APIResponse(HttpRequestMessage request, ILogger logger)
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
		ResponseLength = ((content is byte[] array) ? array.Length : 0);
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
		_logger?.LogError(exception, message);
	}

	public void Log(bool logRequest, bool logResponse)
	{
		if (HttpRequest != null)
		{
			string text = (logRequest ? HttpRequest.RequestUri.ToString() : HttpRequest.RequestUri.GetLeftPart(UriPartial.Path));
			_logger?.LogInformation($"Request: {HttpRequest.Method}, Uri: {text}, Status: {StatusCode}({(int)StatusCode}), Time:{ResponseTime}, Length:{ResponseLength}");
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

	public object Scrub()
	{
		if (Response == null)
		{
			return Exception?.Message;
		}
		return (Response as IMessageScrub)?.Scrub() ?? JsonConvert.SerializeObject(Response);
	}

	public string GetErrors()
	{
		if (string.IsNullOrWhiteSpace(ResponseContent) && !Errors.Any())
		{
			return $"ApiResponse.ResponseContent and ApiResponse.Errors are empty. Request: {HttpRequest.Method},Uri: {HttpRequest.RequestUri}, Status: {StatusCode}({(int)StatusCode}), Time:{ResponseTime}, Length:{ResponseLength} HttpResponse StatusCode: {HttpResponse?.StatusCode} {HttpResponse?.ReasonPhrase}";
		}
		return "ResponseContent: " + ResponseContent + " . Errors: " + string.Join(',', Errors.Select((Error x) => x.Code + " - " + x.Message));
	}
}
