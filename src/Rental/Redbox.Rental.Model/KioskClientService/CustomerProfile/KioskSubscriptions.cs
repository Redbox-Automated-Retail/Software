using System.Collections.Generic;

namespace Redbox.Rental.Model.KioskClientService.CustomerProfile;

public class KioskSubscriptions
{
	public List<string> EligibleSubscriptionPlanIds { get; set; }

	public List<KioskSubscription> ExistingSubscriptions { get; set; }
}
