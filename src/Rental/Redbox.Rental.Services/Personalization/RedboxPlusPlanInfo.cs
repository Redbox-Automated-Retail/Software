using System;
using Redbox.Rental.Model.KioskClientService.Subscriptions;
using Redbox.Rental.Model.Personalization;

namespace Redbox.Rental.Services.Personalization;

public class RedboxPlusPlanInfo : IRedboxPlusPlanInfo
{
	public string PlanId { get; set; }

	public RedboxPlusSubscriptionTier SubscriptionTier { get; set; }

	public SubscriptionPlanStatus SubscriptionStatus { get; set; }

	public bool IsEarnPointsFor1ExtraNightAvailable { get; set; }

	public DateTime? SubscriptionEndDate { get; set; }

	public bool HasSubscriptionEnded
	{
		get
		{
			if (SubscriptionEndDate.HasValue)
			{
				return SubscriptionEndDate.Value < DateTime.UtcNow;
			}
			return false;
		}
	}
}
