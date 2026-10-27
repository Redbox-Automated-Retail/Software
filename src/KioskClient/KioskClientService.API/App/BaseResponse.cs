using System;
using System.Collections.Generic;
using KioskClientService.API.Services.HttpQueue;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace KioskClientService.API.App;

public abstract class BaseResponse : IMessageScrub
{
	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
	public bool Success { get; set; }

	public List<Error> Errors { get; set; } = new List<Error>();

	[JsonProperty(Required = Required.DisallowNull)]
	public string HostName => Environment.MachineName;

	public int StatusCode { get; set; }

	public int? ResponseTime { get; set; }

	protected BaseResponse(int statusCode)
	{
		StatusCode = statusCode;
	}

	public ObjectResult ToObjectResult()
	{
		return new ObjectResult(this)
		{
			StatusCode = StatusCode
		};
	}

	public ObjectResult ToObjectResult(int statusCode)
	{
		return new ObjectResult(this)
		{
			StatusCode = statusCode
		};
	}

	public virtual object Scrub()
	{
		return this.ToJsonIndented();
	}

	public virtual void ScrubData()
	{
	}

	internal void CheckAndReplay(ILogger logger, Guid messageId, HttpRequest source, IHttpQueueService queueService)
	{
		if (messageId == default(Guid))
		{
			logger?.LogInformation("BaseResponse.CheckAndReplay - messageId is not set; not replaying");
			return;
		}
		bool flag;
		if (StatusCode == 200)
		{
			flag = false;
		}
		else if (StatusCode == 400)
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
				logger?.LogError($"BaseResponse.CheckAndReplay - unable to replay message id: {messageId}!");
			}
		}
		else
		{
			queueService.Finalize(request);
		}
	}
}
