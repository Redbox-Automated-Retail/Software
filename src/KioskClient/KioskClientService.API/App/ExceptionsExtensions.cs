using System;
using Microsoft.AspNetCore.Builder;

namespace KioskClientService.API.App;

public static class ExceptionsExtensions
{
	public static void ConfigureExceptionMiddleware(this IApplicationBuilder app)
	{
		app.UseMiddleware<ExceptionMiddleware>(Array.Empty<object>());
	}
}
