using System;
using Redbox.Rental.Model.KioskClientService.Personalization;

namespace Redbox.Rental.Services.KioskClientService.Personalization;

public class PlanSubscription
{
	public long SubscriptionId { get; set; }

	public SubscriptionStatus SubscriptionStatus { get; set; }

	public DateTime? EnrollmentStartDate { get; set; }

	public DateTime? EnrollmentEndDate { get; set; }

	public BenefitPlan BenefitPlan { get; set; }
}
