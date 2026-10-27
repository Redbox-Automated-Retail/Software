using System.Collections.ObjectModel;

namespace Redbox.Rental.Services.KioskClientService.Subscriptions;

public class SubscriptionPlan
{
	public string Id { get; set; }

	public string Name { get; set; }

	public string Description { get; set; }

	public string SubscriptionId { get; set; }

	public bool IsPromotional { get; set; }

	public Collection<SubscriptionPlanItem> PlanItems { get; set; }
}
