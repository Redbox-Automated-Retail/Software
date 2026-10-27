using System;
using Redbox.Rental.Model.KioskClientService.Subscriptions;

namespace Redbox.Rental.Model.Personalization;

public interface IRedboxPlusPlanInfo
{
	string PlanId { get; set; }

	RedboxPlusSubscriptionTier SubscriptionTier { get; set; }

	SubscriptionPlanStatus SubscriptionStatus { get; set; }

	bool IsEarnPointsFor1ExtraNightAvailable { get; set; }

	DateTime? SubscriptionEndDate { get; set; }

	bool HasSubscriptionEnded { get; }
}
