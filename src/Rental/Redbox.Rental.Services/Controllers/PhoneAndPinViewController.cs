using System;
using System.Windows;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.Personalization;
using Redbox.Rental.UI;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class PhoneAndPinViewController : BaseViewController
{
	public static void ConfigurePhoneAndPinView(PhoneAndPinInfo phoneAndPinInfo)
	{
		OrchestrationService.SetCurrentUICulture();
		if (!phoneAndPinInfo.IsSignUp)
		{
			BaseViewController.StartIdleTimer();
		}
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl == null)
		{
			return;
		}
		if (phoneAndPinInfo.PhoneAndPinViewModel != null)
		{
			phoneAndPinInfo.PhoneAndPinViewModel.IsStartButtonEnabled = true;
			viewUserControl.DataContext = phoneAndPinInfo.PhoneAndPinViewModel;
			return;
		}
		PhoneAndPinViewModel phoneAndPinViewModel = new PhoneAndPinViewModel();
		phoneAndPinViewModel.KeypadModel = new KeypadModel();
		phoneAndPinViewModel.KeypadModel.ClearAllButtonText = Resources.keypad_clear_all_button_text;
		phoneAndPinViewModel.KeypadModel.BackButtonText = Resources.keypad_back_button_text;
		KeypadModel keypadModel = phoneAndPinViewModel.KeypadModel;
		keypadModel.OnNumberButtonClicked = (Action<string>)Delegate.Combine(keypadModel.OnNumberButtonClicked, new Action<string>(KeypadModel_OnNumberButtonClicked));
		KeypadModel keypadModel2 = phoneAndPinViewModel.KeypadModel;
		keypadModel2.OnClearAllButtonClicked = (Action)Delegate.Combine(keypadModel2.OnClearAllButtonClicked, new Action(KeypadModel_OnClearAllButtonClicked));
		KeypadModel keypadModel3 = phoneAndPinViewModel.KeypadModel;
		keypadModel3.OnBackButtonClicked = (Action)Delegate.Combine(keypadModel3.OnBackButtonClicked, new Action(KeypadModel_OnBackButtonClicked));
		phoneAndPinViewModel.PhoneAndPinInfo = phoneAndPinInfo;
		phoneAndPinViewModel.PhoneRequiredLength = 10;
		phoneAndPinViewModel.PinRequiredLength = 4;
		phoneAndPinViewModel.SetDefaultDisplays();
		phoneAndPinViewModel.PhoneLabelText = Resources.phone_and_pin_phone_label_text;
		phoneAndPinViewModel.PinLabelText = Resources.phone_and_pin_pin_label_text;
		phoneAndPinViewModel.PinConfirmLabelText = Resources.phone_and_pin_confirm_pin_label_text;
		phoneAndPinViewModel.ErrorMessageText = "";
		phoneAndPinViewModel.MobilePhoneErrorMessageText = "";
		if (!string.IsNullOrEmpty(phoneAndPinInfo.MobilePhoneNumber))
		{
			phoneAndPinViewModel.PhoneDisplayText = phoneAndPinInfo.MobilePhoneNumber;
		}
		if (phoneAndPinInfo.IsSignUp)
		{
			phoneAndPinViewModel.HeaderLabelText = Resources.phone_and_pin_sign_up_header_text;
			phoneAndPinViewModel.SubHeadLabelText = ((phoneAndPinInfo.SignUpFlowContext == SignUpFlowContext.AfterSwipeAndRedboxPlusSignup) ? Resources.phone_and_pin_sign_up_redbox_plus_sub_header_text : Resources.phone_and_pin_sign_up_sub_header_text);
			phoneAndPinViewModel.StartButtonText = ((phoneAndPinInfo.SignInCancelButtonType == SignInCancelButtonTypes.Back) ? Resources.cancel_button.ToUpper() : Resources.phone_and_pin_sign_up_back_button_text.ToUpper());
			phoneAndPinViewModel.SignInButtonText = Resources.phone_and_pin_sign_up_button_text.ToUpper();
			phoneAndPinViewModel.TermsText = (phoneAndPinInfo.ShowTextClubOption ? Resources.phone_and_pin_terms_text : null);
			phoneAndPinViewModel.TermsButtonText = Resources.terms_link_text;
			phoneAndPinViewModel.TermsBarVisibility = Visibility.Visible;
			phoneAndPinViewModel.PinConfirmVisibility = Visibility.Visible;
			phoneAndPinViewModel.TextClubOptionVisibility = ((!phoneAndPinInfo.ShowTextClubOption) ? Visibility.Collapsed : Visibility.Visible);
			phoneAndPinViewModel.TextClubText = Resources.text_club_option_text;
			phoneAndPinViewModel.CheckMarkVisibility = ((!phoneAndPinInfo.ShowTextClubOption) ? Visibility.Hidden : Visibility.Visible);
			PhoneAndPinViewModel phoneAndPinViewModel2 = phoneAndPinViewModel;
			phoneAndPinViewModel2.OnStartButtonClicked = (Action)Delegate.Combine(phoneAndPinViewModel2.OnStartButtonClicked, new Action(OnMaybeLaterButtonClicked));
			PhoneAndPinViewModel phoneAndPinViewModel3 = phoneAndPinViewModel;
			phoneAndPinViewModel3.OnSignInButtonClicked = (Action)Delegate.Combine(phoneAndPinViewModel3.OnSignInButtonClicked, new Action(OnSignUpButtonClicked));
			phoneAndPinViewModel.CheckMarkCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), (Action)delegate
			{
				CheckMarkCommand_Execute(phoneAndPinViewModel);
			}, (Func<bool>)null);
			phoneAndPinViewModel.MobilePhoneErrorMessageText = string.Empty;
		}
		else
		{
			phoneAndPinViewModel.HeaderLabelText = Resources.phone_and_pin_header_text;
			phoneAndPinViewModel.SubHeadLabelText = Resources.phone_and_pin_sub_header_text;
			phoneAndPinViewModel.StartButtonText = ((phoneAndPinInfo.SignInCancelButtonType == SignInCancelButtonTypes.Back) ? Resources.cancel_button : Resources.phone_and_pin_start_button_text);
			phoneAndPinViewModel.SignInButtonText = Resources.phone_and_pin_sign_in_button_text;
			PhoneAndPinViewModel phoneAndPinViewModel4 = phoneAndPinViewModel;
			phoneAndPinViewModel4.OnStartButtonClicked = (Action)Delegate.Combine(phoneAndPinViewModel4.OnStartButtonClicked, new Action(PhoneAndPinViewModel_OnStartButtonClicked));
			PhoneAndPinViewModel phoneAndPinViewModel5 = phoneAndPinViewModel;
			phoneAndPinViewModel5.OnSignInButtonClicked = (Action)Delegate.Combine(phoneAndPinViewModel5.OnSignInButtonClicked, new Action(PhoneAndPinViewModel_OnSignInButtonClicked));
		}
		PhoneAndPinViewModel phoneAndPinViewModel6 = phoneAndPinViewModel;
		phoneAndPinViewModel6.OnDisplayClicked = (Action<PhoneAndPinViewModel.DisplayType>)Delegate.Combine(phoneAndPinViewModel6.OnDisplayClicked, new Action<PhoneAndPinViewModel.DisplayType>(PhoneAndPinViewModel_OnDisplayClicked));
		PhoneAndPinViewModel phoneAndPinViewModel7 = phoneAndPinViewModel;
		phoneAndPinViewModel7.OnTermsButtonClicked = (Action)Delegate.Combine(phoneAndPinViewModel7.OnTermsButtonClicked, (Action)delegate
		{
			PhoneAndPinViewModel_OnTermsButtonClicked(phoneAndPinInfo.IdleTimerName);
		});
		ConfigureTTS(phoneAndPinViewModel);
		phoneAndPinInfo.PhoneAndPinViewModel = phoneAndPinViewModel;
		viewUserControl.DataContext = phoneAndPinViewModel;
	}

	private static void CheckMarkCommand_Execute(PhoneAndPinViewModel phoneAndPinViewModel)
	{
		BaseViewController.ProcessWPFHit();
		if (phoneAndPinViewModel != null)
		{
			phoneAndPinViewModel.CheckMarkVisibility = ((phoneAndPinViewModel.CheckMarkVisibility == Visibility.Visible) ? Visibility.Hidden : Visibility.Visible);
			BaseViewController.AnalyticsService?.AddCheckBoxEvent("PhoneAndPinTextClubOptin", phoneAndPinViewModel.SignUpForTextClub);
		}
	}

	public static void LeavePhoneAndPinView(PhoneAndPinInfo phoneAndPinInfo)
	{
		BaseViewController.StopIdleTimer();
		phoneAndPinInfo.PhoneAndPinViewModel = GetPhoneAndPinViewModel();
	}

	private static PhoneAndPinViewModel GetPhoneAndPinViewModel()
	{
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			return viewUserControl.DataContext as PhoneAndPinViewModel;
		}
		return null;
	}

	private static void ConfigureTTS(PhoneAndPinViewModel phoneAndPinViewModel)
	{
		ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		if (service != null && service.TTSEnabled)
		{
			phoneAndPinViewModel.OnGetSpeechControl = () => PhoneAndPinViewTextToSpeech.GetSpeechControl(GetPhoneAndPinViewModel);
		}
	}

	private static void KeypadModel_OnNumberButtonClicked(string key)
	{
		BaseViewController.ProcessWPFHit();
		PhoneAndPinViewModel phoneAndPinViewModel = GetPhoneAndPinViewModel();
		if (phoneAndPinViewModel == null || key == null)
		{
			return;
		}
		phoneAndPinViewModel.ErrorMessageText = "";
		phoneAndPinViewModel.MobilePhoneErrorMessageText = "";
		if (phoneAndPinViewModel.DisplayTypeFocus == PhoneAndPinViewModel.DisplayType.Phone)
		{
			if (string.IsNullOrEmpty(phoneAndPinViewModel.PhoneDisplayText))
			{
				phoneAndPinViewModel.PhoneDisplayText = "";
				UpdateIsSignInButtonEnabled();
			}
			if (phoneAndPinViewModel.PhoneDisplayText.Length < phoneAndPinViewModel.PhoneRequiredLength)
			{
				phoneAndPinViewModel.PhoneDisplayText += key;
				UpdateIsSignInButtonEnabled();
			}
			if (phoneAndPinViewModel.PhoneDisplayText.Length == phoneAndPinViewModel.PhoneRequiredLength)
			{
				PhoneAndPinViewModel_OnDisplayClicked(PhoneAndPinViewModel.DisplayType.Pin);
			}
		}
		else if (phoneAndPinViewModel.DisplayTypeFocus == PhoneAndPinViewModel.DisplayType.Pin)
		{
			if (phoneAndPinViewModel.PinDisplayText == Resources.phone_and_pin_default_pin_display)
			{
				phoneAndPinViewModel.PinDisplayText = "";
				UpdateIsSignInButtonEnabled();
			}
			if (phoneAndPinViewModel.PinDisplayText.Length < phoneAndPinViewModel.PinRequiredLength)
			{
				phoneAndPinViewModel.PinDisplayText += key;
				UpdateIsSignInButtonEnabled();
			}
		}
		else if (phoneAndPinViewModel.DisplayTypeFocus == PhoneAndPinViewModel.DisplayType.PinConfirm)
		{
			if (phoneAndPinViewModel.PinConfirmDisplayText == Resources.phone_and_pin_default_pin_display)
			{
				phoneAndPinViewModel.PinConfirmDisplayText = "";
				UpdateIsSignInButtonEnabled();
			}
			if (phoneAndPinViewModel.PinConfirmDisplayText.Length < phoneAndPinViewModel.PinRequiredLength)
			{
				phoneAndPinViewModel.PinConfirmDisplayText += key;
				UpdateIsSignInButtonEnabled();
			}
		}
	}

	private static void KeypadModel_OnClearAllButtonClicked()
	{
		BaseViewController.ProcessWPFHit();
		PhoneAndPinViewModel phoneAndPinViewModel = GetPhoneAndPinViewModel();
		if (phoneAndPinViewModel != null)
		{
			phoneAndPinViewModel.ErrorMessageText = "";
			phoneAndPinViewModel.MobilePhoneErrorMessageText = "";
			if (phoneAndPinViewModel.DisplayTypeFocus == PhoneAndPinViewModel.DisplayType.Phone)
			{
				phoneAndPinViewModel.PhoneDisplayText = string.Empty;
				UpdateIsSignInButtonEnabled();
			}
			else if (phoneAndPinViewModel.DisplayTypeFocus == PhoneAndPinViewModel.DisplayType.Pin)
			{
				phoneAndPinViewModel.PinDisplayText = Resources.phone_and_pin_default_pin_display;
				UpdateIsSignInButtonEnabled();
			}
			else if (phoneAndPinViewModel.DisplayTypeFocus == PhoneAndPinViewModel.DisplayType.PinConfirm)
			{
				phoneAndPinViewModel.PinConfirmDisplayText = Resources.phone_and_pin_default_pin_display;
				UpdateIsSignInButtonEnabled();
			}
		}
	}

	private static void KeypadModel_OnBackButtonClicked()
	{
		BaseViewController.ProcessWPFHit();
		PhoneAndPinViewModel phoneAndPinViewModel = GetPhoneAndPinViewModel();
		if (phoneAndPinViewModel == null)
		{
			return;
		}
		phoneAndPinViewModel.ErrorMessageText = "";
		phoneAndPinViewModel.MobilePhoneErrorMessageText = "";
		if (phoneAndPinViewModel.DisplayTypeFocus == PhoneAndPinViewModel.DisplayType.Phone)
		{
			if (phoneAndPinViewModel.PhoneDisplayText.Length > 0)
			{
				phoneAndPinViewModel.PhoneDisplayText = phoneAndPinViewModel.PhoneDisplayText.Remove(phoneAndPinViewModel.PhoneDisplayText.Length - 1);
				UpdateIsSignInButtonEnabled();
			}
			if (phoneAndPinViewModel.PhoneDisplayText == "")
			{
				UpdateIsSignInButtonEnabled();
			}
		}
		else if (phoneAndPinViewModel.DisplayTypeFocus == PhoneAndPinViewModel.DisplayType.Pin)
		{
			if (phoneAndPinViewModel.PinDisplayText != Resources.phone_and_pin_default_pin_display && phoneAndPinViewModel.PinDisplayText.Length > 0)
			{
				phoneAndPinViewModel.PinDisplayText = phoneAndPinViewModel.PinDisplayText.Remove(phoneAndPinViewModel.PinDisplayText.Length - 1);
				UpdateIsSignInButtonEnabled();
			}
			if (phoneAndPinViewModel.PinDisplayText == "")
			{
				phoneAndPinViewModel.PinDisplayText = Resources.phone_and_pin_default_pin_display;
				UpdateIsSignInButtonEnabled();
			}
		}
		else if (phoneAndPinViewModel.DisplayTypeFocus == PhoneAndPinViewModel.DisplayType.PinConfirm)
		{
			if (phoneAndPinViewModel.PinConfirmDisplayText != Resources.phone_and_pin_default_pin_display && phoneAndPinViewModel.PinConfirmDisplayText.Length > 0)
			{
				phoneAndPinViewModel.PinConfirmDisplayText = phoneAndPinViewModel.PinConfirmDisplayText.Remove(phoneAndPinViewModel.PinConfirmDisplayText.Length - 1);
				UpdateIsSignInButtonEnabled();
			}
			if (phoneAndPinViewModel.PinConfirmDisplayText == "")
			{
				phoneAndPinViewModel.PinConfirmDisplayText = Resources.phone_and_pin_default_pin_display;
				UpdateIsSignInButtonEnabled();
			}
		}
	}

	private static void UpdateIsSignInButtonEnabled()
	{
		PhoneAndPinViewModel phoneAndPinViewModel = GetPhoneAndPinViewModel();
		if (phoneAndPinViewModel != null)
		{
			phoneAndPinViewModel.IsSignInButtonEnabled = phoneAndPinViewModel.PhoneDisplayText.Length == phoneAndPinViewModel.PhoneRequiredLength && phoneAndPinViewModel.PinDisplayText != Resources.phone_and_pin_default_pin_display && phoneAndPinViewModel.PinDisplayText.Length == phoneAndPinViewModel.PinRequiredLength;
			if (phoneAndPinViewModel.PhoneAndPinInfo.IsSignUp)
			{
				UpdatePinConfirmView(phoneAndPinViewModel);
			}
		}
	}

	private static void UpdatePinConfirmView(PhoneAndPinViewModel phoneAndPinViewModel)
	{
		bool flag = phoneAndPinViewModel.PinConfirmDisplayText == phoneAndPinViewModel.PinDisplayText;
		phoneAndPinViewModel.IsSignInButtonEnabled &= flag;
		if (!flag)
		{
			phoneAndPinViewModel.ConfirmPinButtonState = 1;
			phoneAndPinViewModel.ErrorMessageText = Resources.phone_and_pin_confirm_error_text;
		}
		else
		{
			phoneAndPinViewModel.ConfirmPinButtonState = 0;
			phoneAndPinViewModel.ErrorMessageText = string.Empty;
		}
	}

	private static void PhoneAndPinViewModel_OnDisplayClicked(PhoneAndPinViewModel.DisplayType displayType)
	{
		BaseViewController.ProcessWPFHit();
		PhoneAndPinViewModel phoneAndPinViewModel = GetPhoneAndPinViewModel();
		if (phoneAndPinViewModel != null)
		{
			phoneAndPinViewModel.ErrorMessageText = "";
			phoneAndPinViewModel.MobilePhoneErrorMessageText = "";
			phoneAndPinViewModel.ObfuscatePins();
			phoneAndPinViewModel.DisplayTypeFocus = displayType;
			switch (displayType)
			{
			case PhoneAndPinViewModel.DisplayType.Phone:
				UpdatePinConfirmView(phoneAndPinViewModel);
				break;
			case PhoneAndPinViewModel.DisplayType.Pin:
				UpdatePinConfirmView(phoneAndPinViewModel);
				break;
			case PhoneAndPinViewModel.DisplayType.PinConfirm:
				UpdatePinConfirmView(phoneAndPinViewModel);
				break;
			}
		}
	}

	private static void PhoneAndPinViewModel_OnStartButtonClicked()
	{
		BaseViewController.AnalyticsService?.AddButtonPressEvent("PhoneAndPinStartBrowsing");
		PhoneAndPinViewModel phoneAndPinViewModel = GetPhoneAndPinViewModel();
		OrchestrationService.ViewGoBackSafe("Phone and pin login cancel", popOnly: false);
		if (phoneAndPinViewModel != null && phoneAndPinViewModel.PhoneAndPinInfo != null && phoneAndPinViewModel.PhoneAndPinInfo.ContinueAction != null)
		{
			phoneAndPinViewModel.PhoneAndPinInfo.ContinueAction();
		}
	}

	private static void PhoneAndPinViewModel_OnSignInButtonClicked()
	{
		BaseViewController.AnalyticsService?.AddButtonPressEvent("PhoneAndPinSignIn");
		PhoneAndPinViewModel phoneAndPinViewModel = GetPhoneAndPinViewModel();
		if (phoneAndPinViewModel != null && phoneAndPinViewModel.PhoneAndPinInfo != null && phoneAndPinViewModel.PhoneAndPinInfo.SignInAction != null)
		{
			phoneAndPinViewModel.PhoneAndPinInfo.SignInAction(phoneAndPinViewModel.PhoneDisplayText, BaseViewController.StringToSecureString(phoneAndPinViewModel.PinDisplayText), arg3: false);
		}
	}

	private static void PhoneAndPinViewModel_OnTermsButtonClicked(string idleTimerName)
	{
		BaseViewController.AnalyticsService?.AddButtonPressEvent("PhoneAndPinTerms");
		HelpViewController.ShowMobileFaqAndPrivacy("perks_terms", idleTimerName);
	}

	private static void OnMaybeLaterButtonClicked()
	{
		BaseViewController.AnalyticsService?.AddButtonPressEvent("PhoneAndPinMaybeLater");
		PhoneAndPinViewModel phoneAndPinViewModel = GetPhoneAndPinViewModel();
		if (phoneAndPinViewModel != null && phoneAndPinViewModel.PhoneAndPinInfo != null && phoneAndPinViewModel.PhoneAndPinInfo.ContinueAction != null)
		{
			phoneAndPinViewModel.PhoneAndPinInfo.ContinueAction();
		}
	}

	private static void OnSignUpButtonClicked()
	{
		BaseViewController.AnalyticsService?.AddButtonPressEvent("PhoneAndPinSignUp");
		PhoneAndPinViewModel phoneAndPinViewModel = GetPhoneAndPinViewModel();
		if (phoneAndPinViewModel != null && phoneAndPinViewModel.PhoneAndPinInfo != null && phoneAndPinViewModel.PhoneAndPinInfo.SignInAction != null)
		{
			phoneAndPinViewModel.PhoneAndPinInfo.SignInAction(phoneAndPinViewModel.PhoneDisplayText, BaseViewController.StringToSecureString(phoneAndPinViewModel.PinDisplayText), phoneAndPinViewModel.SignUpForTextClub);
		}
	}
}
