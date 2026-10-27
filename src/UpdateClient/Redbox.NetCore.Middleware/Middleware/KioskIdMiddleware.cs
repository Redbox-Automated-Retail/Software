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
		string text = context.Request.Headers.Keys.FirstOrDefault((string k) => string.Equals(k, "x-redbox-kioskid", StringComparison.OrdinalIgnoreCase));
		if (string.IsNullOrWhiteSpace(text))
		{
			_logger.LogDebug("KioskId header invalid or missing");
			await _next(context);
			return;
		}
		string text2 = context.Request.Headers[text];
		_logger.LogDebug("KioskId header found: {kioskId}", text2);
		using (LogContext.PushProperty("KioskId", text2))
		{
			await _next(context);
		}
	}
}
