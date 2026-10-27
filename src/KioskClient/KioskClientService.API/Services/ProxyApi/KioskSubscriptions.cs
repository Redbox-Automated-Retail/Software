using System.Collections.Generic;

namespace KioskClientService.API.Services.ProxyApi;

public class KioskSubscriptions
{
	public List<string> EligibleSubscriptionPlanIds { get; set; }

	public List<KioskSubscription> ExistingSubscriptions { get; set; }
}
