using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using Redbox.NetCore.Middleware.Abstractions;
using Redbox.NetCore.Middleware.Extensions;
using Redbox.NetCore.Middleware.Http;

namespace Redbox.NetCore.Middleware.Middleware;

public class ExceptionMiddleware : IMiddleware
{
	private Stream _responseStream;

	private readonly IHostingEnvironment _env;

	private readonly ILogger<ExceptionMiddleware> _logger;

	private readonly IGrafanaMetrics _metrics;

	public ExceptionMiddleware(IHostingEnvironment env, ILogger<ExceptionMiddleware> logger, IGrafanaMetrics metrics)
	{
		_env = env;
		_logger = logger;
		_metrics = metrics;
	}

	public async Task InvokeAsync(HttpContext context, RequestDelegate next)
	{
		_responseStream = context.Response.Body;
		try
		{
			await next(context);
		}
		catch (Exception ex)
		{
			if (!context.Response.HasStarted)
			{
				await HandleExceptionAsync(context, ex);
				return;
			}
			_logger.LogError(ex, "Exception: " + ex.GetType().Name + " RequestUrl: " + context.Request.GetDisplayUrl());
			_metrics.CaptureError(ex);
		}
	}

	private async Task HandleExceptionAsync(HttpContext context, Exception error)
	{
		context.Response.ContentType = "application/json";
		context.Response.StatusCode = 500;
		Guid errorId = Guid.NewGuid();
		_metrics.CaptureError(error);
		string requestBody = await context.Request.ReadBodyAsync();
		await WriteErrorAsync(context, HttpStatusCode.InternalServerError, errorId, $"Error Reference {errorId}", error.GetFullMessage());
		_logger.LogError(error, $"Reference: {errorId} Exception: {error.GetType().Name} RequestUrl: {context.Request.GetDisplayUrl()} RequestBody: {requestBody}");
	}

	private async Task WriteErrorAsync(HttpContext context, HttpStatusCode statusCode, Guid errorId, string prodMessage, string devMessage = "")
	{
		context.Response.Body = _responseStream;
		context.Response.StatusCode = (int)statusCode;
		string activityId = context.GetActivityId();
		string kioskId = context.GetKioskId();
		string message = prodMessage;
		if (_env.IsDevelopment() && !string.IsNullOrWhiteSpace(devMessage))
		{
			message = devMessage;
		}
		if (_responseStream != null && _responseStream.CanWrite && !context.Response.HasStarted)
		{
			try
			{
				context.Response.Body = _responseStream;
				context.Response.StatusCode = (int)statusCode;
				await context.Response.WriteAsync(new Error
				{
					Code = statusCode.ToString(),
					Message = message,
					Id = errorId,
					ActivityId = activityId,
					KioskId = kioskId
				}.ToJson());
			}
			catch (ObjectDisposedException)
			{
			}
		}
	}
}
