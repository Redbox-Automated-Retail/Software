using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;

namespace Redbox.NetCore.Middleware.Middleware;

internal class ValidateModelAttribute : ActionFilterAttribute
{
	private readonly ILogger<ValidateModelAttribute> _logger;

	public ValidateModelAttribute(ILogger<ValidateModelAttribute> logger)
	{
		_logger = logger;
	}

	public override void OnActionExecuting(ActionExecutingContext context)
	{
		if (context.ModelState.IsValid)
		{
			return;
		}
		ControllerActionDescriptor controllerActionDescriptor = (ControllerActionDescriptor)context.ActionDescriptor;
		List<string> list = context.ModelState.Values.SelectMany((ModelStateEntry v) => v.Errors.Select((ModelError e) => e.ErrorMessage)).ToList();
		_logger.LogError("{controllerName}.{actionName} request failed validation, errors: {@errors}", controllerActionDescriptor.ControllerName, controllerActionDescriptor.ActionName, list);
		context.Result = new BadRequestObjectResult(list);
	}
}
