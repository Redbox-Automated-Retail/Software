using System;
using System.Net;
using KioskClientService.API.Services.HttpQueue;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace KioskClientService.API.App;

public class ApiBaseResponse : IMessageScrub
{
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

	internal void CheckAndReplay(ILogger logger, Guid messageId, HttpRequest source, IHttpQueueService queueService)
	{
		if (messageId == default(Guid))
		{
			logger?.LogInformation("BaseResponse.CheckAndReplay - messageId is not set; not replaying");
			return;
		}
		bool flag;
		if (StatusCode == HttpStatusCode.OK)
		{
			flag = false;
		}
		else if (StatusCode == HttpStatusCode.BadRequest)
		{
			flag = false;
		}
		else
		{
			flag = this is IReplayableApiCall replayableApiCall && replayableApiCall.Replayable;
		}
		HttpQueueServiceRequest request = new HttpQueueServiceRequest
		{
			Id = messageId,
			Request = HttpRequestMetadata.FromHttpRequest(source)
		};
		if (flag)
		{
			if (!queueService.Enqueue(request))
			{
				logger?.LogError($"ApiBaseResponse.CheckAndReplay - unable to replay message id: {messageId}!");
			}
		}
		else
		{
			queueService.Finalize(request);
		}
	}

	public virtual object Scrub()
	{
		return GetType().Name;
	}
}
