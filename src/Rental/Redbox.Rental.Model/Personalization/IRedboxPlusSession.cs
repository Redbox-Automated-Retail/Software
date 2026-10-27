using System;

namespace Redbox.Rental.Model.Personalization;

public interface IRedboxPlusSession
{
	long SubscriptionId { get; set; }

	IRedboxPlusPlanInfo RedboxPlusPlanInfo { get; set; }

	bool HasActiveSubscription { get; }

	TimeSpan RentalReturnTime { get; }
}
