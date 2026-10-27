using System;
using Microsoft.AspNetCore.Builder;
using Redbox.NetCore.Middleware.Filter;
using Redbox.NetCore.Middleware.Middleware;

namespace Redbox.NetCore.Middleware.Extensions;

public static class ApplicationBuilderExtensions
{
	public static IApplicationBuilder UseActivityIdMiddleware(this IApplicationBuilder app)
	{
		return app.UseMiddleware<ActivityIdMiddleware>(Array.Empty<object>());
	}

	public static IApplicationBuilder UseMetricsActionFilterExtensions(this IApplicationBuilder builder)
	{
		return builder.UseMiddleware<MetricsActionFilter>(Array.Empty<object>());
	}

	public static IApplicationBuilder UseKioskIdMiddleWare(this IApplicationBuilder builder)
	{
		return builder.UseMiddleware<KioskIdMiddleware>(Array.Empty<object>());
	}

	public static IApplicationBuilder UseSessionIdMiddleware(this IApplicationBuilder app)
	{
		return app.UseMiddleware<SessionIdMiddleware>(Array.Empty<object>());
	}

	public static IApplicationBuilder UseGrafanaMetricsMiddleware(this IApplicationBuilder builder)
	{
		return builder.UseMiddleware<GrafanaMetricsMiddleware>(Array.Empty<object>()).UseMiddleware<ExceptionMiddleware>(Array.Empty<object>());
	}

	public static void AddCompressedRequestPayloadMiddleware(this IApplicationBuilder app)
	{
		app.UseMiddleware<CompressedRequestPayloadMiddleware>(Array.Empty<object>());
	}
}
