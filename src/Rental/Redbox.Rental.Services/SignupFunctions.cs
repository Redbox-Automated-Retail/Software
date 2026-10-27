using System;
using System.Security;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.Rental.Model;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Personalization;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.Services.Controllers;
using Redbox.Rental.Services.Personalization;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services;

public class SignupFunctions
{
	public class EmailEntryInfo
	{
		public string IdleTimerName { get; set; }

		public string EmailAddress { get; set; }

		public Action<string> SetEmailAddressAction { get; set; }

		public Action TimeoutAction { get; set; }

		public Action<IEmailConfirmResult> ProcessSuccessfulConfirmEmailResult { get; set; }

		public KeyboardMode KeyboardMode { get; set; }

		public string KeyboardViewName { get; set; }
	}

	public static void SendKioskOptInForMarketingCampaignMessages(KioskOptInItemId kioskOptInItemId, ConfirmationStatus currentConfirmationStatus, string emailAddress)
	{
		switch (currentConfirmationStatus)
		{
		case ConfirmationStatus.NotSubscribedConfirmed:
		case ConfirmationStatus.NotSubscribedNotConfirmed:
		case ConfirmationStatus.NewCustomer:
			CheckoutFunctions.SendKioskOptinForMarketingCampaign(KioskOptIntItemType.MarketingSubscribe, kioskOptInItemId, emailAddress);
			if (kioskOptInItemId == KioskOptInItemId.Yes)
			{
				CheckoutFunctions.SendKioskOptinForMarketingCampaign(KioskOptIntItemType.MarketingConfirm, kioskOptInItemId, emailAddress);
			}
			break;
		case ConfirmationStatus.SubscribedNotConfirmed:
			CheckoutFunctions.SendKioskOptinForMarketingCampaign(KioskOptIntItemType.MarketingConfirm, kioskOptInItemId, emailAddress);
			break;
		}
		if (Redbox.Rental.Services.Session.Session.Instance?.BrowseState?.SignupFlowState != null)
		{
			Redbox.Rental.Services.Session.Session.Instance.BrowseState.SignupFlowState.MarketingOptInShown = true;
		}
	}

	public static void CallUpdateAccountImmediate(string phoneNumber, SecureString pin, string emailAddress, string customerProfileNumber, bool optinForTextClub, bool acceptedPerksSignup, Action<IUpdateAccountResult> callback, AccountSourceType source = AccountSourceType.General)
	{
		IPersonalizationService service = ServiceLocator.Instance.GetService<IPersonalizationService>();
		LogHelper.Instance.Log("Calling PersonalizationService.UpdateAccount");
		service.UpdateAccount(customerProfileNumber, emailAddress, phoneNumber, pin, acceptedPerksSignup, optinForTextClub, callback, queued: false, source);
	}

	public static void SetNewAccountLoggedInState(IUpdateAccountResult updateAccountResult, string emailAddress, string mobilePhoneNumber, bool textClubEnabled)
	{
		if (Redbox.Rental.Services.Session.Session.Instance == null)
		{
			return;
		}
		IPersonalizationSession personalizationSession = null;
		if (Redbox.Rental.Services.Session.Session.Instance.PersonalizationSession == null)
		{
			personalizationSession = new PersonalizationSession();
			personalizationSession.IsLoggedIn = true;
			personalizationSession.IsCPNewInThisSession = true;
			personalizationSession.PromptForPerks = false;
			personalizationSession.PromptForEarlyId = false;
			personalizationSession.SubscriptionData = new SubscriptionData();
			(ServiceLocator.Instance.GetService<IDataService>()?.GetRedboxPlusSubscriptionProducts())?.ForEach(delegate(IRedboxPlusSubscriptionProduct x)
			{
				personalizationSession.SubscriptionData.EligibleSubscriptionPlanIds.Add(x.SubscriptionId);
			});
			personalizationSession.SubscriptionData.TempPassword = updateAccountResult.TempPassword;
			personalizationSession.LoyaltySession = new LoyaltySession();
			personalizationSession.LoyaltySession.PointsBalance = 0;
			LoyaltyTiers loyaltyTier = LoyaltyTiers.PerksMember;
			personalizationSession.LoyaltySession.LoyaltyTier = loyaltyTier;
			personalizationSession.LoyaltySession.LoyaltyTierName = loyaltyTier.ToString();
			Redbox.Rental.Services.Session.Session.Instance.PersonalizationSession = personalizationSession;
		}
		else
		{
			personalizationSession = Redbox.Rental.Services.Session.Session.Instance.PersonalizationSession;
		}
		personalizationSession.CustomerProfileNumber = updateAccountResult?.CustomerProfileNumber;
		personalizationSession.LoginEmailAddress = emailAddress;
		personalizationSession.MobilePhoneNumber = mobilePhoneNumber;
		personalizationSession.TextClubEnabled = textClubEnabled;
		Redbox.Rental.Services.Session.Session.Instance.UserInfo = new UserInfo
		{
			Email = emailAddress,
			CustomerProfileNumber = updateAccountResult?.CustomerProfileNumber
		};
	}

	public static void AddSubscriptionAndReturnToShoppingCartOrBrowseView(ISubscriptionProduct subscriptionProduct)
	{
		IRentalShoppingCartService service = ServiceLocator.Instance.GetService<IRentalShoppingCartService>();
		IRentalShoppingCart rentalShoppingCart = service?.CurrentRentalShoppingCart;
		if (rentalShoppingCart != null && service != null)
		{
			rentalShoppingCart.AddSubscriptionItem(service?.CreateRentalShoppingCartItemFromSubscriptionProduct(subscriptionProduct));
		}
		ReturnToShoppingCartOrBrowseView("added_subscription_to_cart");
	}

	public static void UpdateMarketingOptInConfirmationStatus(string emailAddress)
	{
		IRentalShoppingCart rentalShoppingCart = ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.CurrentRentalShoppingCart;
		if (rentalShoppingCart != null && rentalShoppingCart.MarketingOptInConfirmationStatus != OptInConfirmationStatus.SubscribedConfirmed)
		{
			ConfirmationStatus marketingOptInConfirmationStatus = (ConfirmationStatus)rentalShoppingCart.MarketingOptInConfirmationStatus;
			if (marketingOptInConfirmationStatus != ConfirmationStatus.SubscribedConfirmed)
			{
				SendKioskOptInForMarketingCampaignMessages(KioskOptInItemId.Yes, marketingOptInConfirmationStatus, emailAddress);
			}
		}
	}

	public static void ReturnToShoppingCartOrBrowseView(string context)
	{
		if (OrchestrationService.PeekViewFrameByName("shopping_cart_view", out var _) == null)
		{
			if (OrchestrationService.PeekViewFrameByName("browse_view", out var _) != null)
			{
				OrchestrationService.UnwindToSafe("browse_view", discard: true, context);
			}
		}
		else
		{
			OrchestrationService.UnwindToSafe("shopping_cart_view", discard: true, context);
		}
	}

	public static void ShowEnterEmailView(EmailEntryInfo emailEntryInfo)
	{
		emailEntryInfo.TimeoutAction = delegate
		{
			OrchestrationService.RemoveIdleTimer(emailEntryInfo?.IdleTimerName);
			OrchestrationService.ViewGoBackSafe("Signup Email Entry Timeout", popOnly: false);
			LogHelper.Instance.Log("Finished Sign up flow with timeout.");
			ReturnToShoppingCartOrBrowseView("timeout. exiting sign up flow.");
		};
		IIdleTimer idleTimer = ConfigureIdleTimer(emailEntryInfo);
		KeyboardInfo parameter = new KeyboardInfo
		{
			MaxLength = 64,
			KeyboardText = null,
			IsValidEntry = ValidationServices.IsValidEmailAddress,
			ErrorMessage = string.Empty,
			KeepKeyboardTextOnInitialErrorMessage = true,
			CancelAction = delegate
			{
				idleTimer?.Stop();
				OrchestrationService.ViewGoBackSafe("Signup Email Entry Cancel", popOnly: false);
			},
			ContinueAction = delegate(string email)
			{
				idleTimer?.Stop();
				emailEntryInfo?.SetEmailAddressAction(email);
				emailEntryInfo.EmailAddress = email;
				ValidateEmail(email, emailEntryInfo);
			},
			KeyboardMode = emailEntryInfo.KeyboardMode
		};
		idleTimer?.Start();
		OrchestrationService.ViewShow(emailEntryInfo.KeyboardViewName, parameter);
	}

	private static void ValidateEmail(string email, EmailEntryInfo emailEntryInfo)
	{
		OrchestrationService.ViewShowSafe("please_wait_view");
		IMarketingServices service = ServiceLocator.Instance.GetService<IMarketingServices>();
		long kioskid = ServiceLocator.Instance.GetService<IStoreManager>()?.KioskId ?? 0;
		SendEmailConfirmCallback completeCallback = delegate(IEmailConfirmResult emailConfirmResult)
		{
			ProcessConfirmEmailResult(emailConfirmResult, emailEntryInfo);
		};
		service.SendEmailConfirm(kioskid, email, null, completeCallback);
	}

	private static IIdleTimer ConfigureIdleTimer(EmailEntryInfo emailEntryInfo)
	{
		IIdleTimer idleTimer = OrchestrationService.CreateIdleTimer(emailEntryInfo.IdleTimerName);
		int num = OrchestrationService.TTSGetTimeout("Medium", 40000);
		ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		num = ((service != null && service.TTSEnabled) ? 90000 : num);
		if (idleTimer != null)
		{
			idleTimer.Initialize(num, delegate
			{
				ShowTimeoutDialog(emailEntryInfo);
			});
			return idleTimer;
		}
		return idleTimer;
	}

	private static void ShowTimeoutDialog(EmailEntryInfo emailEntryParameters)
	{
		OrchestrationService.StopIdleTimer(emailEntryParameters.IdleTimerName);
		IdleTimeoutMessageViewParameter parameter = new IdleTimeoutMessageViewParameter
		{
			IdleTimerName = emailEntryParameters.IdleTimerName,
			NoAction = delegate
			{
				OrchestrationService.ViewGoBackSafe("need_more_time:first_button", popOnly: false);
				emailEntryParameters.TimeoutAction?.Invoke();
			},
			YesAction = delegate
			{
				ServiceLocator.Instance.GetService<IViewService>();
				OrchestrationService.ViewGoBackSafe("need_more_time:second_button", popOnly: false);
				OrchestrationService.StartIdleTimer(emailEntryParameters.IdleTimerName);
			}
		};
		OrchestrationService.ViewShow("idle_timeout_message_view", parameter);
	}

	private static void ProcessConfirmEmailResult(IEmailConfirmResult result, EmailEntryInfo emailEntryInfo)
	{
		if (result.Success && (!string.IsNullOrEmpty(result.CustomerProfileNumber) || result.EmailVerified == true))
		{
			emailEntryInfo?.ProcessSuccessfulConfirmEmailResult(result);
			return;
		}
		if (!result.Success)
		{
			string empty = string.Empty;
			empty = GetErrorsAsString(result.Errors, empty);
			LogHelper.Instance.Log("ProcessConfirmEmailResult ConfirmEmail unsuccessful. " + empty);
		}
		else
		{
			LogHelper.Instance.Log("ProcessConfirmEmailResult Email verify failed.");
		}
		LogHelper.Instance.Log("returning to keyboard view to display error");
		OrchestrationService.StartIdleTimer(emailEntryInfo?.IdleTimerName);
		OrchestrationService.ViewGoBackSafe("ProcessConfirmEmailResult", popOnly: false);
		IViewService service = ServiceLocator.Instance.GetService<IViewService>();
		IViewFrameInstance viewFrameInstance = service?.PeekViewFrame();
		if (service != null && viewFrameInstance != null && viewFrameInstance?.ViewFrame?.ViewName == emailEntryInfo?.KeyboardViewName)
		{
			if (viewFrameInstance.Parameter is KeyboardInfo keyboardInfo)
			{
				OrchestrationService.SetCurrentUICulture();
				keyboardInfo.ErrorMessage = Resources.receipt_email_error;
				keyboardInfo.KeyboardText = emailEntryInfo?.EmailAddress;
				keyboardInfo.KeyboardCode = string.Empty;
			}
			service.Show();
		}
	}

	private static string GetErrorsAsString(ErrorList errors, string errorMessages)
	{
		if (errors != null)
		{
			foreach (Redbox.KioskEngine.ComponentModel.Error error in errors)
			{
				errorMessages = errorMessages + " [" + error.Code + "] " + error.Description + ".";
			}
		}
		return errorMessages;
	}
}
