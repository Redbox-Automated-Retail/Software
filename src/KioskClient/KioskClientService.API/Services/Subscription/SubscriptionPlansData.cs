using System;
using System.Collections.Generic;
using KioskClientService.API.Services.App;
using KioskClientService.API.Services.ProxyApi;

namespace KioskClientService.API.Services.Subscription;

public class SubscriptionPlansData : IPersistentData
{
	public bool Active { get; set; }

	public string BrowseStarzSubscriptionPlan { get; set; }

	public string MyBagStarzSubscriptionPlan { get; set; }

	public List<SubscriptionPlan> SubscriptionPlans { get; set; } = new List<SubscriptionPlan>();

	public List<RedboxPlusPromoCampaign> RedboxPlusPromoCampaigns { get; set; } = new List<RedboxPlusPromoCampaign>();

	public void CopyFrom(GetProxySubscriptionPlanResponse p)
	{
		SubscriptionPlans.Clear();
		if (p != null && p.SubscriptionPlans != null)
		{
			p.SubscriptionPlans.ForEach(delegate(SubscriptionPlan s)
			{
				SubscriptionPlans.Add(s);
			});
		}
	}

	public void CopyFrom(GetProxyRedboxPlusPromoCampaignsResponse p)
	{
		RedboxPlusPromoCampaigns.Clear();
		if (p != null && p.RedboxPlusPromoCampaigns != null)
		{
			p.RedboxPlusPromoCampaigns.ForEach(delegate(RedboxPlusPromoCampaign s)
			{
				RedboxPlusPromoCampaigns.Add(s);
			});
		}
	}

	public void RemoveNonActive()
	{
		List<SubscriptionPlan> list = new List<SubscriptionPlan>();
		DateTime dt = DateTime.Now;
		SubscriptionPlans.ForEach(delegate(SubscriptionPlan plan)
		{
			if (plan.EffectiveStartDate <= dt && plan.EffectiveEndDate > dt)
			{
				list.Add(plan);
			}
		});
		SubscriptionPlans.Clear();
		list.ForEach(delegate(SubscriptionPlan plan)
		{
			SubscriptionPlans.Add(plan);
		});
	}
}
