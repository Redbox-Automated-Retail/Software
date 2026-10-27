using System;

namespace KioskClientService.API.Services.Analytics;

public class Summary
{
	public long KioskId { get; set; }

	public AnalyticsType Type { get; set; }

	public DateTime SummaryDate { get; set; }

	public int Requests { get; set; }
}
