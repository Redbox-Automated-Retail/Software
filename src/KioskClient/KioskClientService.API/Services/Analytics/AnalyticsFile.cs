using System.Collections.Generic;

namespace KioskClientService.API.Services.Analytics;

public class AnalyticsFile<T>
{
	public bool Sent { get; set; }

	public List<T> Requests { get; set; }
}
