using System;
using Redbox.Core;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Personalization;
using Redbox.Rental.Services.Controllers;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI.Controls;

namespace Redbox.Rental.Services.RedboxPlus;

public class RedboxPlusSignupService
{
	private static string _emailAddress;

	private static IRedboxPlusSubscriptionProduct _redboxPlusSubscriptionProduct;

	private const string IdleTimerName = "redbox_plus_signup_idle_timer";

	public static void StartRedboxPlusSignup(IRedboxPlusSubscriptionProduct redboxPlusSubscriptionProduct)
	{
		_redboxPlusSubscriptionProduct = redboxPlusSubscriptionProduct;
		IPersonalizationSession personalizationSession = Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession;
		if (personalizationSession != null && personalizationSession.IsLoggedIn)
		{
			_emailAddress = personalizationSession.LoginEmailAddress;
			ConfirmAccountIsEligibleForRedboxPlusOffer();
			return;
		}
		SignupFunctions.ShowEnterEmailView(new SignupFunctions.EmailEntryInfo
		{
			IdleTimerName = "redbox_plus_signup_idle_timer",
			SetEmailAddressAction = SetEmailAddress,
			ProcessSuccessfulConfirmEmailResult = ProcessSuccessfulConfirmEmailResult,
			KeyboardMode = KeyboardMode.REDBOX_PLUS_SIGN_UP,
			KeyboardViewName = "keyboard_redbox_plus_signup_view"
		});
	}

	private static void ConfirmAccountIsEligibleForRedboxPlusOffer()
	{
		bool flag = true;
		IPersonalizationSession personalizationSession = Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession;
		if (personalizationSession != null)
		{
			if (personalizationSession?.SubscriptionData?.SubscriptionServiceIsDown == true)
			{
				LogHelper.Instance.Log("Unable to sign up for Redbox+ because the subscription service is down.");
				flag = false;
				MessagePopupViewController.ShowRedboxPlusGenericErrorMessage(continueAction);
			}
			else if (personalizationSession?.RedboxPlusSession?.HasActiveSubscription == true)
			{
				LogHelper.Instance.Log("Customer with CPN " + personalizationSession?.CustomerProfileNumber + " already has a Redbox+ subscription.");
				flag = false;
				MessagePopupViewController.ShowRedboxPlusExistingSubscriptionErrorMessage(continueAction);
			}
			else if (personalizationSession?.SubscriptionData != null && !personalizationSession.SubscriptionData.IsEligibleForSubscription(_redboxPlusSubscriptionProduct?.SubscriptionId))
			{
				LogHelper.Instance.Log("Customer with CPN " + personalizationSession?.CustomerProfileNumber + " is not eligible for Redbox+ subscription " + _redboxPlusSubscriptionProduct?.SubscriptionId + ".");
				flag = false;
				MessagePopupViewController.ShowRedboxPlusGenericErrorMessage(continueAction);
			}
		}
		if (flag)
		{
			PromptUserToConfirmEnrollment();
		}
		static void continueAction()
		{
			SignupFunctions.ReturnToShoppingCartOrBrowseView(null);
		}
	}

	private static void PromptUserToConfirmEnrollment()
	{
		OrchestrationService.ViewShow("redbox_plus_terms_acceptance_view", new RedboxPlusTermsAcceptanceViewParameters
		{
			RedboxPlusSubscriptionProduct = _redboxPlusSubscriptionProduct,
			EmailAddress = _emailAddress
		});
	}

	public static void AddRedboxPlusSubscriptionPrerequisites(IRedboxPlusSubscriptionProduct redboxPlusSubscriptionProduct)
	{
		OrchestrationService.ViewShow("please_wait_view");
		SignupFunctions.UpdateMarketingOptInConfirmationStatus(_emailAddress);
		if (Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession == null || Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession?.PromptForPerks == true)
		{
			Action<IUpdateAccountResult> callback = delegate(IUpdateAccountResult updateAccountResult)
			{
				RedboxPlusSignUpService_HandlelAccountUpdateResult(updateAccountResult);
			};
			SignupFunctions.CallUpdateAccountImmediate(null, null, _emailAddress, Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession?.CustomerProfileNumber, optinForTextClub: false, acceptedPerksSignup: true, callback);
			Redbox.Rental.Services.Session.Session.Instance.BrowseState.SignupFlowState.WasPerksSignupOfferPresented = true;
		}
		else
		{
			SignupFunctions.AddSubscriptionAndReturnToShoppingCartOrBrowseView(_redboxPlusSubscriptionProduct);
		}
	}

	private static void RedboxPlusSignUpService_HandlelAccountUpdateResult(IUpdateAccountResult updateAccountResult)
	{
		if (updateAccountResult.Success)
		{
			if (Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession == null)
			{
				SignupFunctions.SetNewAccountLoggedInState(updateAccountResult, _emailAddress, null, textClubEnabled: false);
			}
			else
			{
				Redbox.Rental.Services.Session.Session.Instance.PersonalizationSession.PromptForPerks = false;
			}
			SignupFunctions.AddSubscriptionAndReturnToShoppingCartOrBrowseView(_redboxPlusSubscriptionProduct);
		}
		else
		{
			MessagePopupViewController.ShowRedboxPlusGenericErrorMessage(continueAction);
		}
		static void continueAction()
		{
			SignupFunctions.ReturnToShoppingCartOrBrowseView("Unable to update account and add RedboxPlus subscription to cart");
		}
	}

	private static void SetEmailAddress(string emailAddress)
	{
		_emailAddress = emailAddress;
	}

	private static void ProcessSuccessfulConfirmEmailResult(IEmailConfirmResult result)
	{
		if (!string.IsNullOrEmpty(result.CustomerProfileNumber))
		{
			MessagePopupViewController.ShowRedboxPlusExistingAccountErrorMessage(ContinueAction);
		}
		else
		{
			ConfirmAccountIsEligibleForRedboxPlusOffer();
		}
		static void ContinueAction()
		{
			OrchestrationService.UnwindToSafe("sign_in_view", discard: true, null);
		}
	}
}
