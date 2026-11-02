using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace Redbox.NetCore.Middleware.Middleware;

internal class KioskIdMiddleware
{
	private const string KioskIdContextKey = "KioskId";

	private readonly RequestDelegate _next;

	private readonly ILogger _logger;

	public KioskIdMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
	{
		_next = next;
		_logger = loggerFactory.CreateLogger<KioskIdMiddleware>();
	}

	public async Task Invoke(HttpContext context)
	{
		string? headerName = context.Request.Headers.Keys.FirstOrDefault((string k) => string.Equals(k, "x-redbox-kioskid", StringComparison.OrdinalIgnoreCase));
		if (string.IsNullOrWhiteSpace(headerName))
		{
			_logger.LogDebug("KioskId header invalid or missing");
			await _next(context);
			return;
		}
		string? headerValue = context.Request.Headers[headerName];
		_logger.LogDebug("KioskId header found: {kioskId}", headerValue);
		using (LogContext.PushProperty("KioskId", headerValue))
		{
			await _next(context);
		}
	}
}
