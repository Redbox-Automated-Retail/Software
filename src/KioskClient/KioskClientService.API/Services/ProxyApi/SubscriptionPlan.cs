using System;
using System.Collections.Generic;

namespace KioskClientService.API.Services.ProxyApi;

public class SubscriptionPlan
{
	public string Id { get; set; }

	public string Description { get; set; }

	public string SubscriptionId { get; set; }

	public DateTimeOffset EffectiveStartDate { get; set; }

	public DateTimeOffset EffectiveEndDate { get; set; }

	public bool IsPromotional { get; set; }

	public ICollection<SubscriptionPlanItem> PlanItems { get; set; }
}
