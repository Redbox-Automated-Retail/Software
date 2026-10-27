using System;

namespace KioskClientService.API.Services.Analytics;

public static class BaseAnalyticsExtensions
{
	public static string ToTimestampTzString(this DateTimeOffset date)
	{
		return date.ToString("yyyy-MM-dd HH:mm:sszzz");
	}
}
