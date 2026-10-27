using System.Collections.Generic;
using System.Linq;
using KioskClientService.API.App;

namespace KioskClientService.API.Services.ProxyApi;

public class GetProxySubscriptionPlanResponse : BaseResponse, IMessageScrub
{
	public List<SubscriptionPlan> SubscriptionPlans { get; } = new List<SubscriptionPlan>();

	public GetProxySubscriptionPlanResponse()
		: base(200)
	{
		base.Success = true;
	}

	public override object Scrub()
	{
		return SubscriptionPlans.Select((SubscriptionPlan s) => s.Id).ToJson();
	}
}
