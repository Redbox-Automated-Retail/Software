using System;

namespace Redbox.UpdateService.Client;

public class SubscriptionStatus
{
	public DateTime ModifiedOn { get; set; }

	public SubscriptionState State { get; set; }
}
