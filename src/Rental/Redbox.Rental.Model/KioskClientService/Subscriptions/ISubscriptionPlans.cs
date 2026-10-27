using System.Collections.Generic;
using Redbox.Rental.Model.KioskProduct;

namespace Redbox.Rental.Model.KioskClientService.Subscriptions;

public interface ISubscriptionPlans
{
	bool Active { get; set; }

	List<ISubscriptionProduct> SubscriptionProducts { get; set; }

	List<IRedboxPlusPromoCampaign> RedboxPlusPromoCampaigns { get; set; }
}
