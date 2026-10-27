using System.Collections.Generic;

namespace Redbox.Rental.Model.Personalization;

public class SubscriptionData : ISubscriptionData
{
	public bool SubscriptionServiceIsDown { get; set; }

	public List<string> EligibleSubscriptionPlanIds { get; set; }

	public List<IExistingSubscription> ExistingSubscriptions { get; set; }

	public string TempPassword { get; set; }

	public SubscriptionData()
	{
		EligibleSubscriptionPlanIds = new List<string>();
		ExistingSubscriptions = new List<IExistingSubscription>();
	}

	public bool IsEligibleForSubscription(string subscriptionPlanId)
	{
		if (EligibleSubscriptionPlanIds != null)
		{
			return EligibleSubscriptionPlanIds.Contains(subscriptionPlanId);
		}
		return false;
	}
}
