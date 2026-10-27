using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API.App;

public class LoggingActionFilter : IAsyncActionFilter, IFilterMetadata
{
	private readonly ILogger<LoggingActionFilter> _logger;

	public LoggingActionFilter(ILogger<LoggingActionFilter> logger)
	{
		_logger = logger;
	}

	public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
	{
		ControllerActionDescriptor controllerActionDescriptor = (ControllerActionDescriptor)context.ActionDescriptor;
		string controllerName = controllerActionDescriptor.ControllerName.Trim();
		string actionName = controllerActionDescriptor.ActionName.Trim();
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("START: ").Append(controllerName).Append('.')
			.Append(actionName);
		if (context.ActionArguments.Any())
		{
			stringBuilder.Append(", ");
			bool flag = false;
			foreach (KeyValuePair<string, object> actionArgument in context.ActionArguments)
			{
				if (flag)
				{
					stringBuilder.Append(',');
				}
				stringBuilder.Append(actionArgument.Key).Append(':');
				stringBuilder.Append((actionArgument.Value is IMessageScrub) ? (actionArgument.Value as IMessageScrub).Scrub() : actionArgument.Value);
				flag = true;
			}
		}
		_logger.LogInformation(stringBuilder.ToString());
		Stopwatch requestTimer = Stopwatch.StartNew();
		ActionExecutedContext actionExecutedContext = await next();
		StringBuilder stringBuilder2 = new StringBuilder();
		if (actionExecutedContext != null && actionExecutedContext.Result != null)
		{
			if (actionExecutedContext.Result is ObjectResult)
			{
				ObjectResult objectResult = actionExecutedContext.Result as ObjectResult;
				if (objectResult.Value != null && objectResult.Value is IMessageScrub)
				{
					stringBuilder2.Append($"{(objectResult.Value as IMessageScrub).Scrub()}");
				}
				else
				{
					stringBuilder2.Append($"{objectResult.StatusCode}");
				}
			}
			else if (actionExecutedContext.Result is ContentResult)
			{
				ContentResult contentResult = actionExecutedContext.Result as ContentResult;
				stringBuilder2.Append(contentResult.Content);
			}
		}
		else
		{
			stringBuilder2.Append("none");
		}
		requestTimer.Stop();
		_logger.LogInformation($"{controllerName}.{actionName}, response={stringBuilder2}");
		_logger.LogInformation($"END: {controllerName}.{actionName}, statuscode={(HttpStatusCode)context.HttpContext.Response.StatusCode}, elapsed={requestTimer.ElapsedMilliseconds}");
	}
}
