using System;

namespace Redbox.Rental.Model.KioskClientService.Personalization;

public interface IPlanSubscriptionInfo
{
	SubscriptionStatus SubscriptionStatus { get; set; }

	int PlanId { get; set; }

	bool IsOneNightFreeRentalAvailable { get; set; }

	bool IsEarnPointsFor1ExtraNightAvailable { get; set; }

	DateTime? EnrollmentEndDate { get; set; }
}
