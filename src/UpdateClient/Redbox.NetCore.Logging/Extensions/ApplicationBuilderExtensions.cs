using System;
using Microsoft.AspNetCore.Builder;
using Redbox.NetCore.Logging.Filter;

namespace Redbox.NetCore.Logging.Extensions;

public static class ApplicationBuilderExtensions
{
	public static IApplicationBuilder UseMetricsActionFilterExtensions(this IApplicationBuilder builder)
	{
		return builder.UseMiddleware<LoggingActionFilter>(Array.Empty<object>());
	}
}
