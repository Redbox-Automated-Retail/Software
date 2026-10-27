using System;
using Microsoft.AspNetCore.Builder;

namespace Redbox.NetCore.Middleware.Middleware;

internal static class GrafanaMetricsBuilder
{
	public static IApplicationBuilder UseGrafanaMetrics(this IApplicationBuilder builder)
	{
		return builder.UseMiddleware<GrafanaMetricsMiddleware>(Array.Empty<object>());
	}
}
