using System.Collections.Generic;
using KioskClientService.API.Ads;

namespace KioskClientService.API.Services.Analytics.Vistar.Summaries;

public class GetAdTimeoutEventSummary : Summary
{
	public Dictionary<AdLocation, int> RequestsByAdLocation { get; set; }

	public Dictionary<int, int> RequestsByHour { get; set; }
}
