using System.Net.Http;
using System.Threading.Tasks;
using Redbox.Core;
using Redbox.Rental.Services.App;
using Redbox.Rental.Services.KioskClientService.Subscriptions;

namespace Redbox.KioskEngine.PreferenceManager.KioskClientServiceClient.Subscriptions;

public class KioskClientServiceSubscriptions : KioskClientServiceBaseClient
{
	private static string _controllerPrefix = "api/subscription";

	public static async Task<APIResponse<GetSubscriptionPlanResponse>> GetSubscriptionPlans()
	{
		IHttpService httpService = ServiceLocator.Instance.GetService<IHttpService>();
		HttpRequestMessage request = httpService.GenerateRequest(KioskClientServiceBaseClient.KioskClientServiceBaseUrl, _controllerPrefix + "/plans", null, HttpMethod.Get);
		return await httpService.SendRequestAsync<GetSubscriptionPlanResponse>(request, null, "GetSubscriptionPlans", "c:\\build\\Redbox.KioskEngine.PreferenceManager\\KioskClientServiceClient\\Subscriptions\\KioskClientServiceSubscriptions.cs");
	}
}
