using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Redbox.NetCore.Middleware.Abstractions;

namespace Redbox.NetCore.Middleware.Filter;

internal class MetricsActionFilter : IAsyncActionFilter, IFilterMetadata
{
	private readonly IApplicationMetrics _metrics;

	public MetricsActionFilter(IApplicationMetrics metrics)
	{
		_metrics = metrics;
	}

	public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
	{
		ControllerActionDescriptor descriptor = (ControllerActionDescriptor)context.ActionDescriptor;
		Stopwatch requestTimer = Stopwatch.StartNew();
		await next();
		requestTimer.Stop();
		Task.Run(() => _metrics.CaptureEndpointExecutionAsync(descriptor.ControllerName, descriptor.ActionName, requestTimer.ElapsedMilliseconds, (HttpStatusCode)context.HttpContext.Response.StatusCode));
	}
}
