using System;

namespace KioskClientService.API.Services.ProxyApi;

public class KioskSubscription
{
	public string CustomerSubscriptionId { get; set; }

	public string SubscriptionId { get; set; }

	public string SubscriptionPlanId { get; set; }

	public string CampaignId { get; set; }

	public string SubscriptionTier { get; set; }

	public DateTime? EndDate { get; set; }

	public CustomerSubscriptionPlanStatus status { get; set; }

	public KioskBenefitUsageList BenefitUsages { get; set; }
}
