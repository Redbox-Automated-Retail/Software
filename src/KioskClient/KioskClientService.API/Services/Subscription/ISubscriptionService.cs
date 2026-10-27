using System.Threading.Tasks;

namespace KioskClientService.API.Services.Subscription;

public interface ISubscriptionService
{
	Task<SubscriptionPlansData> GetSubscriptionPlanData();

	Task RefreshSubscriptionPlans();

	Task RefreshIfNeeded();

	void DeleteSubscriptionPlans();
}
