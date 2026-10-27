using KioskClientService.API.Ads;
using KioskClientService.API.Services.Ads;

namespace KioskClientService.API.Services.Analytics.Vistar.Request;

public class ExpireAnalyticsRequest : HttpAnalyticsRequest
{
	public bool? Requested { get; set; }

	public AdExpirationSource AdExpirationSource { get; set; }

	public AdLocation AdLocation { get; set; }
}
