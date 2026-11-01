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
			bool needsSeparator = false;
			foreach (KeyValuePair<string, object> actionArgument in context.ActionArguments)
			{
				if (needsSeparator)
				{
					stringBuilder.Append(',');
				}
				stringBuilder.Append(actionArgument.Key).Append(':');
				stringBuilder.Append((actionArgument.Value is IMessageScrub) ? (actionArgument.Value as IMessageScrub).Scrub() : actionArgument.Value);
				needsSeparator = true;
			}
		}
		_logger.LogInformation(stringBuilder.ToString());
		Stopwatch requestTimer = Stopwatch.StartNew();
		ActionExecutedContext actionExecutedContext = await next();
		StringBuilder responseText = new StringBuilder();
		if (actionExecutedContext != null && actionExecutedContext.Result != null)
		{
			if (actionExecutedContext.Result is ObjectResult)
			{
				ObjectResult objectResult = actionExecutedContext.Result as ObjectResult;
				if (objectResult.Value != null && objectResult.Value is IMessageScrub)
				{
					responseText.Append($"{(objectResult.Value as IMessageScrub).Scrub()}");
				}
				else
				{
					responseText.Append($"{objectResult.StatusCode}");
				}
			}
			else if (actionExecutedContext.Result is ContentResult)
			{
				ContentResult contentResult = actionExecutedContext.Result as ContentResult;
				responseText.Append(contentResult.Content);
			}
		}
		else
		{
			responseText.Append("none");
		}
		requestTimer.Stop();
		_logger.LogInformation($"{controllerName}.{actionName}, response={responseText}");
		_logger.LogInformation($"END: {controllerName}.{actionName}, statuscode={(HttpStatusCode)context.HttpContext.Response.StatusCode}, elapsed={requestTimer.ElapsedMilliseconds}");
	}
}
