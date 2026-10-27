using KioskClientService.API.Ads;

namespace KioskClientService.API.Services.Analytics.Vistar.Request;

public class GetAdAnalyticsRequest : HttpAnalyticsRequest
{
	public AdLocation AdLocation { get; set; }
}
