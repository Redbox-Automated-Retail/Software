using System;

namespace KioskClientService.API.Services.Analytics;

public class BaseAnalyticsRequest
{
	public DateTimeOffset LocalTime = DateTimeOffset.Now;

	public long KioskId { get; set; }
}
