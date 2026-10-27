using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using KioskClientService.API.App;
using KioskClientService.API.Services.Configuration;
using KioskClientService.API.Services.KioskEngineApi;
using KioskClientService.API.Services.ProxyApi;
using KioskClientService.API.Services.TestData;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace KioskClientService.API.Services.Loyalty;

internal class LoyaltyService : ILoyaltyService
{
	public class LoyaltyTestAccrualRedemption
	{
		public long ProductId { get; set; }

		public string Name { get; set; }

		public int AccrualPoints { get; set; }

		public int RedemptionPoints { get; set; }
	}

	private enum LoyaltyEstimateType
	{
		Accrual,
		Redemption
	}

	public static class Constants
	{
		public class TestData
		{
			public class LoyaltyService
			{
				public const string CategoryName = "ProxyServiceLoyalytService";

				public const string LiveCall = "LoyaltyService.LiveCall";

				public const string SimulateAccrualSuccess = "LoyaltyService.SimulateAccrualSuccess";

				public const string AccrualError = "LoyaltyService.AccrualError";

				public const string SimulateRedemptionSuccess = "LoyaltyService.SimulateRedemptionSuccess";

				public const string RedemptionError = "LoyaltyService.RedemptionError";

				public const string DefaultAccrualValue = "LoyaltyService.DefaultAccrualValue";

				public const string DefaultRedemptionValue = "LoyaltyService.DefaultRedemptionValue";

				public const string ShoppingCartItems = "LoyaltyService.ShoppingCartItems";
			}

			public class PersonalizationService
			{
				public const string CategoryName = "ProxyServicePersonalizationService";

				public const string LiveCall = "PersonalizationService.LiveCall";

				public const string UpdateAccountError = "PersonalizationService.UpdateAccountError";

				public const string MobilePhoneNumber = "PersonalizationService.MobilePhoneNumber";

				public const string CustomerProfileNumber = "PersonalizationService.CustomerProfileNumber";

				public const string TempPassword = "PersonalizationService.TempPassword";
			}
		}

		public class AccrualValues
		{
			public const string NoContentErrorCode = "(EA000)";

			public const string AccountNotFound = "Account not found";

			public const string AccountNotFoundErrorCode = "(EA001)";

			public const string LoyaltyError = "Loyalty error";

			public const string LoyalytErrorCode = "(EA002)";

			public const string CommunicationErrorCode = "(EA003)";
		}

		public class RedemptionValues
		{
			public const string NoContentErrorCode = "(ER000)";

			public const string AccountNotFound = "Account not found";

			public const string AccountNotFoundErrorCode = "(ER001)";

			public const string LoyaltyError = "Loyalty error";

			public const string LoyalytErrorCode = "(ER002)";

			public const string CommunicationErrorCode = "(ER003)";
		}
	}

	private readonly ILogger<ILoyaltyService> _logger;

	private readonly IProxyApi _proxyApi;

	private readonly ITestDataService _testDataService;

	private readonly IKioskConfiguration _kioskConfiguration;

	private readonly IKioskEngineApiShoppingCartClient _kioskEngineApiShoppingCartClient;

	public LoyaltyService(ILogger<ILoyaltyService> logger, IProxyApi proxyApi, ITestDataService testDataService, IOptionsSnapshotKioskConfiguration optionsSnapshotKioskConfiguration, IKioskEngineApiShoppingCartClient kioskEngineApiShoppingCartClient)
	{
		_logger = logger;
		_proxyApi = proxyApi;
		_testDataService = testDataService;
		_kioskConfiguration = optionsSnapshotKioskConfiguration;
		_kioskEngineApiShoppingCartClient = kioskEngineApiShoppingCartClient;
	}

	public async Task<APIResponse<ServerLoyaltyResponse>> EstimateAccrualAsync(CartPayload payload)
	{
		if (await GetUseTestData("LoyaltyService.LiveCall", "ProxyServiceLoyalytService"))
		{
			return await GetTestDataServerLoyaltyResponse(payload, LoyaltyEstimateType.Accrual);
		}
		return await _proxyApi.EstimateAccrualAsync(payload);
	}

	private async Task<APIResponse<ServerLoyaltyResponse>> GetTestDataServerLoyaltyResponse(CartPayload payload, LoyaltyEstimateType loyaltyEstimateType)
	{
		APIResponse<ServerLoyaltyResponse> apiResponse = new APIResponse<ServerLoyaltyResponse>(null, _logger)
		{
			HttpResponse = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.InternalServerError
			},
			Response = new ServerLoyaltyResponse()
		};
		await GetTestDataLoyaltyResponseStatusCode(apiResponse, loyaltyEstimateType);
		await GetTestDataLoyaltyResponseItemEstimations(apiResponse.Response, loyaltyEstimateType);
		return apiResponse;
	}

	private async Task GetTestDataLoyaltyResponseStatusCode(APIResponse<ServerLoyaltyResponse> apiResponse, LoyaltyEstimateType loyaltyEstimateType)
	{
		if (bool.TryParse(await GetTestDataValue("LoyaltyService.SimulateAccrualSuccess", "ProxyServiceLoyalytService"), out var result) && result)
		{
			apiResponse.StatusCode = HttpStatusCode.OK;
		}
		if (apiResponse.StatusCode == HttpStatusCode.OK)
		{
			return;
		}
		if (loyaltyEstimateType == LoyaltyEstimateType.Accrual)
		{
			switch (await GetTestDataValue("LoyaltyService.AccrualError", "ProxyServiceLoyalytService"))
			{
			case "(EA001)":
				apiResponse.Response.Error = "Account not found";
				break;
			case "(EA002)":
				apiResponse.Response.Error = "Loyalty error";
				break;
			}
		}
		else
		{
			switch (await GetTestDataValue("LoyaltyService.RedemptionError", "ProxyServiceLoyalytService"))
			{
			case "(ER001)":
				apiResponse.Response.Error = "Account not found";
				break;
			case "(ER002)":
				apiResponse.Response.Error = "Loyalty error";
				break;
			}
		}
	}

	private async Task GetTestDataLoyaltyResponseItemEstimations(ServerLoyaltyResponse response, LoyaltyEstimateType loyaltyEstimateType)
	{
		int defaultAccrualValue = 1000;
		int.TryParse(await GetTestDataValue("LoyaltyService.DefaultAccrualValue", "ProxyServiceLoyalytService"), out defaultAccrualValue);
		int defaultRedemptionValue = 2000;
		int.TryParse(await GetTestDataValue("LoyaltyService.DefaultRedemptionValue", "ProxyServiceLoyalytService"), out defaultRedemptionValue);
		List<LoyaltyTestAccrualRedemption> savedLoyaltyTestAccrualRedemptions = new List<LoyaltyTestAccrualRedemption>();
		string value = await GetTestDataValue("LoyaltyService.ShoppingCartItems", "ProxyServiceLoyalytService");
		if (!string.IsNullOrEmpty(value))
		{
			try
			{
				savedLoyaltyTestAccrualRedemptions = JsonConvert.DeserializeObject<List<LoyaltyTestAccrualRedemption>>(value);
			}
			catch
			{
			}
		}
		List<LoyaltyTestShoppingCartItem> list = await _kioskEngineApiShoppingCartClient.GetLoyaltyTestShoppingCartItems();
		if (list == null)
		{
			return;
		}
		foreach (LoyaltyTestShoppingCartItem eachLoyaltyTestShoppingCartItem in list)
		{
			LoyaltyTestAccrualRedemption loyaltyTestAccrualRedemption = savedLoyaltyTestAccrualRedemptions.FirstOrDefault((LoyaltyTestAccrualRedemption x) => x.ProductId == eachLoyaltyTestShoppingCartItem.ProductId);
			int num = loyaltyTestAccrualRedemption?.AccrualPoints ?? defaultAccrualValue;
			int num2 = loyaltyTestAccrualRedemption?.RedemptionPoints ?? defaultRedemptionValue;
			response.Items.Add(new ItemEstimation
			{
				ProductId = eachLoyaltyTestShoppingCartItem.ProductId,
				Points = ((loyaltyEstimateType == LoyaltyEstimateType.Accrual) ? num : num2)
			});
		}
	}

	public async Task<APIResponse<ServerLoyaltyResponse>> EstimateRedemptionAsync(CartPayload payload)
	{
		if (await GetUseTestData("LoyaltyService.LiveCall", "ProxyServiceLoyalytService"))
		{
			return await GetTestDataServerLoyaltyResponse(payload, LoyaltyEstimateType.Redemption);
		}
		return await _proxyApi.EstimateRedemptionAsync(payload);
	}

	public async Task<UpdateAccountResponse> UpdateAccountAsync(UpdateAccountPayload payload)
	{
		if (await GetUseTestData("PersonalizationService.LiveCall", "ProxyServicePersonalizationService"))
		{
			return await GetTestDataUpdateAccountResponse(payload);
		}
		UpdateAccountResponse result = new UpdateAccountResponse
		{
			MessageId = payload.MessageId,
			KioskId = payload.KioskId
		};
		try
		{
			UpdateAccountResponse updateAccountResponse = result;
			updateAccountResponse.CopyFromApiResponse(await _proxyApi.UpdateAccountAsync(payload));
		}
		catch (Exception exception)
		{
			_logger.LogCritical(exception, "LoyaltyService.UpdateAccountAsync -> Exception updating account!");
			result.StatusCode = HttpStatusCode.InternalServerError;
			result.Error = HttpStatusCode.InternalServerError.ToString();
		}
		return result;
	}

	private async Task<UpdateAccountResponse> GetTestDataUpdateAccountResponse(UpdateAccountPayload payload)
	{
		UpdateAccountResponse updateAccountResponse = new UpdateAccountResponse
		{
			StatusCode = HttpStatusCode.OK
		};
		UpdateAccountResponse updateAccountResponse2 = updateAccountResponse;
		updateAccountResponse2.Error = await GetTestDataValue("PersonalizationService.UpdateAccountError", "ProxyServicePersonalizationService");
		updateAccountResponse2 = updateAccountResponse;
		updateAccountResponse2.CustomerProfileNumber = await GetTestDataValue("PersonalizationService.CustomerProfileNumber", "ProxyServicePersonalizationService");
		updateAccountResponse2 = updateAccountResponse;
		updateAccountResponse2.MobilePhoneNumber = await GetTestDataValue("PersonalizationService.MobilePhoneNumber", "ProxyServicePersonalizationService");
		updateAccountResponse2 = updateAccountResponse;
		updateAccountResponse2.TempPassword = await GetTestDataValue("PersonalizationService.TempPassword", "ProxyServicePersonalizationService");
		return updateAccountResponse;
	}

	private async Task<bool> GetUseTestData(string liveCallPropertyName, string categoryName)
	{
		bool result = false;
		if (_kioskConfiguration?.TestData?.TestDataEnabled == true)
		{
			ReadTestDataResponse readTestDataResponse = await (_testDataService?.ReadTestDataValueAsync(categoryName, liveCallPropertyName));
			if (readTestDataResponse != null && readTestDataResponse.Success && readTestDataResponse.GetPropertyValue<bool?>() == false)
			{
				result = true;
			}
		}
		return result;
	}

	private async Task<string> GetTestDataValue(string propertyName, string categoryName)
	{
		return (await (_testDataService?.ReadTestDataValueAsync(categoryName, propertyName)))?.PropertyValue;
	}
}
