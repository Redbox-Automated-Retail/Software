using System;
using KioskClientService.API.Ads;

namespace KioskClientService.API.Services.Analytics.Vistar.Request;

public class GetAdTimeoutEvent : BaseAnalyticsRequest
{
	public DateTime CreatedOn { get; set; }

	public AdLocation AdLocation { get; set; }

	public int TimeoutSetting { get; set; }
}
