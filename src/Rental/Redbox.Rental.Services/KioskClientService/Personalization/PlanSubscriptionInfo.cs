using System;
using Redbox.Rental.Model.KioskClientService.Personalization;

namespace Redbox.Rental.Services.KioskClientService.Personalization;

public class PlanSubscriptionInfo : IPlanSubscriptionInfo
{
	public SubscriptionStatus SubscriptionStatus { get; set; }

	public int PlanId { get; set; }

	public bool IsOneNightFreeRentalAvailable { get; set; }

	public bool IsEarnPointsFor1ExtraNightAvailable { get; set; }

	public DateTime? EnrollmentEndDate { get; set; }
}
