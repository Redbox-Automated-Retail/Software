using System;

namespace Redbox.Rental.Model.Personalization;

public interface IExistingSubscription
{
	string CustomerSubscriptionId { get; set; }

	string SubscriptionTier { get; set; }

	string SubscriptionId { get; set; }

	string SubscriptionPlanId { get; set; }

	string CampaignId { get; set; }

	DateTime? EndDate { get; set; }

	bool HasSubscriptionEnded { get; }

	SubscriptionPlanStatus Status { get; set; }

	BenefitUsageList BenefitUsages { get; set; }
}
