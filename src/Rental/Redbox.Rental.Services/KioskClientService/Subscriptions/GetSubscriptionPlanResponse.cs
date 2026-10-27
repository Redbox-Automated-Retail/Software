using System.Collections.Generic;
using Redbox.Rental.Model;

namespace Redbox.Rental.Services.KioskClientService.Subscriptions;

public class GetSubscriptionPlanResponse : BaseResponse
{
	public bool Active { get; set; }

	public string BrowseStarzSubscriptionPlan { get; set; }

	public string MyBagStarzSubscriptionPlan { get; set; }

	public List<SubscriptionPlan> SubscriptionPlans { get; set; }

	public List<RedboxPlusPromoCampaign> RedboxPlusPromoCampaigns { get; set; }
}
