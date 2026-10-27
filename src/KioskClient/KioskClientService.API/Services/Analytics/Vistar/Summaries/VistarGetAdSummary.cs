using System.Collections.Generic;

namespace KioskClientService.API.Services.Analytics.Vistar.Summaries;

public class VistarGetAdSummary : HttpSummary
{
	public Dictionary<string, double?> MedianResponseTimeByLocation { get; set; }

	public Dictionary<string, int?> MaxResponseTimeByLocation { get; set; }

	public Dictionary<string, double?> SuccessRateByLocation { get; set; }
}
