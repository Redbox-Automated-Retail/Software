using Redbox.Rental.Model;

namespace Redbox.Rental.Services.KioskClientService.Personalization;

public class GetPlanSubscriptionResponse : BaseResponse
{
	public PlanSubscription PlanSubscription { get; set; }

	public GetPlanSubscriptionResponse()
	{
		base.Success = true;
	}
}
