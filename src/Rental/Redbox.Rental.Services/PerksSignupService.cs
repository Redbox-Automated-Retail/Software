using System;
using System.Security;
using Newtonsoft.Json;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Analytics;
using Redbox.Rental.Model.KioskHealth;
using Redbox.Rental.Model.Personalization;
using Redbox.Rental.Model.Promotion;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.Services.Configuration;
using Redbox.Rental.Services.Controllers;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services;

public class PerksSignupService : IPerksSignupService
{
	private class ThanksDialogStrings
	{
		public string Message { get; set; }

		public string Comment { get; set; }
	}

	private Action<ISignupFlowResult> _callbackAction;

	private Action<ISignupFlowResult> _timeoutCallbackAction;

	private static PerksSignupService _instance;

	private string _emailAddress;

	private string _accountNumber;

	private string _customerProfileNumber;

	private string _mobilePhoneNumber;

	private SecureString _pin;

	private bool _optinForTextClub;

	private bool _textClubEnabled;

	private bool _promptForPerks;

	private bool _promptForEarlyId;

	private bool? _emailVerified;

	private SignUpFlowContext _signUpFlowContext;

	private bool _enableEarlyIdSignup;

	private ConfirmationStatus _confirmationStatus;

	private Action _timeoutAction;

	private bool _forcePerksPromptEntry;

	private ISignupFlowResult _signupFlowResult;

	private const string IdleTimerName = "perks_signup_idle_timer";

	public static IPerksSignupService Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = new PerksSignupService();
			}
			return _instance;
		}
	}

	private bool HasSuccessfulEmailConfirmResult
	{
		get
		{
			if (Redbox.Rental.Services.Session.Session.Instance?.EmailConfirmResult != null && Redbox.Rental.Services.Session.Session.Instance.EmailConfirmResult.Success)
			{
				if (Redbox.Rental.Services.Session.Session.Instance.EmailConfirmResult.EmailVerified != true)
				{
					return !string.IsNullOrWhiteSpace(Redbox.Rental.Services.Session.Session.Instance.EmailConfirmResult.CustomerProfileNumber);
				}
				return true;
			}
			return false;
		}
	}

	private bool WasPerksSignupOfferPresented => Redbox.Rental.Services.Session.Session.Instance?.BrowseState?.SignupFlowState?.WasPerksSignupOfferPresented == true;

	private bool WasEarlyIdSignupPresented
	{
		get
		{
			return Redbox.Rental.Services.Session.Session.Instance?.BrowseState?.SignupFlowState?.WasEarlyIdSignupPresented == true;
		}
		set
		{
			if (Redbox.Rental.Services.Session.Session.Instance?.BrowseState?.SignupFlowState != null)
			{
				Redbox.Rental.Services.Session.Session.Instance.BrowseState.SignupFlowState.WasEarlyIdSignupPresented = value;
			}
		}
	}

	private bool UserInvokedSignUp
	{
		get
		{
			if (_signUpFlowContext != SignUpFlowContext.SignUp)
			{
				return _signUpFlowContext == SignUpFlowContext.AfterPerksEquivalentOffer;
			}
			return true;
		}
	}

	private static IIdleTimer IdleTimer { get; set; }

	private PerksSignupService()
	{
	}

	public void StartSignupFlow(Action<ISignupFlowResult> callbackAction, SignUpFlowContext signUpFlowContext, Action<ISignupFlowResult> timeoutCallbackAction)
	{
		InitializeVariables(callbackAction, signUpFlowContext, timeoutCallbackAction);
		LogHelper.Instance.Log($"Starting Sign up flow with context: {_signUpFlowContext}");
		ConfigureIdleTimer();
		switch (signUpFlowContext)
		{
		case SignUpFlowContext.AfterSignIn:
		case SignUpFlowContext.AfterSwipeAndRedboxPlusSignup:
			SignUpFlow_StartSignUpDialogs();
			return;
		case SignUpFlowContext.AfterSwipe:
			if (!string.IsNullOrEmpty(_emailAddress))
			{
				if (!HasSuccessfulEmailConfirmResult)
				{
					ValidateEmail(_emailAddress);
					return;
				}
				_customerProfileNumber = Redbox.Rental.Services.Session.Session.Instance.EmailConfirmResult.CustomerProfileNumber;
				ProcessConfirmEmailResult(Redbox.Rental.Services.Session.Session.Instance.EmailConfirmResult);
			}
			else
			{
				ExitSignupFlow();
			}
			return;
		case SignUpFlowContext.AfterPerksEquivalentOffer:
			_forcePerksPromptEntry = true;
			break;
		}
		if (HasSuccessfulEmailConfirmResult)
		{
			_customerProfileNumber = Redbox.Rental.Services.Session.Session.Instance.EmailConfirmResult.CustomerProfileNumber;
			ProcessConfirmEmailResult(Redbox.Rental.Services.Session.Session.Instance.EmailConfirmResult);
		}
		else
		{
			ShowEnterEmailView();
		}
	}

	private void InitializeVariables(Action<ISignupFlowResult> callbackAction, SignUpFlowContext signUpFlowContext, Action<ISignupFlowResult> timeoutCallbackAction)
	{
		_forcePerksPromptEntry = false;
		_signupFlowResult = new SignupFlowResult();
		_callbackAction = callbackAction;
		_timeoutCallbackAction = timeoutCallbackAction;
		_signUpFlowContext = signUpFlowContext;
		_enableEarlyIdSignup = _signUpFlowContext != SignUpFlowContext.AfterPerksEquivalentOffer;
		_promptForPerks = false;
		_promptForEarlyId = false;
		_emailVerified = false;
		_mobilePhoneNumber = null;
		_pin = null;
		_optinForTextClub = false;
		_emailAddress = string.Empty;
		_accountNumber = string.Empty;
		OptInConfirmationStatus valueOrDefault = (ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.CurrentRentalShoppingCart?.MarketingOptInConfirmationStatus).GetValueOrDefault();
		_confirmationStatus = (ConfirmationStatus)valueOrDefault;
		switch (signUpFlowContext)
		{
		case SignUpFlowContext.AfterSwipeAndRedboxPlusSignup:
			if (Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession != null)
			{
				_emailAddress = Redbox.Rental.Services.Session.Session.Instance.PersonalizationSession.LoginEmailAddress;
				_accountNumber = null;
				_customerProfileNumber = Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession.CustomerProfileNumber;
				_promptForEarlyId = true;
				_mobilePhoneNumber = Redbox.Rental.Services.Session.Session.Instance.PersonalizationSession.MobilePhoneNumber;
				_textClubEnabled = Redbox.Rental.Services.Session.Session.Instance.PersonalizationSession.TextClubEnabled;
			}
			break;
		case SignUpFlowContext.AfterSignIn:
			if (Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession != null)
			{
				_emailAddress = Redbox.Rental.Services.Session.Session.Instance.PersonalizationSession.LoginEmailAddress;
				_accountNumber = null;
				_customerProfileNumber = Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession.CustomerProfileNumber;
				_promptForPerks = Redbox.Rental.Services.Session.Session.Instance.PersonalizationSession.PromptForPerks;
				_promptForEarlyId = Redbox.Rental.Services.Session.Session.Instance.PersonalizationSession.PromptForEarlyId;
				_mobilePhoneNumber = Redbox.Rental.Services.Session.Session.Instance.PersonalizationSession.MobilePhoneNumber;
				_textClubEnabled = Redbox.Rental.Services.Session.Session.Instance.PersonalizationSession.TextClubEnabled;
			}
			break;
		default:
			if (Redbox.Rental.Services.Session.Session.Instance.UserInfo != null)
			{
				_emailAddress = Redbox.Rental.Services.Session.Session.Instance.UserInfo.Email;
				_accountNumber = Redbox.Rental.Services.Session.Session.Instance.UserInfo.AccountNumber;
				_customerProfileNumber = Redbox.Rental.Services.Session.Session.Instance.UserInfo.CustomerProfileNumber;
			}
			else
			{
				_emailAddress = null;
				_accountNumber = null;
				_customerProfileNumber = null;
			}
			break;
		}
	}

	private void ShowEnterEmailView()
	{
		KeyboardInfo parameter = new KeyboardInfo
		{
			MaxLength = 64,
			KeyboardText = _emailAddress,
			IsValidEntry = ValidationServices.IsValidEmailAddress,
			ErrorMessage = string.Empty,
			KeepKeyboardTextOnInitialErrorMessage = true,
			CancelAction = delegate
			{
				IdleTimer.Stop();
				OrchestrationService.ViewGoBackSafe("Signup Email Entry Cancel", popOnly: false);
				ExitSignupFlow();
			},
			ContinueAction = delegate(string email)
			{
				IdleTimer.Stop();
				ValidateEmail(email);
			},
			KeyboardMode = KeyboardMode.PERKS_SIGN_UP
		};
		IdleTimer.Start();
		_timeoutAction = delegate
		{
			OrchestrationService.ViewGoBackSafe("Signup Email Entry Timeout", popOnly: false);
			TimeoutSignupFlow();
		};
		OrchestrationService.ViewShow("keyboard_perks_signup_view", parameter);
	}

	private void ValidateEmail(string email)
	{
		IMarketingServices service = ServiceLocator.Instance.GetService<IMarketingServices>();
		long kioskid = ServiceLocator.Instance.GetService<IStoreManager>()?.KioskId ?? 0;
		SendEmailConfirmCallback completeCallback = delegate(IEmailConfirmResult emailConfirmResult)
		{
			ProcessConfirmEmailResult(emailConfirmResult);
		};
		_emailAddress = email;
		service.SendEmailConfirm(kioskid, email, _accountNumber, completeCallback);
	}

	private void ProcessConfirmEmailResult(IEmailConfirmResult result)
	{
		if (Redbox.Rental.Services.Session.Session.Instance != null && _signUpFlowContext != SignUpFlowContext.SignUp)
		{
			Redbox.Rental.Services.Session.Session.Instance.EmailConfirmResult = result;
		}
		if (result.Success)
		{
			ProcessEmailConfirmResult(result);
		}
		if (result.Success && (result.EmailVerified == true || !string.IsNullOrWhiteSpace(_customerProfileNumber)))
		{
			SignUpFlow_StartSignUpDialogs();
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
		if (!UserInvokedSignUp)
		{
			ExitSignupFlow();
			return;
		}
		IViewService service = ServiceLocator.Instance.GetService<IViewService>();
		IViewFrameInstance viewFrameInstance = service?.PeekViewFrame();
		if (service != null && viewFrameInstance != null && viewFrameInstance?.ViewFrame?.ViewName == "keyboard_perks_signup_view")
		{
			LogHelper.Instance.Log("returning to keyboard view to display error");
			if (viewFrameInstance.Parameter is KeyboardInfo keyboardInfo)
			{
				OrchestrationService.SetCurrentUICulture();
				keyboardInfo.ErrorMessage = Resources.receipt_email_error;
				keyboardInfo.KeyboardText = _emailAddress;
				keyboardInfo.KeyboardCode = string.Empty;
			}
			IdleTimer.Start();
			service.Show();
		}
	}

	private void ProcessEmailConfirmResult(IEmailConfirmResult emailConfirmResult)
	{
		_customerProfileNumber = emailConfirmResult.CustomerProfileNumber;
		_mobilePhoneNumber = emailConfirmResult.MobilePhoneNumber;
		_promptForPerks = emailConfirmResult.PromptForPerks;
		_promptForEarlyId = emailConfirmResult.PromptForEarlyId && _enableEarlyIdSignup;
		_textClubEnabled = emailConfirmResult.TextClubEnabled;
		_emailVerified = emailConfirmResult.EmailVerified;
		_confirmationStatus = emailConfirmResult.ConfirmationStatus;
		IRentalShoppingCart rentalShoppingCart = ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.CurrentRentalShoppingCart;
		if (rentalShoppingCart != null)
		{
			rentalShoppingCart.MarketingOptInConfirmationStatus = (OptInConfirmationStatus)emailConfirmResult.ConfirmationStatus;
		}
	}

	private void ContinueFlowIfNotTimedout(SignupResult signupResult, Action continueAction)
	{
		if (signupResult == SignupResult.TimedOut)
		{
			TimeoutSignupFlow();
		}
		else
		{
			continueAction();
		}
	}

	private void SignUpFlow_StartSignUpDialogs()
	{
		LogHelper.Instance.Log($"Starting sign up flow dialogs.  promptForPerks: {_promptForPerks}, promptForEarlyId: {_promptForEarlyId}, confirmationStatus: {_confirmationStatus},  LoyaltySignUpAfterSwipeEnabled: {Redbox.Rental.Services.Configuration.Configuration.Instance?.LoyaltySignUpAfterSwipeEnabled}, MarketingOptInEnabled: {Redbox.Rental.Services.Configuration.Configuration.Instance?.MarketingOptInEnabled}");
		if ((!WasPerksSignupOfferPresented || _forcePerksPromptEntry) && (UserInvokedSignUp || Redbox.Rental.Services.Configuration.Configuration.Instance.LoyaltySignUpAfterSwipeEnabled) && (string.IsNullOrWhiteSpace(_customerProfileNumber) || _promptForPerks))
		{
			ShowPerksSignUpDialog(SignUpFlow_HandlePerksSignupResult);
		}
		else
		{
			SignUpFlow_HandlePerksSignupResult();
		}
	}

	private void SignUpFlow_HandlePerksSignupResult()
	{
		if (_signupFlowResult.PerksSignupResult == SignupResult.Declined && (_signUpFlowContext == SignUpFlowContext.SignUp || _signUpFlowContext == SignUpFlowContext.AfterPerksEquivalentOffer))
		{
			_forcePerksPromptEntry = true;
			OrchestrationService.ViewGoBackSafe("PerksConfirmView Back", popOnly: false);
		}
		else
		{
			ContinueFlowIfNotTimedout(_signupFlowResult.PerksSignupResult, SignUpFlow_ShowEarlyIdSignUpIfNeeded);
		}
	}

	private void SignUpFlow_ShowEarlyIdSignUpIfNeeded()
	{
		if (!WasEarlyIdSignupPresented && (_signupFlowResult.PerksSignupResult == SignupResult.Accepted || _signUpFlowContext == SignUpFlowContext.AfterSwipeAndRedboxPlusSignup) && _promptForEarlyId)
		{
			ShowSignUpForEarlyIdDialog(SignUpFlow_UpdateAccountIfNeeded);
		}
		else
		{
			SignUpFlow_UpdateAccountIfNeeded();
		}
	}

	private void SignUpFlow_UpdateAccountIfNeeded()
	{
		if (_signupFlowResult.PerksSignupResult == SignupResult.Accepted || _signupFlowResult.PhoneAndPinSignupResult == SignupResult.Accepted)
		{
			if (_signupFlowResult.PhoneAndPinSignupResult == SignupResult.TimedOut)
			{
				QueueUpdateAccount(_mobilePhoneNumber, _pin, _optinForTextClub);
				TimeoutSignupFlow();
				return;
			}
			Action<IUpdateAccountResult> callback = delegate(IUpdateAccountResult updateAccountResult)
			{
				SignUpFlow_HandlelAccountUpdateResult(updateAccountResult, SignUpFlow_ShowEarlyIdVerifyIfNeeded);
			};
			SignupFunctions.CallUpdateAccountImmediate(_mobilePhoneNumber, _pin, _emailAddress, _customerProfileNumber, _optinForTextClub, _signupFlowResult.AcceptedPerksSignup, callback);
		}
		else
		{
			ContinueFlowIfNotTimedout(_signupFlowResult.PhoneAndPinSignupResult, SignUpFlow_ShowEarlyIdVerifyIfNeeded);
		}
	}

	private void SignUpFlow_HandlelAccountUpdateResult(IUpdateAccountResult updateAccountResult, Action continueAction)
	{
		if (updateAccountResult.Success)
		{
			if (_signupFlowResult.AcceptedPerksSignup)
			{
				SignupFunctions.SetNewAccountLoggedInState(updateAccountResult, _emailAddress, _mobilePhoneNumber, _textClubEnabled);
			}
			continueAction();
		}
		else if (_signupFlowResult.AcceptedPhoneAndPinSignup)
		{
			ShowUpdateAccountErrorOnPhoneAndPinView(updateAccountResult);
		}
		else
		{
			ShowUpdateAccountErrorMessage();
		}
	}

	private void SignUpFlow_ShowEarlyIdVerifyIfNeeded()
	{
		if (_signupFlowResult.AcceptedPhoneAndPinSignup)
		{
			ShowJoinedTextClubView(SignUpFlow_HandleEarlyIdThanksResult);
		}
		else
		{
			SignUpFlow_HandleEarlyIdThanksResult();
		}
	}

	private void SignUpFlow_HandleEarlyIdThanksResult()
	{
		ContinueFlowIfNotTimedout(_signupFlowResult.EarlyIdThanksResult, SignUpFlow_ShowPerksSignUpThanksIfNeeded);
	}

	private void SignUpFlow_ShowPerksSignUpThanksIfNeeded()
	{
		if (_signupFlowResult.AcceptedPerksSignup)
		{
			ShowThanksForSigningUpForPerks(SignUpFlow_HandlePerksThanksResult);
		}
		else
		{
			SignUpFlow_HandlePerksThanksResult();
		}
	}

	private void SignUpFlow_HandlePerksThanksResult()
	{
		ContinueFlowIfNotTimedout(_signupFlowResult.PerksThanksResult, SignUpFlow_ShowMarketingEmailOptInIfNeeded);
	}

	private void SignUpFlow_ShowMarketingEmailOptInIfNeeded()
	{
		if (Redbox.Rental.Services.Session.Session.Instance?.BrowseState?.SignupFlowState?.MarketingOptInShown != true && !_signupFlowResult.AcceptedPerksSignup)
		{
			IConfiguration instance = Redbox.Rental.Services.Configuration.Configuration.Instance;
			if (instance != null && instance.MarketingOptInEnabled && (_confirmationStatus == ConfirmationStatus.NotSubscribedConfirmed || _confirmationStatus == ConfirmationStatus.NotSubscribedNotConfirmed || _confirmationStatus == ConfirmationStatus.SubscribedNotConfirmed) && ((_signUpFlowContext == SignUpFlowContext.AfterSwipe && _signupFlowResult.PerksSignupResult == SignupResult.NotShown) || _signUpFlowContext == SignUpFlowContext.AfterPerksEquivalentOffer || _signUpFlowContext == SignUpFlowContext.SignUp))
			{
				ShowEmailMarketingOptin(SignUpFlow_HandleMarketingEmailSignUpResult);
				return;
			}
		}
		SignUpFlow_HandleMarketingEmailSignUpResult();
	}

	private void SignUpFlow_HandleMarketingEmailSignUpResult()
	{
		ContinueFlowIfNotTimedout(_signupFlowResult.MarketingEmailSignupResult, SignUpFlow_ShowAlreadyMemberIfNeeded);
	}

	private void SignUpFlow_ShowAlreadyMemberIfNeeded()
	{
		if (UserInvokedSignUp && _signupFlowResult.MarketingEmailSignupResult == SignupResult.NotShown && (!_promptForPerks || !string.IsNullOrEmpty(_customerProfileNumber)))
		{
			ShowAlreadyMemberDialog(SignUpFlow_HandleAlreadyPerksMemberResult);
		}
		else
		{
			SignUpFlow_HandleAlreadyPerksMemberResult();
		}
	}

	private void SignUpFlow_HandleAlreadyPerksMemberResult()
	{
		ContinueFlowIfNotTimedout(_signupFlowResult.AlreadyPerksMemberResult, SignUpFlow_ShowMarketingEmailThanksIfNeeded);
	}

	private void SignUpFlow_ShowMarketingEmailThanksIfNeeded()
	{
		if (_signupFlowResult.AcceptedMarketingEmailSignup)
		{
			ShowEmailMarketingThanks(SignUpFlow_HandleMarketingEmailThanksResult);
		}
		else
		{
			SignUpFlow_HandleMarketingEmailThanksResult();
		}
	}

	private void SignUpFlow_HandleMarketingEmailThanksResult()
	{
		ContinueFlowIfNotTimedout(_signupFlowResult.MarketingEmailThanksResult, SignUpFlow_EndOfSignUpDialogs);
	}

	private void SignUpFlow_EndOfSignUpDialogs()
	{
		ExitSignupFlow();
	}

	private void ShowPerksSignUpDialog(Action continueAction)
	{
		_forcePerksPromptEntry = false;
		PerksConfirmViewParameters parameter = new PerksConfirmViewParameters
		{
			EmailAddress = _emailAddress,
			SignUpFlowContext = _signUpFlowContext,
			AcceptAction = delegate
			{
				_signupFlowResult.PerksSignupResult = SignupResult.Accepted;
				IdleTimer.Stop();
				ProcessWPFHit();
				SignupFunctions.SendKioskOptInForMarketingCampaignMessages(KioskOptInItemId.Yes, _confirmationStatus, _emailAddress);
				continueAction?.Invoke();
			},
			CancelAction = delegate
			{
				IdleTimer.Stop();
				ProcessWPFHit();
				_signupFlowResult.PerksSignupResult = SignupResult.Declined;
				continueAction?.Invoke();
			},
			IdleTimerName = "perks_signup_idle_timer"
		};
		_timeoutAction = delegate
		{
			IdleTimer?.Stop();
			_signupFlowResult.PerksSignupResult = SignupResult.TimedOut;
			OrchestrationService.ViewGoBackSafe("Perks Signup Confirmation Screen Timeout", popOnly: false);
			continueAction?.Invoke();
		};
		IdleTimer.Start();
		OrchestrationService.ViewShow("perks_confirm_view", parameter);
	}

	private void ShowThanksForSigningUpForPerks(Action continueAction)
	{
		if (_signUpFlowContext == SignUpFlowContext.AfterSwipe)
		{
			ShowPerksThanksWithIcons(continueAction);
		}
		else
		{
			ShowStandardPerksThanks(continueAction);
		}
	}

	private void ShowStandardPerksThanks(Action continueAction)
	{
		OrchestrationService.SetCurrentUICulture();
		PerksThanksInfo perksThanksInfo = new PerksThanksInfo
		{
			TitleText = Resources.perks_thanks_title_text,
			TopImage = PerksThanksInfo.ImageType.PerksStar,
			ShowPerksIcons = false,
			ContinueAction = delegate
			{
				IdleTimer.Stop();
				_signupFlowResult.PerksThanksResult = SignupResult.Accepted;
				OrchestrationService.ViewGoBackSafe("Signup Thanks Continue", popOnly: false);
				continueAction?.Invoke();
			}
		};
		OrchestrationService.SetCurrentUICulture();
		if (_signUpFlowContext == SignUpFlowContext.AfterPerksEquivalentOffer)
		{
			ThanksDialogStrings thanksDialogStringsForPerksOffer = GetThanksDialogStringsForPerksOffer();
			perksThanksInfo.MessageText = thanksDialogStringsForPerksOffer?.Message;
			perksThanksInfo.CommentText = thanksDialogStringsForPerksOffer?.Comment;
		}
		else if (_signUpFlowContext == SignUpFlowContext.SignUp || _signUpFlowContext == SignUpFlowContext.AfterSignIn)
		{
			perksThanksInfo.MessageText = Resources.perks_thanks_standard_message;
			perksThanksInfo.CommentText = Resources.perks_thanks_comment_text;
		}
		else
		{
			LogHelper.Instance.Log($"Error in ShowStandardThanksView.  Unexpected SignUpFlowContext value {_signUpFlowContext}");
		}
		_timeoutAction = delegate
		{
			IdleTimer.Stop();
			_signupFlowResult.PerksThanksResult = SignupResult.TimedOut;
			continueAction?.Invoke();
		};
		IdleTimer.Start();
		OrchestrationService.ViewShow("perks_thanks_view", perksThanksInfo);
	}

	private ThanksDialogStrings GetThanksDialogStringsForPerksOffer()
	{
		ThanksDialogStrings thanksDialogStrings = new ThanksDialogStrings();
		thanksDialogStrings.Comment = Resources.perks_thanks_promo_comment_text;
		CustomerOffer customerOffer = Redbox.Rental.Services.Session.Session.Instance?.PromoOfferState?.SelectedOffer;
		if (customerOffer != null)
		{
			switch (customerOffer.DiscountType)
			{
			case OfferDiscountType.AmountOff:
				thanksDialogStrings.Message = string.Format(Resources.perks_thanks_amount_off_message, customerOffer.Amount);
				break;
			case OfferDiscountType.Loyalty:
				thanksDialogStrings.Message = Resources.perks_thanks_extra_rental_points_message;
				break;
			case OfferDiscountType.Free:
				thanksDialogStrings.Message = Resources.perks_thanks_free_item_message;
				break;
			default:
				thanksDialogStrings.Message = Resources.perks_thanks_standard_message;
				thanksDialogStrings.Comment = Resources.perks_thanks_comment_text;
				break;
			}
		}
		return thanksDialogStrings;
	}

	private void ShowEmailMarketingThanks(Action continueAction)
	{
		OrchestrationService.SetCurrentUICulture();
		PerksThanksInfo parameter = new PerksThanksInfo
		{
			TitleText = Resources.perks_thanks_email_title_text,
			CommentText = string.Empty,
			MessageText = ((_signUpFlowContext == SignUpFlowContext.AfterSwipe) ? Resources.perks_thanks_email_message_text2 : Resources.perks_thanks_email_message_text),
			TopImage = PerksThanksInfo.ImageType.CheckMark,
			ShowPerksIcons = false,
			ContinueAction = delegate
			{
				IdleTimer.Stop();
				_signupFlowResult.MarketingEmailThanksResult = SignupResult.Accepted;
				continueAction();
			}
		};
		_timeoutAction = delegate
		{
			IdleTimer.Stop();
			_signupFlowResult.MarketingEmailThanksResult = SignupResult.TimedOut;
			continueAction();
		};
		IdleTimer.Start();
		OrchestrationService.ViewShow("perks_thanks_view", parameter);
	}

	private void ShowPerksThanksWithIcons(Action continueAction)
	{
		OrchestrationService.SetCurrentUICulture();
		PerksThanksInfo parameter = new PerksThanksInfo
		{
			TitleText = Resources.perks_thanks_title_youre_in_text,
			MessageText = Resources.perks_thanks_free_stuff_message,
			CommentText = Resources.perks_thanks_comment_all_details_text,
			TopImage = PerksThanksInfo.ImageType.PerksStar,
			ShowPerksIcons = true,
			ContinueAction = delegate
			{
				IdleTimer.Stop();
				_signupFlowResult.PerksThanksResult = SignupResult.Accepted;
				OrchestrationService.ViewGoBackSafe("Signup Thanks with icons Continue", popOnly: false);
				continueAction?.Invoke();
			}
		};
		_timeoutAction = delegate
		{
			IdleTimer.Stop();
			_signupFlowResult.PerksThanksResult = SignupResult.TimedOut;
			continueAction?.Invoke();
		};
		IdleTimer.Start();
		OrchestrationService.ViewShow("perks_thanks_view", parameter);
	}

	private void ShowAlreadyMemberDialog(Action continueAction)
	{
		SimpleInfoWithSignUpFlow parameter = new SimpleInfoWithSignUpFlow
		{
			SignUpFlowContext = _signUpFlowContext,
			ContinueAction = delegate
			{
				IdleTimer.Stop();
				_signupFlowResult.AlreadyPerksMemberResult = SignupResult.Accepted;
				continueAction?.Invoke();
			}
		};
		_timeoutAction = delegate
		{
			IdleTimer.Stop();
			_signupFlowResult.AlreadyPerksMemberResult = SignupResult.TimedOut;
			continueAction?.Invoke();
		};
		IdleTimer.Start();
		OrchestrationService.ViewShow("perks_signup_already_member_view", parameter);
	}

	private void ShowEmailMarketingOptin(Action continueAction)
	{
		PerksSignupAlreadyMemberOptInViewInfo parameter = new PerksSignupAlreadyMemberOptInViewInfo
		{
			EmailAddress = _emailAddress,
			PerksDeclined = (_signupFlowResult.PerksSignupResult == SignupResult.Declined),
			SignUpFlowContext = _signUpFlowContext,
			ContinueAction = delegate
			{
				IdleTimer.Stop();
				_signupFlowResult.MarketingEmailSignupResult = SignupResult.Accepted;
				SignupFunctions.SendKioskOptInForMarketingCampaignMessages(KioskOptInItemId.Yes, _confirmationStatus, _emailAddress);
				continueAction?.Invoke();
			},
			CancelAction = delegate
			{
				IdleTimer.Stop();
				_signupFlowResult.MarketingEmailSignupResult = SignupResult.Declined;
				SignupFunctions.SendKioskOptInForMarketingCampaignMessages(KioskOptInItemId.No, _confirmationStatus, _emailAddress);
				continueAction?.Invoke();
			}
		};
		_timeoutAction = delegate
		{
			IdleTimer.Stop();
			_signupFlowResult.MarketingEmailSignupResult = SignupResult.TimedOut;
			SignupFunctions.SendKioskOptInForMarketingCampaignMessages(KioskOptInItemId.Timeout, _confirmationStatus, _emailAddress);
			continueAction?.Invoke();
		};
		IdleTimer.Start();
		OrchestrationService.ViewShow("perks_signup_already_member_optin_view", parameter);
	}

	private void ShowSignUpForEarlyIdDialog(Action continueAction)
	{
		LogHelper.Instance.Log("Starting login with phone number.");
		WasEarlyIdSignupPresented = true;
		PhoneAndPinInfo parameter = new PhoneAndPinInfo
		{
			SignInCancelButtonType = SignInCancelButtonTypes.MaybeLater,
			MobilePhoneNumber = _mobilePhoneNumber,
			IsSignUp = true,
			SignUpFlowContext = _signUpFlowContext,
			ShowTextClubOption = !_textClubEnabled,
			IdleTimerName = "perks_signup_idle_timer",
			SignInAction = delegate(string phone, SecureString pin, bool isTextClubChecked)
			{
				IdleTimer.Stop();
				_signupFlowResult.PhoneAndPinSignupResult = SignupResult.Accepted;
				_mobilePhoneNumber = phone;
				_pin = pin;
				_optinForTextClub = isTextClubChecked;
				ProcessWPFHit();
				continueAction?.Invoke();
			},
			ContinueAction = delegate
			{
				IdleTimer.Stop();
				ProcessWPFHit();
				_signupFlowResult.PhoneAndPinSignupResult = SignupResult.Declined;
				_mobilePhoneNumber = null;
				continueAction?.Invoke();
			}
		};
		_timeoutAction = delegate
		{
			IdleTimer?.Stop();
			_signupFlowResult.PhoneAndPinSignupResult = SignupResult.TimedOut;
			_mobilePhoneNumber = null;
			continueAction?.Invoke();
		};
		IdleTimer.Start();
		OrchestrationService.ViewShow("phone_and_pin_view", parameter);
	}

	private void ShowUpdateAccountErrorMessage()
	{
		OrchestrationService.SetCurrentUICulture();
		MessagePopupViewController.ShowPerksCreateAccountError(TimeoutSignupFlow, ExitSignupFlow);
	}

	private void ShowUpdateAccountErrorOnPhoneAndPinView(IUpdateAccountResult result)
	{
		IViewService service = ServiceLocator.Instance.GetService<IViewService>();
		IViewFrameInstance viewFrameInstance = service?.PeekViewFrame();
		if (service != null && viewFrameInstance != null && viewFrameInstance?.ViewFrame.ViewName == "phone_and_pin_view")
		{
			LogHelper.Instance.Log("returning to phone and pin view to display error");
			PhoneAndPinInfo phoneAndPinInfo = viewFrameInstance.Parameter as PhoneAndPinInfo;
			if (phoneAndPinInfo?.PhoneAndPinViewModel != null)
			{
				phoneAndPinInfo.PhoneAndPinViewModel.MobilePhoneErrorMessageText = result.ErrorMessage;
				phoneAndPinInfo.PhoneAndPinViewModel.PinDisplayText = Resources.phone_and_pin_default_pin_display;
				phoneAndPinInfo.PhoneAndPinViewModel.PinConfirmDisplayText = Resources.phone_and_pin_default_pin_display;
			}
			IdleTimer?.Start();
			service.Show();
		}
		else
		{
			IdleTimer?.Stop();
			ExitSignupFlow();
		}
	}

	private void ShowJoinedTextClubView(Action continueAction)
	{
		LogHelper.Instance.Log("Promo to Joined Text Club: starting.");
		SimpleInfo parameter = new SimpleInfo
		{
			UseIdleTimer = false,
			AnalyticsAction = delegate
			{
				ServiceLocator.Instance.GetService<IAnalyticsService>()?.AddButtonPressEvent("TextMessageAlertOK");
			},
			ContinueAction = delegate
			{
				IdleTimer.Stop();
				ProcessWPFHit();
				_signupFlowResult.EarlyIdThanksResult = SignupResult.Accepted;
				continueAction?.Invoke();
			}
		};
		_timeoutAction = delegate
		{
			_signupFlowResult.EarlyIdThanksResult = SignupResult.TimedOut;
			continueAction?.Invoke();
		};
		IdleTimer.Start();
		OrchestrationService.ViewShow("join_text_club_promo_view", parameter);
	}

	private void ShowTimeoutDialog()
	{
		IdleTimer.Stop();
		IdleTimeoutMessageViewParameter parameter = new IdleTimeoutMessageViewParameter
		{
			IdleTimerName = "perks_signup_idle_timer",
			NoAction = delegate
			{
				OrchestrationService.ViewGoBackSafe("need_more_time:first_button", popOnly: false);
				_timeoutAction?.Invoke();
			},
			YesAction = delegate
			{
				ServiceLocator.Instance.GetService<IViewService>();
				OrchestrationService.ViewGoBackSafe("need_more_time:second_button", popOnly: false);
				IdleTimer.Start();
			}
		};
		OrchestrationService.ViewShow("idle_timeout_message_view", parameter);
	}

	private void QueueUpdateAccount(string mobilePhoneNumber, SecureString pin, bool textClubEnabled)
	{
		if (_signupFlowResult.AcceptedPerksSignup)
		{
			IPersonalizationService service = ServiceLocator.Instance.GetService<IPersonalizationService>();
			LogHelper.Instance.Log("Putting the call to UpdateAccount on the queue.");
			service.UpdateAccount(_customerProfileNumber, _emailAddress, mobilePhoneNumber, pin, _signupFlowResult.AcceptedPerksSignup, textClubEnabled, null, queued: true);
		}
	}

	private void ExitSignupFlow()
	{
		RemoveIdleTimer();
		LogHelper.Instance.Log($"Finished Sign up flow. context: {_signUpFlowContext}. {JsonConvert.SerializeObject(_signupFlowResult)}");
		_callbackAction(_signupFlowResult);
	}

	private void TimeoutSignupFlow()
	{
		RemoveIdleTimer();
		LogHelper.Instance.Log($"Finished Sign up flow with timeout.  context: {_signUpFlowContext}. {JsonConvert.SerializeObject(_signupFlowResult)}");
		_timeoutCallbackAction(_signupFlowResult);
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

	private static void ProcessWPFHit()
	{
		ServiceLocator.Instance.GetService<IRenderingService>()?.ActiveScene.PrcoessWPFHit();
		PostHealthActivity();
	}

	private static void PostHealthActivity()
	{
		ServiceLocator.Instance.GetService<ITouchScreenHealth>()?.PostActivity();
		ServiceLocator.Instance.GetService<IViewHealth>()?.PostActivity("Button Press");
	}

	private void ConfigureIdleTimer()
	{
		IdleTimer = ServiceLocator.Instance.GetService<IIdleTimerService>().CreateIdleTimer("perks_signup_idle_timer");
		int num = OrchestrationService.TTSGetTimeout("Medium", 60000);
		ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		num = ((service != null && service.TTSEnabled) ? 90000 : num);
		IdleTimer.Initialize(num, ShowTimeoutDialog);
	}

	private static void RemoveIdleTimer()
	{
		ServiceLocator.Instance.GetService<IIdleTimerService>().RemoveIdleTimer("perks_signup_idle_timer");
	}
}
