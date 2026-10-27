using System;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Server.Kestrel.Core.Features;

namespace Redbox.NetCore.Middleware.Attributes;

[AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
public class DisableRequestBodyDataRateAttribute : Attribute, IResourceFilter, IFilterMetadata
{
	public void OnResourceExecuted(ResourceExecutedContext context)
	{
	}

	public void OnResourceExecuting(ResourceExecutingContext context)
	{
		IHttpMinRequestBodyDataRateFeature httpMinRequestBodyDataRateFeature = context.HttpContext.Features.Get<IHttpMinRequestBodyDataRateFeature>();
		if (httpMinRequestBodyDataRateFeature != null)
		{
			httpMinRequestBodyDataRateFeature.MinDataRate = null;
		}
	}
}
