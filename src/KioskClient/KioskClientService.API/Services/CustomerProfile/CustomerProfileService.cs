using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using KioskClientService.API.App;
using KioskClientService.API.Services.Configuration;
using KioskClientService.API.Services.ProxyApi;
using KioskClientService.API.Services.TestData;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API.Services.CustomerProfile;

public class CustomerProfileService : ICustomerProfileService
{
	public class Constants
	{
		public class TestData
		{
			public class Login
			{
				public const string LiveCall = "Login.LiveCall";

				public const string ErrorCode = "Login.ErrorCode";

				public const string CustomerProfileNumber = "Login.CustomerProfileNumber";

				public const string FirstName = "Login.FirstName";

				public const string LoginEmail = "Login.LoginEmail";

				public const string MobilePhoneNumber = "Login.MobilePhoneNumber";

				public const string LoyaltyPointBalance = "Login.LoyaltyPointBalance";

				public const string LoyaltyTier = "Login.LoyaltyTier";

				public const string LoyaltyTierCounter = "Login.LoyaltyTierCounter";

				public const string LoyaltySystemOnline = "Login.LoyaltySystemOnline";

				public const string LoyaltyIsEnrolled = "Login.LoyaltyIsEnrolled";

				public const string PointsExpiring = "Login.PointsExpiring";

				public const string PointsExpirationDate = "Login.PointsExpirationDate";

				public const string NumberOfSampleStoredPromos = "Login.NumberOfSampleStoredPromos";

				public const string PromptForPerks = "Login.PromptForPerks";

				public const string PromptForEarlyId = "Login.PromptForEarlyId";

				public const string TextClubEnabled = "Login.TextClubEnabled";

				public const string ActiveStarzSubscriptionPlanId = "Login.ActiveStarzSubscriptionPlanId";

				public const string RedboxPlusSubscriptionPlanId = "Login.RedboxPlusSubscriptionPlanId";

				public const string RedboxPlusSubscriptionCampaignId = "Login.RedboxPlusSubscriptionCampaignId";

				public const string RedboxPlusSubscriptionTier = "Login.RedboxPlusSubscriptionTier";

				public const string RedboxPlusSubscriptionEndDate = "Login.RedboxPlusSubscriptionEndDate";

				public const string RedboxPlusSubscriptionStatus = "Login.RedboxPlusSubscriptionStatus";

				public const string RedboxPlusFree1NightRentalAvailable = "Login.RedboxPlusFree1NightRentalAvailable";

				public const string EligibleSubscriptionPlanIds = "Login.EligibleSubscriptionPlanIds";

				public const string TivoQueryId = "Login.TivoQueryId";

				public const string OfferCode = "Login.OfferCode";

				public const string RecommendedTitlesIdType = "Login.RecommendedTitleIdType";

				public const string RecommendedIds = "Login.RecommendedIds";

				public const string RecommendedTags = "Login.RecommendedTags";
			}

			public const string CategoryName = "ProxyServiceCustomerProfile";
		}

		public class SubscriptionIds
		{
			public const string RedboxPlusSubscriptionId = "RBPLUS";

			public const string StarzSubscriptionId = "STARZ";
		}

		public class SubscriptionPlanIds
		{
			public const string RedboxPlusTier1SubscriptionPlanId = "RBPLUS_OFFER_1";

			public const string RedboxPlusTier2SubscriptionPlanId = "RBPLUS_OFFER_2";
		}
	}

	private readonly IKioskConfiguration _kioskConfiguration;

	private readonly ITestDataService _testDataService;

	private readonly ILogger<CustomerProfileService> _logger;

	private IProxyApi _proxyApi;

	public CustomerProfileService(IOptionsSnapshotKioskConfiguration optionsSnapshotKioskConfiguration, ITestDataService testDataService, ILogger<CustomerProfileService> logger, IProxyApi proxyApi)
	{
		_kioskConfiguration = optionsSnapshotKioskConfiguration;
		_testDataService = testDataService;
		_logger = logger;
		_proxyApi = proxyApi;
	}

	public async Task<APIResponse<CustomerProfileKioskLoginResponse>> Login(CustomerProfileLoginRequest request)
	{
		APIResponse<CustomerProfileKioskLoginResponse> aPIResponse;
		if (await GetUseTestData("Login.LiveCall"))
		{
			aPIResponse = await GetTestDataLoginResponse(request);
		}
		else
		{
			aPIResponse = await _proxyApi.Login(request);
			if (aPIResponse?.Response?.Response?.Subscriptions?.ExistingSubscriptions != null)
			{
				foreach (KioskSubscription existingSubscription in aPIResponse.Response.Response.Subscriptions.ExistingSubscriptions)
				{
					if (existingSubscription.SubscriptionId == "RBPLUS")
					{
						switch (existingSubscription.SubscriptionPlanId)
						{
						case "RBPLUS_OFFER_1":
							existingSubscription.SubscriptionTier = RedboxPlusSubscriptionTier.Tier1.ToString();
							break;
						case "RBPLUS_OFFER_2":
							existingSubscription.SubscriptionTier = RedboxPlusSubscriptionTier.Tier2.ToString();
							break;
						}
					}
				}
			}
			if (aPIResponse != null && !aPIResponse.IsSuccessStatusCode)
			{
				if (aPIResponse?.Response?.Success == true)
				{
					aPIResponse.Response.Success = false;
					aPIResponse.Response.Errors.Add(new Error
					{
						Code = "(KLI008)",
						Message = "ProxyService API Error"
					});
				}
				_logger.LogErrorWithSource(aPIResponse.Exception, "Login was unsuccessful.", "Login");
				LogResponseErrors(aPIResponse);
			}
		}
		return aPIResponse;
	}

	private async Task<APIResponse<CustomerProfileKioskLoginResponse>> GetTestDataLoginResponse(CustomerProfileLoginRequest request)
	{
		_logger?.LogInformation("CustomerProfileService.Login returning test data result");
		CustomerProfileKioskLoginResponse loginResponse = new CustomerProfileKioskLoginResponse
		{
			Response = new KioskLoginResponse(),
			Errors = new List<Error>()
		};
		string errorCode = await GetTestDataValue("Login.ErrorCode");
		if (!string.IsNullOrEmpty(errorCode))
		{
			loginResponse.Errors.Add(new Error
			{
				Code = errorCode,
				Message = "error code " + errorCode
			});
		}
		loginResponse.Success = loginResponse.Errors.Count == 0 || errorCode == "(KLI007)";
		loginResponse.MessageId = request.MessageId;
		KioskLoginResponse response = loginResponse.Response;
		KioskCustomer kioskCustomer = new KioskCustomer();
		KioskCustomer kioskCustomer2 = kioskCustomer;
		kioskCustomer2.CPN = await GetTestDataValue("Login.CustomerProfileNumber");
		KioskCustomer kioskCustomer3 = kioskCustomer;
		kioskCustomer3.FirstName = await GetTestDataValue("Login.FirstName");
		KioskCustomer kioskCustomer4 = kioskCustomer;
		kioskCustomer4.LoginEmail = await GetTestDataValue("Login.LoginEmail");
		KioskCustomer kioskCustomer5 = kioskCustomer;
		kioskCustomer5.MobilePhoneNumber = await GetTestDataValue("Login.MobilePhoneNumber");
		response.Customer = kioskCustomer;
		if (bool.TryParse(await GetTestDataValue("Login.PromptForPerks"), out var promptForPerks))
		{
			loginResponse.Response.Customer.PromptForPerks = promptForPerks;
		}
		if (bool.TryParse(await GetTestDataValue("Login.PromptForEarlyId"), out var promptForEarlyId))
		{
			loginResponse.Response.Customer.PromptForEarlyId = promptForEarlyId;
		}
		if (bool.TryParse(await GetTestDataValue("Login.TextClubEnabled"), out var textClubEnabled))
		{
			loginResponse.Response.Customer.TextClubEnabled = textClubEnabled;
		}
		if (int.TryParse(await GetTestDataValue("Login.NumberOfSampleStoredPromos"), out var sampleStoredPromoCount))
		{
			if (sampleStoredPromoCount > 0)
			{
				AddStoredPromo(loginResponse.Response, new KioskStoredPromoCode
				{
					Code = "UDVD01",
					CampaignName = "First Night DVD Rental",
					ExpirationDate = DateTime.Today.AddDays(1.0)
				});
			}
			if (sampleStoredPromoCount > 1)
			{
				AddStoredPromo(loginResponse.Response, new KioskStoredPromoCode
				{
					Code = "UBLU01",
					CampaignName = "First Night Blu-Ray Rental",
					ExpirationDate = DateTime.Today.AddDays(0.0)
				});
			}
			if (sampleStoredPromoCount > 2)
			{
				for (int i = 2; i < sampleStoredPromoCount; i++)
				{
					AddStoredPromo(loginResponse.Response, new KioskStoredPromoCode
					{
						Code = $"UDVD{i}",
						CampaignName = $"First Night DVD Rental ({i})",
						ExpirationDate = DateTime.Today.AddDays(7 * i)
					});
				}
			}
		}
		loginResponse.Response.Subscriptions = new KioskSubscriptions
		{
			EligibleSubscriptionPlanIds = new List<string>(),
			ExistingSubscriptions = new List<KioskSubscription>()
		};
		string eligibleSubscriptionPlanIds = await GetTestDataValue("Login.EligibleSubscriptionPlanIds");
		if (!string.IsNullOrEmpty(eligibleSubscriptionPlanIds))
		{
			string[] collection = eligibleSubscriptionPlanIds.Split(',');
			loginResponse.Response.Subscriptions.EligibleSubscriptionPlanIds.AddRange(collection);
		}
		string activeStarzSubscriptionPlanId = await GetTestDataValue("Login.ActiveStarzSubscriptionPlanId");
		if (!string.IsNullOrEmpty(activeStarzSubscriptionPlanId))
		{
			loginResponse.Response.Subscriptions.ExistingSubscriptions.Add(new KioskSubscription
			{
				status = CustomerSubscriptionPlanStatus.Active,
				SubscriptionPlanId = activeStarzSubscriptionPlanId,
				SubscriptionId = "STARZ"
			});
		}
		string redboxPlusSubscriptionPlanId = await GetTestDataValue("Login.RedboxPlusSubscriptionPlanId");
		if (!string.IsNullOrEmpty(redboxPlusSubscriptionPlanId))
		{
			string redboxPlusSubscriptionCampaignId = await GetTestDataValue("Login.RedboxPlusSubscriptionCampaignId");
			string redboxPlusSubscriptionTier = await GetTestDataValue("Login.RedboxPlusSubscriptionTier");
			string s = await GetTestDataValue("Login.RedboxPlusSubscriptionEndDate");
			DateTime redboxPlusSubscriptionEndDate = DateTime.Now.AddDays(200.0);
			if (DateTime.TryParse(s, out var parsedEndDate))
			{
				redboxPlusSubscriptionEndDate = parsedEndDate;
			}
			CustomerSubscriptionPlanStatus customerSubscriptionPlanStatus = CustomerSubscriptionPlanStatus.Active;
			string subscriptionStatusText = await GetTestDataValue("Login.RedboxPlusSubscriptionStatus");
			if (Enum.TryParse(typeof(CustomerSubscriptionPlanStatus), subscriptionStatusText, out object parsedStatus))
			{
				customerSubscriptionPlanStatus = (CustomerSubscriptionPlanStatus)parsedStatus;
			}
			loginResponse.Response.Subscriptions.ExistingSubscriptions.Add(new KioskSubscription
			{
				status = customerSubscriptionPlanStatus,
				SubscriptionPlanId = redboxPlusSubscriptionPlanId,
				SubscriptionId = "RBPLUS",
				CampaignId = redboxPlusSubscriptionCampaignId,
				EndDate = redboxPlusSubscriptionEndDate,
				SubscriptionTier = redboxPlusSubscriptionTier
			});
			string free1NightRentalAvailableText = await GetTestDataValue("Login.RedboxPlusFree1NightRentalAvailable");
			bool free1NightRentalAvailable = false;
			bool.TryParse(free1NightRentalAvailableText, out free1NightRentalAvailable);
			if (free1NightRentalAvailable)
			{
				string campaignName = ((redboxPlusSubscriptionTier == RedboxPlusSubscriptionTier.Tier1.ToString()) ? "RBPLUS 9.99" : "RBPLUS 19.99");
				int campaignId = ((redboxPlusSubscriptionTier == RedboxPlusSubscriptionTier.Tier1.ToString()) ? 9979 : 9981);
				DateTime dateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(1).AddDays(-1.0);
				DateTime expirationDate = ((redboxPlusSubscriptionTier == "Tier1") ? dateTime : dateTime.AddMonths(11));
				int promoCount = ((redboxPlusSubscriptionTier == "Tier1") ? 1 : 24);
				for (int j = 0; j < promoCount; j++)
				{
					AddStoredPromo(loginResponse.Response, new KioskStoredPromoCode
					{
						Code = $"RBPLUS{j}",
						CampaignId = campaignId,
						CampaignName = campaignName,
						ExpirationDate = expirationDate
					});
				}
			}
		}
		loginResponse.Response.Loyalty = new KioskLoyalty();
		if (bool.TryParse(await GetTestDataValue("Login.LoyaltyIsEnrolled"), out var isEnrolled))
		{
			loginResponse.Response.Loyalty.IsEnrolled = isEnrolled;
		}
		if (bool.TryParse(await GetTestDataValue("Login.LoyaltySystemOnline"), out var loyaltySystemOnline))
		{
			loginResponse.Response.Loyalty.LoyaltySystemOnline = loyaltySystemOnline;
		}
		if (int.TryParse(await GetTestDataValue("Login.LoyaltyPointBalance"), out var pointBalance))
		{
			loginResponse.Response.Loyalty.PointBalance = pointBalance;
		}
		KioskLoyalty loyalty = loginResponse.Response.Loyalty;
		loyalty.CurrentTier = await GetTestDataValue("Login.LoyaltyTier");
		if (int.TryParse(await GetTestDataValue("Login.LoyaltyTierCounter"), out var tierCounter))
		{
			loginResponse.Response.Loyalty.CurrentTierCounter = tierCounter;
		}
		if (int.TryParse(await GetTestDataValue("Login.PointsExpiring"), out var pointsExpiring))
		{
			loginResponse.Response.Loyalty.PointsExpiring = pointsExpiring;
		}
		if (DateTime.TryParse(await GetTestDataValue("Login.PointsExpirationDate"), out var pointsExpirationDate))
		{
			loginResponse.Response.Loyalty.PointsExpirationDate = pointsExpirationDate;
		}
		loginResponse.Response.Recommendations = new KioskRecommendedTitles();
		KioskRecommendedTitles recommendations = loginResponse.Response.Recommendations;
		recommendations.QueryId = await GetTestDataValue("Login.TivoQueryId");
		recommendations = loginResponse.Response.Recommendations;
		recommendations.OfferCode = await GetTestDataValue("Login.OfferCode");
		loginResponse.Response.Recommendations.Products = new List<KioskTitleInfo>();
		string recommendedTitleIdType = await GetTestDataValue("Login.RecommendedTitleIdType");
		string recommendedTags = await GetTestDataValue("Login.RecommendedTags");
		List<string> tagList = new List<string>();
		if (!string.IsNullOrEmpty(recommendedTags))
		{
			tagList = recommendedTags.Replace(" ", null).Replace("[", null).Split(']')?.ToList();
		}
		string recommendedIds = await GetTestDataValue("Login.RecommendedIds");
		if (!string.IsNullOrEmpty(recommendedIds))
		{
			List<string> recommendedIdList = recommendedIds.Replace(" ", null).Split(',')?.ToList();
			if (recommendedIdList != null)
			{
				int titleIndex = 0;
				foreach (string titleId in recommendedIdList)
				{
					KioskTitleInfo kioskTitleInfo = new KioskTitleInfo
					{
						Id = titleId,
						Type = recommendedTitleIdType,
						Tags = new List<string>()
					};
					string titleTags = ((tagList != null && tagList.Count > titleIndex) ? tagList[titleIndex] : null);
					if (!string.IsNullOrEmpty(titleTags))
					{
						List<string> titleTagList = titleTags.Split(',')?.ToList();
						if (titleTagList != null)
						{
							kioskTitleInfo.Tags.AddRange(titleTagList);
						}
					}
					loginResponse.Response.Recommendations.Products.Add(kioskTitleInfo);
					titleIndex++;
				}
			}
		}
		APIResponse<CustomerProfileKioskLoginResponse> aPIResponse = new APIResponse<CustomerProfileKioskLoginResponse>(null, _logger);
		aPIResponse.Response = loginResponse;
		aPIResponse.StatusCode = HttpStatusCode.OK;
		return aPIResponse;
	}

	private void AddStoredPromo(KioskLoginResponse response, KioskStoredPromoCode kioskStoredPromoCode)
	{
		if (response != null)
		{
			if (response.PromoCodes == null)
			{
				response.PromoCodes = new List<KioskStoredPromoCode>();
			}
			if (kioskStoredPromoCode != null)
			{
				response.PromoCodes.Add(kioskStoredPromoCode);
			}
		}
	}

	private async Task<string> GetTestDataValue(string propertyName)
	{
		return (await (_testDataService?.ReadTestDataValueAsync("ProxyServiceCustomerProfile", propertyName)))?.PropertyValue;
	}

	private async Task<bool> GetUseTestData(string liveCallPropertyName)
	{
		bool result = false;
		if (_kioskConfiguration?.TestData?.TestDataEnabled == true)
		{
			ReadTestDataResponse readTestDataResponse = await (_testDataService?.ReadTestDataValueAsync("ProxyServiceCustomerProfile", liveCallPropertyName));
			if (readTestDataResponse != null && readTestDataResponse.Success && readTestDataResponse.GetPropertyValue<bool?>() == false)
			{
				result = true;
			}
		}
		return result;
	}

	private void LogResponseErrors<T>(APIResponse<T> response)
	{
		if (response.Errors == null || response.Errors.Count <= 0)
		{
			return;
		}
		foreach (Error error in response.Errors)
		{
			_logger.LogErrorWithSource("Error code " + error.Code + " message " + error.Message, "LogResponseErrors");
		}
	}
}
