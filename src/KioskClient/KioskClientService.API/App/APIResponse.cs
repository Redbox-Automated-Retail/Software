using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using KioskClientService.API.Services.HttpQueue;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace KioskClientService.API.App;

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
			if (!string.IsNullOrEmpty(content) && !string.IsNullOrWhiteSpace(content))
			{
				Response = JsonConvert.DeserializeObject<T>(content);
				ResponseContent = ((!(Response is IMessageScrub)) ? JsonConvert.SerializeObject(Response, Formatting.None, new JsonSerializerSettings
				{
					NullValueHandling = NullValueHandling.Ignore
				}) : (Response as IMessageScrub)?.Scrub()?.ToString());
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
				ResponseContent = JsonConvert.SerializeObject(Response, Formatting.Indented, new JsonSerializerSettings
				{
					NullValueHandling = NullValueHandling.Ignore
				});
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
		string text = $"StatusCode: {HttpStatusCode.InternalServerError}, Message: {exception.Message}";
		if (exception.InnerException != null)
		{
			text = text + ", Inner: " + exception.InnerException.Message;
		}
		Errors.Add(new Error
		{
			Code = StatusCode.ToString(),
			Message = text
		});
		_logger?.LogError(exception, text);
	}

	public void Log(bool responseContentLoggingEnabled = true)
	{
		if (HttpRequest != null)
		{
			_logger?.LogInformation($"Request: {HttpRequest.Method}, Uri: {HttpRequest.RequestUri}, Status: {StatusCode}, Time:{ResponseTime}, Length:{ResponseLength}");
			if (responseContentLoggingEnabled && ResponseContent != null)
			{
				_logger?.LogInformation("Response Content: " + ResponseContent);
			}
		}
	}

	internal void CheckAndReplay(Guid messageId, HttpRequest source, IHttpQueueService queueService)
	{
		if (!typeof(IReplayableApiCall).IsAssignableFrom(typeof(T)))
		{
			return;
		}
		if (messageId == default(Guid))
		{
			_logger?.LogInformation("ApiResponse.CheckAndReplay - messageId is not set; not replaying");
			return;
		}
		bool flag;
		if (IsSuccessStatusCode)
		{
			flag = false;
		}
		else if (Response == null)
		{
			flag = true;
		}
		else
		{
			T response = Response;
			flag = response is IReplayableApiCall replayableApiCall && replayableApiCall.Replayable;
		}
		_logger?.LogInformation($"APIResponse.CheckAndReplay - id: {messageId}, api: {source.GetDisplayUrl()}, replay: {flag}");
		HttpQueueServiceRequest request = new HttpQueueServiceRequest
		{
			Id = messageId,
			Request = HttpRequestMetadata.FromHttpRequest(source)
		};
		if (flag)
		{
			if (!queueService.Enqueue(request))
			{
				_logger?.LogError($"ApiResponse.CheckAndReplay - unable to replay message id: {messageId}!");
			}
		}
		else
		{
			queueService.Finalize(request);
		}
	}
}
