using System;
using Redbox.Rental.Model.Personalization;

namespace Redbox.Rental.Services.Personalization;

public class RedboxPlusSession : IRedboxPlusSession
{
	private static readonly TimeSpan OneSecondBeforeMidnight = TimeSpan.FromDays(1.0) - TimeSpan.FromSeconds(1.0);

	public long SubscriptionId { get; set; }

	public IRedboxPlusPlanInfo RedboxPlusPlanInfo { get; set; }

	public bool HasActiveSubscription
	{
		get
		{
			if (RedboxPlusPlanInfo != null)
			{
				return !RedboxPlusPlanInfo.HasSubscriptionEnded;
			}
			return false;
		}
	}

	public TimeSpan RentalReturnTime
	{
		get
		{
			if (!HasActiveSubscription)
			{
				return TimeSpan.FromHours(21.0);
			}
			return OneSecondBeforeMidnight;
		}
	}
}
