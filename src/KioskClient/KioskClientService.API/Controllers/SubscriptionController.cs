using System.Threading.Tasks;
using KioskClientService.API.App;
using KioskClientService.API.Services.Configuration;
using KioskClientService.API.Services.Subscription;
using KioskClientService.API.Services.TestData;
using Microsoft.AspNetCore.Mvc;

namespace KioskClientService.API.Controllers;

[Route("api/subscription")]
[ApiController]
public class SubscriptionController : ControllerBase
{
	private class Constants
	{
		public class TestData
		{
			public class GetSubscriptionPlans
			{
				public const string LiveCall = "GetSubscriptionPlans.LiveCall";

				public const string BrowseStarzSubscriptionPlan = "GetSubscriptionPlans.BrowseStarzSubscriptionPlan";

				public const string MyBagStarzSubscriptionPlan = "GetSubscriptionPlans.MyBagStarzSubscriptionPlan";
			}

			public const string CategoryName = "ProxyServiceSubscriptions";
		}
	}

	private readonly ISubscriptionService _subscriptionService;

	private readonly IKioskConfiguration _kioskConfiguration;

	private readonly ITestDataService _testDataService;

	public SubscriptionController(IOptionsSnapshotKioskConfiguration optionsSnapshotKioskConfiguration, ITestDataService testDataService, ISubscriptionService subscriptionService)
	{
		_subscriptionService = subscriptionService;
		_kioskConfiguration = optionsSnapshotKioskConfiguration;
		_testDataService = testDataService;
	}

	[HttpGet("plans")]
	[ProducesResponseType(404)]
	[ProducesResponseType(typeof(GetSubscriptionPlanResponse), 200)]
	public async Task<ActionResult<GetSubscriptionPlanResponse>> GetSubscriptionPlans()
	{
		SubscriptionPlansData subscriptionPlansData = await _subscriptionService.GetSubscriptionPlanData();
		if (subscriptionPlansData == null)
		{
			return new NotFoundResult();
		}
		subscriptionPlansData.RemoveNonActive();
		GetSubscriptionPlanResponse response = new GetSubscriptionPlanResponse();
		response.CopyFrom(subscriptionPlansData);
		if (await GetUseTestData("GetSubscriptionPlans.LiveCall"))
		{
			string browseStarzSubscriptionPlan = (await (_testDataService?.ReadTestDataValueAsync("ProxyServiceSubscriptions", "GetSubscriptionPlans.BrowseStarzSubscriptionPlan")))?.PropertyValue;
			string text = (await (_testDataService?.ReadTestDataValueAsync("ProxyServiceSubscriptions", "GetSubscriptionPlans.MyBagStarzSubscriptionPlan")))?.PropertyValue;
			if (!string.IsNullOrEmpty(browseStarzSubscriptionPlan))
			{
				response.BrowseStarzSubscriptionPlan = browseStarzSubscriptionPlan;
			}
			if (!string.IsNullOrEmpty(text))
			{
				response.MyBagStarzSubscriptionPlan = text;
			}
		}
		return response.ToObjectResult();
	}

	private async Task<bool> GetUseTestData(string liveCallPropertyName)
	{
		bool result = false;
		if (_kioskConfiguration?.TestData?.TestDataEnabled == true)
		{
			ReadTestDataResponse readTestDataResponse = await (_testDataService?.ReadTestDataValueAsync("ProxyServiceSubscriptions", liveCallPropertyName));
			if (readTestDataResponse != null && readTestDataResponse.Success && readTestDataResponse.GetPropertyValue<bool?>() == false)
			{
				result = true;
			}
		}
		return result;
	}

	[HttpPost("plans/refresh")]
	[ProducesResponseType(200)]
	public async Task<ActionResult<BaseResponse>> RefreshSortOrder()
	{
		await _subscriptionService.RefreshSubscriptionPlans();
		return Ok();
	}

	[HttpPost("plans/refreshifneeded")]
	[ProducesResponseType(200)]
	public async Task<ActionResult<BaseResponse>> RefreshSortOrderIfNeeded()
	{
		await _subscriptionService.RefreshIfNeeded();
		return Ok();
	}

	[HttpDelete("plans")]
	[ProducesResponseType(200)]
	public ActionResult<BaseResponse> DeleteSortOrder()
	{
		_subscriptionService.DeleteSubscriptionPlans();
		return Ok();
	}
}
