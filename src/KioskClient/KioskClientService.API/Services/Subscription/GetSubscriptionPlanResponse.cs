using System.Collections.Generic;
using KioskClientService.API.App;
using KioskClientService.API.Services.ProxyApi;

namespace KioskClientService.API.Services.Subscription;

public class GetSubscriptionPlanResponse : BaseResponse, IMessageScrub
{
	public bool Active { get; set; }

	public string BrowseStarzSubscriptionPlan { get; set; }

	public string MyBagStarzSubscriptionPlan { get; set; }

	public List<SubscriptionPlan> SubscriptionPlans { get; } = new List<SubscriptionPlan>();

	public List<RedboxPlusPromoCampaign> RedboxPlusPromoCampaigns { get; } = new List<RedboxPlusPromoCampaign>();

	public GetSubscriptionPlanResponse()
		: base(200)
	{
		base.Success = true;
	}

	public override object Scrub()
	{
		return $"Active: {Active}, SubscriptionCount: {SubscriptionPlans?.Count}";
	}

	public void CopyFrom(SubscriptionPlansData p)
	{
		SubscriptionPlans.Clear();
		if (p == null || p.SubscriptionPlans == null)
		{
			return;
		}
		Active = p.Active;
		BrowseStarzSubscriptionPlan = p.BrowseStarzSubscriptionPlan;
		MyBagStarzSubscriptionPlan = p.MyBagStarzSubscriptionPlan;
		p.SubscriptionPlans.ForEach(delegate(SubscriptionPlan s)
		{
			SubscriptionPlans.Add(s);
		});
		if (p?.RedboxPlusPromoCampaigns != null)
		{
			p.RedboxPlusPromoCampaigns.ForEach(delegate(RedboxPlusPromoCampaign r)
			{
				RedboxPlusPromoCampaigns.Add(r);
			});
		}
	}
}
