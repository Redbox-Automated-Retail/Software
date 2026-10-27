using System;
using Microsoft.AspNetCore.Builder;

namespace Redbox.NetCore.Middleware.Middleware;

public static class ExceptionBuilder
{
	public static IApplicationBuilder UseExceptionMiddleware(this IApplicationBuilder builder)
	{
		return builder.UseMiddleware<ExceptionMiddleware>(Array.Empty<object>());
	}
}
