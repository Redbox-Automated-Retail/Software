using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Redbox.NetCore.Logging.Abstractions;

namespace Redbox.NetCore.Logging.Filter;

internal class LoggingActionFilter : IAsyncActionFilter, IFilterMetadata
{
	private readonly ILogger<LoggingActionFilter> _logger;

	private readonly IConfiguration _configuration;

	public LoggingActionFilter(ILogger<LoggingActionFilter> logger, IConfiguration configuration)
	{
		_logger = logger;
		_configuration = configuration;
	}

	public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
	{
		ControllerActionDescriptor controllerActionDescriptor = (ControllerActionDescriptor)context.ActionDescriptor;
		string controllerName = controllerActionDescriptor.ControllerName;
		string actionName = controllerActionDescriptor.ActionName;
		StringBuilder scrubbedRequest = new StringBuilder();
		context.ActionArguments.ToList().ForEach(delegate(KeyValuePair<string, object> arg)
		{
			if (arg.Value is IMessageScrub)
			{
				scrubbedRequest.Append($",{arg.Key}:{(arg.Value as IMessageScrub).Scrub()}");
			}
			else
			{
				scrubbedRequest.Append($",{arg.Key}:{arg.Value}");
			}
		});
		_logger.LogInformation($"START: {controllerName}.{actionName} {scrubbedRequest}");
		Stopwatch requestTimer = Stopwatch.StartNew();
		ActionExecutedContext actionExecutedContext = await next();
		StringBuilder stringBuilder = new StringBuilder();
		if (actionExecutedContext?.Result != null)
		{
			if (actionExecutedContext.Result is OkObjectResult okObjectResult)
			{
				if (okObjectResult.Value is IMessageScrub messageScrub)
				{
					stringBuilder.Append($"{messageScrub.Scrub()}");
				}
				else
				{
					stringBuilder.Append($"{okObjectResult.StatusCode}");
				}
			}
			else if (actionExecutedContext.Result is ContentResult contentResult)
			{
				stringBuilder.Append(contentResult.Content);
			}
			else if (actionExecutedContext.Result is ObjectResult objectResult)
			{
				if (objectResult.Value is IMessageScrub messageScrub2)
				{
					stringBuilder.Append($"{messageScrub2.Scrub()}");
				}
				else
				{
					stringBuilder.Append($"{objectResult.StatusCode}");
				}
			}
			int? num = (int?)actionExecutedContext.Result.GetType().GetProperty("StatusCode")?.GetValue(actionExecutedContext.Result);
			if (num.HasValue)
			{
				context.HttpContext.Response.StatusCode = num.Value;
			}
		}
		else
		{
			stringBuilder.Append("none");
		}
		requestTimer.Stop();
		_logger.LogInformation($"{controllerName}.{actionName},response={stringBuilder}");
		_logger.LogInformation($"END: {controllerName}.{actionName},statuscode={(HttpStatusCode)context.HttpContext.Response.StatusCode},elapsed={requestTimer.ElapsedMilliseconds}");
	}
}
