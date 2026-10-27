using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Model;
using Redbox.Rental.Model.KioskClientService;
using Redbox.Rental.Model.KioskClientService.CustomerProfile;
using Redbox.Rental.Model.KioskClientService.Subscriptions;
using Redbox.Rental.Model.KioskClientService.TestData;
using Redbox.Rental.Model.Personalization;
using Redbox.Rental.Services.Configuration;
using Redbox.Rental.Services.Personalization;
using Redbox.Rental.UI.Properties;

namespace Redbox.Rental.Services.KioskClientService.CustomerProfile;

public class KioskClientServiceCustomerProfile : BaseKioskClientService, IKioskClientServiceCustomerProfile
{
	private const string _apiPath = "api/customerprofile";

	private int m_working;

	private bool IsLoginLiveCall
	{
		get
		{
			bool result = true;
			IKioskClientServiceTestData kioskClientServiceTestData = ServiceLocator.Instance.GetService<IKioskClientService>()?.TestData;
			if (kioskClientServiceTestData != null)
			{
				IReadTestDataValueResult readTestDataValueResult = kioskClientServiceTestData.ReadPropertyValue("ProxyServiceCustomerProfile", "Login.LiveCall");
				if (readTestDataValueResult != null && readTestDataValueResult.Success && bool.TryParse(readTestDataValueResult.PropertyValue, out var result2))
				{
					result = result2;
				}
			}
			return result;
		}
	}

	public void LoginWithEmail(string emailAddress, SecureString password, Action<ILoginResult> loginResultCallback)
	{
		Login(emailAddress, password, null, null, null, loginResultCallback);
	}

	public void LoginWithPhoneNumber(string phoneNumber, SecureString pin, Action<ILoginResult> loginResultCallback)
	{
		Login(null, null, phoneNumber, pin, null, loginResultCallback);
	}

	public void LoginWithCustomerProfileNumber(string customerProfileNumber, Action<ILoginResult> loginResultCallback)
	{
		Login(null, null, null, null, customerProfileNumber, loginResultCallback);
	}

	public void Login(ICustomerProfileLoginRequest customerProfileLoginRequest, RemoteServiceCallback completeCallback)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			RemoteServiceResult remoteServiceResult = new RemoteServiceResult();
			ICustomerProfileKioskLoginResponse result = LoginAsync(customerProfileLoginRequest).Result;
			remoteServiceResult.Properties["AuthorizeResult"] = result;
			remoteServiceResult.Success = result.Success;
			completeCallback(remoteServiceResult);
		});
	}

	private void Login(string emailAddress, SecureString password, string phoneNumber, SecureString pin, string customerProfileNumber, Action<ILoginResult> loginResultCallback)
	{
		IConfiguration instance = Redbox.Rental.Services.Configuration.Configuration.Instance;
		if (instance != null && instance.EarlyIdEnabled)
		{
			int num = (int)(ServiceLocator.Instance.GetService<IStoreManager>()?.KioskId ?? 0);
			string languageCode = OrchestrationService.GetCulture()?.Substring(0, 2);
			bool isEncrypted = true;
			ICustomerProfileKioskLoginResponse customerProfileKioskLoginResponse = null;
			CustomerProfileLoginRequest customerProfileLoginRequest = new CustomerProfileLoginRequest
			{
				MessageId = Guid.NewGuid(),
				ActivityId = ServiceLocator.Instance.GetService<IShoppingSessionService>()?.GetCurrentSession()?.Id.ToString(),
				EmailAddress = emailAddress,
				Password = password?.SecureStringToString(),
				PhoneNumber = phoneNumber,
				Pin = pin?.SecureStringToString(),
				KioskId = num,
				IsEncrypted = isEncrypted,
				LanguageCode = languageCode,
				CustomerProfileNumber = customerProfileNumber
			};
			if (customerProfileLoginRequest.IsEncrypted)
			{
				customerProfileLoginRequest.Password = customerProfileLoginRequest.Password?.EncryptToBase64();
				customerProfileLoginRequest.Pin = customerProfileLoginRequest.Pin?.EncryptToBase64();
			}
			Task.Run(delegate
			{
				string text = (IsLoginLiveCall ? "Live ProxyService" : "Test data");
				LogHelper.Instance.Log("Performing " + text + " Login Call");
				LogHelper.Instance.Log($"Login Request: {customerProfileLoginRequest?.Scrub()}");
				customerProfileKioskLoginResponse = LoginAsync(customerProfileLoginRequest).Result;
			}).ContinueWith(delegate
			{
				ProcessLoginResult(customerProfileLoginRequest, customerProfileKioskLoginResponse, loginResultCallback);
			}, CancellationToken.None, TaskContinuationOptions.None, ServiceLocator.Instance.GetService<IViewService>()?.UITaskScheduler);
		}
		else
		{
			LogHelper.Instance.Log("PersonalizationService login rejected.  Early ID is not enabled.");
			LoginResult obj = new LoginResult
			{
				Success = false
			};
			if (loginResultCallback != null)
			{
				loginResultCallback(obj);
			}
		}
	}

	private async Task<ICustomerProfileKioskLoginResponse> LoginAsync(ICustomerProfileLoginRequest customerProfileLoginRequest)
	{
		CustomerProfileKioskLoginResponse customerProfileKioskloginResponse = new CustomerProfileKioskLoginResponse();
		using (ExecutionTimer executionTimer = new ExecutionTimer())
		{
			try
			{
				Interlocked.Increment(ref m_working);
				customerProfileKioskloginResponse = await CallLogineAsync(customerProfileLoginRequest);
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in KioskClientServiceCustomerProfile.LoginAsync.", e);
				return customerProfileKioskloginResponse;
			}
			finally
			{
				try
				{
					LogHelper.Instance.Log($"Login Result: {customerProfileKioskloginResponse?.Scrub()}");
					LogHelper.Instance.Log($"Login completed in {executionTimer.Elapsed} ms");
				}
				finally
				{
					Interlocked.Decrement(ref m_working);
				}
			}
		}
		return customerProfileKioskloginResponse;
	}

	private async Task<CustomerProfileKioskLoginResponse> CallLogineAsync(ICustomerProfileLoginRequest loginRequest)
	{
		WebResult<CustomerProfileKioskLoginResponse> webResult = await ExecuteAsyncWebRequest<CustomerProfileKioskLoginResponse>(new Uri(base.KioskClientServiceBaseUrl + "/api/customerprofile/login"), loginRequest, "POST", 100000);
		LogResponse("KioskClientServiceCustomerProfile.Login", webResult);
		return webResult?.Response;
	}

	private void ProcessLoginResult(ICustomerProfileLoginRequest customerProfileLoginRequest, ICustomerProfileKioskLoginResponse customerProfileKioskLoginResponse, Action<ILoginResult> loginResultCallback)
	{
		LoginResult loginResult = new LoginResult
		{
			Success = (customerProfileKioskLoginResponse?.Success ?? false),
			KioskLoginResponseError = KioskLoginResponseErrors.None
		};
		LoginType loginType = (string.IsNullOrEmpty(customerProfileLoginRequest?.EmailAddress) ? LoginType.Phone : LoginType.Email);
		OrchestrationService.SetCurrentUICulture();
		if (customerProfileKioskLoginResponse != null && customerProfileKioskLoginResponse.Success)
		{
			PopulatePersonalizationSession(customerProfileKioskLoginResponse, loginResult);
			(ServiceLocator.Instance.GetService<IShoppingSessionService>()?.GetCurrentSession())?.ShoppingCart?.SetPropertyBagEntry("EarlyIdIndicator", "true");
		}
		else
		{
			string text = (((customerProfileKioskLoginResponse?.Errors?.Count()).GetValueOrDefault() <= 0) ? null : customerProfileKioskLoginResponse?.Errors[0]?.Code);
			object fieldsForResponseCode = ResponseCodeAttribute.GetFieldsForResponseCode(typeof(KioskLoginResponseErrors?), text);
			switch (loginResult.KioskLoginResponseError = ((fieldsForResponseCode != null) ? ((KioskLoginResponseErrors)fieldsForResponseCode) : KioskLoginResponseErrors.None))
			{
			case KioskLoginResponseErrors.AccountLocked:
				loginResult.ErrorMessage = Resources.personalization_service_error_KLI0001;
				break;
			case KioskLoginResponseErrors.InvalidPasswordOrPin:
				if (loginType == LoginType.Email)
				{
					loginResult.ErrorMessage = Resources.personalization_service_error_KLI0002_email;
				}
				else
				{
					loginResult.ErrorMessage = Resources.personalization_service_error_KLI0002_phone;
				}
				break;
			case KioskLoginResponseErrors.AccountNotFound:
				if (loginType == LoginType.Email)
				{
					loginResult.ErrorMessage = Resources.personalization_service_error_KLI0003_email;
				}
				else
				{
					loginResult.ErrorMessage = Resources.personalization_service_error_KLI0003_phone;
				}
				break;
			case KioskLoginResponseErrors.CommunicationError:
				loginResult.ErrorMessage = Resources.personalization_service_error_KLI0004;
				break;
			default:
				LogHelper.Instance.Log("KioskClientServiceCustomerProfile.ProcessLoginResult - Warning: No specific error handler for error code: " + text + " ");
				loginResult.ErrorMessage = Resources.personalization_service_error_KLI0004;
				break;
			case KioskLoginResponseErrors.LoyaltyIsDown:
			case KioskLoginResponseErrors.LoyaltyAccountNotFound:
			case KioskLoginResponseErrors.SubscriptionServiceIsDown:
				break;
			}
		}
		if (customerProfileKioskLoginResponse?.Errors != null)
		{
			foreach (Redbox.Rental.Model.Error error in customerProfileKioskLoginResponse.Errors)
			{
				object fieldsForResponseCode2 = ResponseCodeAttribute.GetFieldsForResponseCode(typeof(KioskLoginResponseErrors?), error?.Code);
				if (((fieldsForResponseCode2 != null) ? ((KioskLoginResponseErrors)fieldsForResponseCode2) : KioskLoginResponseErrors.None) == KioskLoginResponseErrors.SubscriptionServiceIsDown && loginResult?.PersonalizationSession?.SubscriptionData != null)
				{
					loginResult.PersonalizationSession.SubscriptionData.SubscriptionServiceIsDown = true;
				}
			}
		}
		if (loginResult?.PersonalizationSession?.LoyaltySession?.SignInError == true)
		{
			loginResult.ErrorMessage = Resources.personalization_service_error_KLI0005;
		}
		loginResultCallback?.Invoke(loginResult);
	}

	private static void PopulatePersonalizationSession(ICustomerProfileKioskLoginResponse customerProfileKioskLoginResponse, LoginResult loginResult)
	{
		KioskLoginResponse response = customerProfileKioskLoginResponse.Response;
		loginResult.PersonalizationSession = new PersonalizationSession();
		if (response?.Customer != null)
		{
			loginResult.PersonalizationSession.CustomerProfileNumber = response.Customer?.CPN;
			loginResult.PersonalizationSession.LoginEmailAddress = response.Customer?.LoginEmail;
			loginResult.PersonalizationSession.MobilePhoneNumber = response.Customer?.MobilePhoneNumber;
			loginResult.PersonalizationSession.FirstName = response.Customer.FirstName;
			loginResult.PersonalizationSession.PromptForPerks = response.Customer.PromptForPerks == true;
			loginResult.PersonalizationSession.PromptForEarlyId = response.Customer.PromptForEarlyId == true;
			loginResult.PersonalizationSession.TextClubEnabled = response.Customer.TextClubEnabled == true;
		}
		loginResult.PersonalizationSession.IsLoggedIn = true;
		loginResult.PersonalizationSession.RecommendedTitles = new List<IRecommendedTitles>();
		if (response?.Recommendations?.Products != null)
		{
			loginResult.PersonalizationSession.RecommendedTitles = new List<IRecommendedTitles>();
			RecommendedTitles recommendedTitles = new RecommendedTitles
			{
				TivoQueryId = response?.Recommendations.QueryId,
				OfferCode = response?.Recommendations?.OfferCode
			};
			loginResult.PersonalizationSession.RecommendedTitles.Add(recommendedTitles);
			foreach (KioskTitleInfo product in response.Recommendations.Products)
			{
				if (long.TryParse(product.Id, out var result))
				{
					RecommendedTitlesIdType idType = RecommendedTitlesIdType.ProductGroupID;
					if (Enum.TryParse<RecommendedTitlesIdType>(product.Type, ignoreCase: true, out var result2))
					{
						idType = result2;
					}
					RecommendedTitleId item = new RecommendedTitleId
					{
						Id = result,
						IdType = idType,
						PersonalizationTitleTags = product.Tags
					};
					recommendedTitles.Add(item);
				}
			}
		}
		loginResult.PersonalizationSession.StoredPromoCodes = new StoredPromoCodeCollection();
		if (response?.PromoCodes != null)
		{
			foreach (KioskStoredPromoCode promoCode in response.PromoCodes)
			{
				StoredPromoCode item2 = new StoredPromoCode
				{
					PromoCode = promoCode.Code,
					CampaignId = promoCode.CampaignId,
					CampaignName = promoCode.CampaignName,
					UsageDescription = promoCode.PromotionUsage,
					RedemptionTypeDescription = promoCode.RedemptionType,
					ExpirationDate = promoCode.ExpirationDate,
					ExpirationText = ((promoCode.ExpirationText != null) ? promoCode.ExpirationText : string.Format(Resources.offer_expires_message, promoCode.ExpirationDate.ToString("M/d/yyyy"))),
					DaysUntilExpiration = (promoCode.DaysUntilExpiration.HasValue ? promoCode.DaysUntilExpiration : new int?((promoCode.ExpirationDate.Date - DateTime.Today).Days))
				};
				loginResult.PersonalizationSession.StoredPromoCodes.Add(item2);
			}
		}
		loginResult.PersonalizationSession.AcceptedOffers = new List<IAcceptedOffer>();
		if (response?.AcceptedOffers != null)
		{
			foreach (KioskAcceptedOffer acceptedOffer in response.AcceptedOffers)
			{
				AcceptedOffer item3 = new AcceptedOffer
				{
					Code = acceptedOffer.Code,
					StartDate = acceptedOffer.StartDate,
					EndDate = acceptedOffer.EndDate,
					Name = acceptedOffer.Name,
					Status = acceptedOffer.Status,
					Description = acceptedOffer.Description,
					Partner = acceptedOffer.Partner,
					OptedInDate = acceptedOffer.OptedInDate,
					TimeToComplete = acceptedOffer.TimeToComplete,
					CompleteDate = acceptedOffer.CompleteDate,
					ShowInBannerPriority = acceptedOffer.ShowInBannerPriority,
					LegalInformation = acceptedOffer.LegalInformation,
					ProgressTrackerCode = acceptedOffer.ProgressTrackerCode,
					CurrentValue = acceptedOffer.CurrentValue,
					MaxValue = acceptedOffer.MaxValue,
					RemainderValue = acceptedOffer.RemainderValue,
					EndValueText = acceptedOffer.EndValueText
				};
				loginResult.PersonalizationSession.AcceptedOffers.Add(item3);
			}
		}
		if (response?.Loyalty != null && response.Loyalty.IsEnrolled)
		{
			loginResult.PersonalizationSession.LoyaltySession = new LoyaltySession();
			loginResult.PersonalizationSession.LoyaltySession.SignInError = !response.Loyalty.LoyaltySystemOnline;
			loginResult.PersonalizationSession.LoyaltySession.PointsBalance = response.Loyalty.PointBalance;
			LoyaltyTiers loyaltyTier = LoyaltyTiers.None;
			if (Enum.TryParse<LoyaltyTiers>(response.Loyalty.CurrentTier, out var result3))
			{
				loyaltyTier = result3;
			}
			loginResult.PersonalizationSession.LoyaltySession.LoyaltyTier = loyaltyTier;
			loginResult.PersonalizationSession.LoyaltySession.LoyaltyTierName = response.Loyalty.CurrentTier;
			loginResult.PersonalizationSession.LoyaltySession.LoyaltyTierCounter = response.Loyalty.CurrentTierCounter;
			loginResult.PersonalizationSession.LoyaltySession.PointsExpiring = response.Loyalty.PointsExpiring;
			loginResult.PersonalizationSession.LoyaltySession.PointsExpirationDate = response.Loyalty.PointsExpirationDate;
		}
		loginResult.PersonalizationSession.SubscriptionData = new SubscriptionData();
		if (response?.Subscriptions?.ExistingSubscriptions != null)
		{
			foreach (KioskSubscription existingSubscription3 in response.Subscriptions.ExistingSubscriptions)
			{
				SubscriptionPlanStatus status = SubscriptionPlanStatus.Active;
				if (Enum.TryParse<SubscriptionPlanStatus>(existingSubscription3.status.ToString(), ignoreCase: true, out var result4))
				{
					status = result4;
				}
				IExistingSubscription existingSubscription = new ExistingSubscription
				{
					CustomerSubscriptionId = existingSubscription3.CustomerSubscriptionId,
					SubscriptionId = existingSubscription3.SubscriptionId,
					SubscriptionPlanId = existingSubscription3.SubscriptionPlanId,
					CampaignId = existingSubscription3.CampaignId,
					EndDate = existingSubscription3.EndDate,
					SubscriptionTier = existingSubscription3.SubscriptionTier,
					Status = status
				};
				if (existingSubscription3?.BenefitUsages != null)
				{
					if (existingSubscription.BenefitUsages == null)
					{
						existingSubscription.BenefitUsages = new BenefitUsageList();
					}
					foreach (KioskBenefitUsage benefitUsage2 in existingSubscription3.BenefitUsages)
					{
						existingSubscription.BenefitUsages.Add(new BenefitUsage
						{
							BenefitId = benefitUsage2.BenefitId,
							Issued = benefitUsage2.Issued,
							Used = benefitUsage2.Used
						});
					}
				}
				loginResult.PersonalizationSession.SubscriptionData.ExistingSubscriptions.Add(existingSubscription);
			}
		}
		IExistingSubscription existingSubscription2 = loginResult.PersonalizationSession.SubscriptionData.ExistingSubscriptions.FirstOrDefault((IExistingSubscription x) => !x.HasSubscriptionEnded && x.SubscriptionId == "RBPLUS");
		if (existingSubscription2 != null)
		{
			RedboxPlusSession redboxPlusSession = new RedboxPlusSession();
			RedboxPlusSubscriptionTier subscriptionTier = RedboxPlusSubscriptionTier.Tier1;
			if (Enum.TryParse<RedboxPlusSubscriptionTier>(existingSubscription2.SubscriptionTier, out var result5))
			{
				subscriptionTier = result5;
			}
			IBenefitUsage benefitUsage = existingSubscription2?.BenefitUsages?.FirstOrDefault((IBenefitUsage x) => x.BenefitId == "EXT_NGHT_1500");
			redboxPlusSession.RedboxPlusPlanInfo = new RedboxPlusPlanInfo
			{
				SubscriptionStatus = existingSubscription2.Status,
				SubscriptionEndDate = existingSubscription2.EndDate,
				PlanId = existingSubscription2.SubscriptionId,
				SubscriptionTier = subscriptionTier,
				IsEarnPointsFor1ExtraNightAvailable = (benefitUsage != null && benefitUsage.Used == 0)
			};
			loginResult.PersonalizationSession.RedboxPlusSession = redboxPlusSession;
		}
		if (response?.Subscriptions?.EligibleSubscriptionPlanIds != null)
		{
			loginResult.PersonalizationSession.SubscriptionData.EligibleSubscriptionPlanIds.AddRange(response.Subscriptions.EligibleSubscriptionPlanIds);
		}
	}
}
