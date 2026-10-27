using System;
using Redbox.Core;
using Redbox.Rental.Model.KioskClientService.Subscriptions;

namespace Redbox.Rental.Services.KioskClientService.Subscriptions;

public class KioskClientServiceSubscriptions : BaseKioskClientService, IKioskClientServiceSubscriptions
{
	private const string _apiPath = "api/subscription";

	public ISubscriptionPlans GetSubscriptionPlans()
	{
		ISubscriptionPlans subscriptionPlans = null;
		WebResult<GetSubscriptionPlanResponse> webResult = ExecuteWebRequest<GetSubscriptionPlanResponse>(new Uri(base.KioskClientServiceBaseUrl + "/api/subscription/plans"), null, "GET", 20000);
		LogResponse("KioskClientServiceSubscriptions.GetSubscriptionPlans", webResult);
		if (webResult?.Response?.Success == true)
		{
			subscriptionPlans = new SubscriptionPlans(webResult.Response);
			LogHelper.Instance.Log("GetSubscriptionPlanResponse: " + webResult.Response.ToJson());
			if (subscriptionPlans != null && subscriptionPlans.RedboxPlusPromoCampaigns != null && subscriptionPlans.RedboxPlusPromoCampaigns.Count == 0)
			{
				subscriptionPlans.RedboxPlusPromoCampaigns.Add(new RedboxPlusPromoCampaign
				{
					PromoCampaignId = 9979,
					Tier = RedboxPlusSubscriptionTier.Tier1
				});
				subscriptionPlans.RedboxPlusPromoCampaigns.Add(new RedboxPlusPromoCampaign
				{
					PromoCampaignId = 16377,
					Tier = RedboxPlusSubscriptionTier.Tier1
				});
				subscriptionPlans.RedboxPlusPromoCampaigns.Add(new RedboxPlusPromoCampaign
				{
					PromoCampaignId = 9981,
					Tier = RedboxPlusSubscriptionTier.Tier2
				});
				subscriptionPlans.RedboxPlusPromoCampaigns.Add(new RedboxPlusPromoCampaign
				{
					PromoCampaignId = 16378,
					Tier = RedboxPlusSubscriptionTier.Tier2
				});
				LogHelper.Instance.Log("RedboxPlusPromoCampaigns.Count=0, added: " + subscriptionPlans.RedboxPlusPromoCampaigns.ToJson());
			}
		}
		return subscriptionPlans;
	}
}
