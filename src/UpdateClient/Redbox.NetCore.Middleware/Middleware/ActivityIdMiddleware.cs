using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Redbox.NetCore.Middleware.Abstractions;
using Serilog.Context;

namespace Redbox.NetCore.Middleware.Middleware;

internal class ActivityIdMiddleware
{
	private const string ActivityIdContextKey = "ActivityId";

	private const string KioskIdContextKey = "KioskId";

	private readonly RequestDelegate _next;

	private readonly ILogger _logger;

	public ActivityIdMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
	{
		_next = next;
		_logger = loggerFactory.CreateLogger<ActivityIdMiddleware>();
	}

	public async Task Invoke(HttpContext context, IMiddlewareData data)
	{
		data.ActivityId = ScrapeHeader(context, "ActivityId", "x-redbox-activityid", Guid.NewGuid().ToString());
		data.KioskId = ScrapeHeader<long?>(context, "KioskId", "x-redbox-kioskid", null);
		using (LogContext.PushProperty("ActivityId", data.ActivityId))
		{
			using (LogContext.PushProperty("KioskId", data.KioskId))
			{
				await _next(context);
			}
		}
	}

	private T ScrapeHeader<T>(HttpContext context, string field, string redboxKey, T defaultValue)
	{
		string text = context.Request.Headers.Keys.FirstOrDefault((string k) => string.Equals(k, redboxKey, StringComparison.OrdinalIgnoreCase));
		T val;
		if (string.IsNullOrWhiteSpace(text))
		{
			val = defaultValue;
			_logger.LogDebug($"Generated new field: {field}, '{val}'");
		}
		else
		{
			try
			{
				val = (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(null, CultureInfo.InvariantCulture, context.Request.Headers[text]);
				_logger.LogDebug($"Found field: {field}, '{val}'");
			}
			catch (Exception exception)
			{
				val = defaultValue;
				_logger.LogError(exception, $"Exception converting found value for field: {field}, using default value: '{defaultValue}'");
			}
		}
		return val;
	}
}
