using System.Collections.Generic;

namespace KioskClientService.API.Services.Analytics.Vistar.Summaries;

public class VistarProofOfPlaySummary : HttpSummary
{
	public double? MedianDisplayedDuration { get; set; }

	public int MaxDisplayedDuration { get; set; }

	public int MinDisplayedDuration { get; set; }

	public Dictionary<string, double?> MedianResponseTimeByLocation { get; set; }

	public Dictionary<string, int?> MaxResponseTimeByLocation { get; set; }

	public Dictionary<string, double?> SuccessRateByLocation { get; set; }
}
