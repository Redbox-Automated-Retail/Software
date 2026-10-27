using System;

namespace Redbox.Rental.Model.Personalization;

public class ExistingSubscription : IExistingSubscription
{
	public string CustomerSubscriptionId { get; set; }

	public string SubscriptionId { get; set; }

	public string SubscriptionPlanId { get; set; }

	public string CampaignId { get; set; }

	public string SubscriptionTier { get; set; }

	public DateTime? EndDate { get; set; }

	public SubscriptionPlanStatus Status { get; set; }

	public BenefitUsageList BenefitUsages { get; set; }

	public bool HasSubscriptionEnded
	{
		get
		{
			if (EndDate.HasValue)
			{
				return EndDate.Value < DateTime.UtcNow;
			}
			return false;
		}
	}
}
