using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using KioskClientService.API.App;
using KioskClientService.API.Models;
using KioskClientService.API.Services.ProxyApi;
using KioskClientService.API.Services.TestData;
using KioskClientService.API.Services.Transaction;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace KioskClientService.API.Services.Promotion;

public class PromotionService : IPromotionService
{
	private static class Constants
	{
		public class TestData
		{
			public class PromotionService
			{
				public const string LiveCall = "PromotionService.LiveCall";

				public const string CommunicationFailure = "PromotionService.CommunicationFailure";

				public const string TestPromoCodes = "PromotionService.TestPromoCodes";
			}

			public const string CategoryName = "ProxyServicePromotionService";
		}
	}

	public class TestPromoCode
	{
		public string PromoCode { get; set; }

		public decimal Amount { get; set; }

		public PromotionIntentCode PromotionIntentCode { get; set; }

		public PromotionValidationResponse PromotionValidationResponse { get; set; }
	}

	private readonly ILogger<IPromotionService> _logger;

	private readonly ITestDataService _testDataService;

	private readonly ITestDataHelper _testDataHelper;

	private readonly IStoreService _store;

	private readonly IProxyApi _proxyApi;

	public PromotionService(ILogger<IPromotionService> logger, ITestDataService testDataService, IStoreService store, IProxyApi proxyApi)
	{
		_logger = logger;
		_testDataService = testDataService;
		_testDataHelper = _testDataService.GetTestDataHelper("ProxyServicePromotionService", "PromotionService.LiveCall");
		_store = store;
		_proxyApi = proxyApi;
	}

	public async Task<APIResponse<PromotionCodeValidationResponse>> ValidatePromotionCode(PromotionCodeValidationPayload payload)
	{
		_logger.LogInfoWithSource("Making Promotion Code Validation call to Proxy Service api", "ValidatePromotionCode");
		if (await _testDataHelper.GetUseTestData())
		{
			return await GetTestDataPromotionCodeValidationResponse(payload);
		}
		return await _proxyApi.PromoCodeValidationAsync(payload);
	}

	public async Task<APIResponse<PromotionCodeValidationResponse>> ValidatePromotionCode(string code)
	{
		return await ValidatePromotionCode(new PromotionCodeValidationPayload
		{
			Code = code,
			KioskId = _store.KioskId,
			MessageId = Guid.NewGuid()
		});
	}

	private async Task<APIResponse<PromotionCodeValidationResponse>> GetTestDataPromotionCodeValidationResponse(PromotionCodeValidationPayload payload)
	{
		APIResponse<PromotionCodeValidationResponse> response = new APIResponse<PromotionCodeValidationResponse>(null, _logger);
		PromotionCodeValidationResponse promotionCodeValidationResponse = new PromotionCodeValidationResponse();
		if (await _testDataHelper.GetTestDataValue("PromotionService.CommunicationFailure", defaultValue: false))
		{
			response.StatusCode = HttpStatusCode.InternalServerError;
			return response;
		}
		new List<TestPromoCode>();
		List<TestPromoCode> testPromoCodes = new List<TestPromoCode>();
		string value = await _testDataHelper.GetTestDataValue<string>("PromotionService.TestPromoCodes", null);
		if (!string.IsNullOrEmpty(value))
		{
			try
			{
				testPromoCodes = JsonConvert.DeserializeObject<List<TestPromoCode>>(value);
			}
			catch
			{
			}
		}
		TestPromoCode testPromoCode = testPromoCodes?.FirstOrDefault((TestPromoCode x) => x.PromoCode == payload.Code);
		if (testPromoCode == null)
		{
			testPromoCode = new TestPromoCode
			{
				PromotionValidationResponse = PromotionValidationResponse.CodeNotValid
			};
		}
		bool flag = testPromoCode != null && testPromoCode.PromotionValidationResponse == PromotionValidationResponse.Valid;
		if (testPromoCode != null)
		{
			promotionCodeValidationResponse.Amount = testPromoCode.Amount;
			promotionCodeValidationResponse.PromotionIntentCode = testPromoCode.PromotionIntentCode.ToString();
			promotionCodeValidationResponse.AllowFullDiscount = true;
			promotionCodeValidationResponse.Error = ResponseCodeAttribute.GetCodeFromEnum<PromotionValidationResponse>(testPromoCode.PromotionValidationResponse);
		}
		response.Response = promotionCodeValidationResponse;
		response.StatusCode = (flag ? HttpStatusCode.OK : HttpStatusCode.BadRequest);
		return response;
	}
}
