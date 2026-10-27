using System;
using KioskClientService.API.Ads;

namespace KioskClientService.API.Services.Analytics.Vistar.Request;

public class ProofOfPlayAnalyticsRequest : HttpAnalyticsRequest
{
	public int DisplayedMilliseconds { get; set; }

	public DateTime DisplayedTime { get; set; }

	public AdLocation AdLocation { get; set; }
}
