using System.Threading.Tasks;
using KioskClientService.API.App;
using KioskClientService.API.Models;
using KioskClientService.API.Services.Configuration;
using KioskClientService.API.Services.HttpQueue;
using KioskClientService.API.Services.ProxyApi;
using KioskClientService.API.Services.TestData;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API.Services.Marketing;

public class MarketingService : IMarketingService
{
	private static class Constants
	{
		public class TestData
		{
			public class MarketingService
			{
				public const string LiveCall = "MarketingService.LiveCall";

				public const string CommunicationFailure = "MarketingService.CommunicationFailure";

				public const string ConfirmationStatus = "MarketingService.ConfirmationStatus";

				public const string CustomerProfileNumber = "MarketingService.CustomerProfileNumber";

				public const string MobilePhoneNumber = "MarketingService.MobilePhoneNumber";

				public const string EmailVerified = "MarketingService.EmailVerified";

				public const string PromptForPerks = "MarketingService.PromptForPerks";

				public const string PromptForEarlyId = "MarketingService.PromptForEarlyId";

				public const string TextClubEnabled = "MarketingService.TextClubEnabled";
			}

			public const string CategoryName = "ProxyServiceMarketingService";
		}
	}

	private readonly ILogger<MarketingService> _logger;

	private readonly IProxyApi _proxyApi;

	private readonly ITestDataService _testDataService;

	private readonly IKioskConfiguration _kioskConfiguration;

	private readonly ITestDataHelper _testDataHelper;

	private readonly IHttpQueueService _httpQueueService;

	public MarketingService(ILogger<MarketingService> logger, IProxyApi proxyApi, ITestDataService testDataService, IOptionsSnapshotKioskConfiguration optionsSnapshotKioskConfiguration, IHttpQueueService httpQueueService)
	{
		_logger = logger;
		_proxyApi = proxyApi;
		_testDataService = testDataService;
		_kioskConfiguration = optionsSnapshotKioskConfiguration;
		_testDataHelper = _testDataService.GetTestDataHelper("ProxyServiceMarketingService", "MarketingService.LiveCall");
		_httpQueueService = httpQueueService;
	}

	public async Task<EmailConfirmationResponse> EmailConfirmation(EmailConfirmationPayload payload)
	{
		EmailConfirmationResponse result;
		if (await _testDataHelper.GetUseTestData())
		{
			result = await GetTestDataEmailConfirmationResponse(payload);
		}
		else
		{
			APIResponse<EmailConfirmationResponse> aPIResponse = await _proxyApi.EmailConfirmation(payload);
			result = (aPIResponse.IsSuccessStatusCode ? aPIResponse.Response : ((aPIResponse.Response != null) ? aPIResponse.Response : new EmailConfirmationResponse
			{
				MessageId = payload.MessageId,
				StatusCode = (int)aPIResponse.StatusCode
			}));
		}
		return result;
	}

	private async Task<EmailConfirmationResponse> GetTestDataEmailConfirmationResponse(EmailConfirmationPayload payload)
	{
		EmailConfirmationResponse emailConfirmationResponse = new EmailConfirmationResponse();
		if (await _testDataHelper.GetTestDataValue("MarketingService.CommunicationFailure", defaultValue: false))
		{
			emailConfirmationResponse.StatusCode = 500;
			return emailConfirmationResponse;
		}
		EmailConfirmationResponse emailConfirmationResponse2 = emailConfirmationResponse;
		emailConfirmationResponse2.ConfirmationStatus = await _testDataHelper.GetTestDataValue("MarketingService.ConfirmationStatus", ConfirmationStatus.DoNotShow);
		emailConfirmationResponse2 = emailConfirmationResponse;
		emailConfirmationResponse2.CustomerProfileNumber = await _testDataHelper.GetTestDataValue<string>("MarketingService.CustomerProfileNumber", null);
		emailConfirmationResponse2 = emailConfirmationResponse;
		emailConfirmationResponse2.MobilePhoneNumber = await _testDataHelper.GetTestDataValue<string>("MarketingService.MobilePhoneNumber", null);
		emailConfirmationResponse2 = emailConfirmationResponse;
		emailConfirmationResponse2.EmailVerified = await _testDataHelper.GetTestDataValue<bool?>("MarketingService.EmailVerified", null);
		emailConfirmationResponse2 = emailConfirmationResponse;
		emailConfirmationResponse2.PromptForPerks = await _testDataHelper.GetTestDataValue("MarketingService.PromptForPerks", defaultValue: false);
		emailConfirmationResponse2 = emailConfirmationResponse;
		emailConfirmationResponse2.PromptForEarlyId = await _testDataHelper.GetTestDataValue("MarketingService.PromptForEarlyId", defaultValue: false);
		emailConfirmationResponse2 = emailConfirmationResponse;
		emailConfirmationResponse2.TextClubEnabled = await _testDataHelper.GetTestDataValue("MarketingService.TextClubEnabled", defaultValue: false);
		emailConfirmationResponse.StatusCode = 200;
		emailConfirmationResponse.Success = true;
		return emailConfirmationResponse;
	}
}
