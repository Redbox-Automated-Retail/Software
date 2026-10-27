using System;
using System.Security;
using System.Windows;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.UI.ControllersLogic;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class KeyboardViewController : BaseViewController
{
	public static void ConfigureView(object parameter)
	{
		KeyboardInfo keyboardInfo = parameter as KeyboardInfo;
		KeyboardControl keyboardControl = (KeyboardControl)BaseViewController.GetViewUserControl();
		if (keyboardControl == null)
		{
			return;
		}
		BaseViewController.StopIdleTimer();
		BaseViewController.SetCurrentUICulture();
		keyboardInfo.KeyboardMode = keyboardControl.KeyboardMode;
		KeyboardModel keyboardModel;
		KeyboardLogic keyboardLogic;
		if (keyboardInfo.keyboardModel != null && keyboardInfo.keyboardLogic != null)
		{
			keyboardModel = keyboardInfo.keyboardModel;
			keyboardLogic = keyboardInfo.keyboardLogic;
		}
		else
		{
			keyboardModel = new KeyboardModel
			{
				KeyboardText = keyboardInfo.KeyboardText
			};
			keyboardModel.KeyEmailMode = true;
			keyboardModel.KeyboardCapLock = false;
			keyboardModel.BackButtonText = Resources.keyboard_back_button_text;
			keyboardModel.SpaceButtonText = Resources.keyboard_space_button_text;
			keyboardModel.ClearButtonText = Resources.keyboard_clear_button_text;
			keyboardModel.SymbolsButtonText = Resources.keyboard_symbols_btn_text;
			KeyboardLogic.StopIdleTimerAction = delegate
			{
				BaseViewController.StopIdleTimer();
			};
			keyboardLogic = new KeyboardLogic(keyboardModel)
			{
				Instructions = true,
				ForgotPasswordAction = ProcessForgotPasswordAction,
				CancelAction = ProcessCancelAction,
				ContinueAction = ProcessContinueAction,
				ContinueCodeAction = ProcessEmailCodeAction
			};
			keyboardLogic.KeyboardInfo = keyboardInfo;
			keyboardLogic.KeyboardMode = keyboardInfo.KeyboardMode;
			keyboardLogic.IsValidEntry = keyboardInfo.IsValidEntry;
			keyboardLogic.IsValidEntries = keyboardInfo.IsValidEntries;
			keyboardLogic.KeyboardTextMaxLength = keyboardInfo.MaxLength;
			keyboardModel.Logic = keyboardLogic;
		}
		if (!string.IsNullOrEmpty(keyboardInfo.ErrorMessage))
		{
			keyboardModel.KeyboardError = keyboardInfo.ErrorMessage;
			if (!keyboardInfo.KeepKeyboardTextOnInitialErrorMessage)
			{
				keyboardModel.KeyboardText = string.Empty;
				keyboardModel.KeyboardCode = string.Empty;
			}
		}
		keyboardControl.DataContext = keyboardModel;
		switch (keyboardInfo.KeyboardMode)
		{
		case KeyboardMode.ACCOUNT_SIGN_IN:
			InitForAccountSignIn(keyboardLogic);
			break;
		case KeyboardMode.ACCOUNT_EMAIL_CHANGED:
			InitForAccountEmailChanged(keyboardLogic);
			break;
		case KeyboardMode.PROMOTIONS_CODE:
			InitForPromotionsCode(keyboardLogic);
			break;
		case KeyboardMode.EMAIL_RECEIPTS_PROMO:
			InitForReceiptPromoEmail(keyboardLogic);
			break;
		case KeyboardMode.EMAIL_COMING_SOON:
			InitForCommingSoonEmail(keyboardLogic);
			break;
		case KeyboardMode.PERKS_SIGN_UP:
			InitForPerksSignup(keyboardLogic);
			break;
		case KeyboardMode.REDBOX_PLUS_SIGN_UP:
			InitForRedboxPlusSignup(keyboardLogic);
			break;
		case KeyboardMode.LEAD_GENERATION_SIGN_UP:
			InitForLeadGenerationSignup(keyboardLogic);
			break;
		}
		ConfigureTTS(keyboardModel);
	}

	public static void ClosingView(object parameter)
	{
		OrchestrationService.RemoveIdleTimer("keyboard_idle_timer");
		KeyboardModel keyboardModel = GetkeyboardModel();
		if (keyboardModel != null && parameter is KeyboardInfo keyboardInfo)
		{
			keyboardInfo.keyboardLogic = keyboardModel.Logic;
			keyboardInfo.keyboardModel = keyboardModel;
		}
	}

	private static void ProcessForgotPasswordAction()
	{
		GetKeyboardLogic()?.KeyboardInfo?.ForgotPasswordAction();
		BaseViewController.ProcessWPFHit();
	}

	private static void ProcessCancelAction()
	{
		KeyboardLogic keyboardLogic = GetKeyboardLogic();
		if (keyboardLogic != null && keyboardLogic.KeyboardInfo?.CancelAction != null)
		{
			keyboardLogic.KeyboardInfo.CancelAction();
		}
		BaseViewController.ProcessWPFHit();
	}

	private static void ProcessContinueAction(string keyboardText)
	{
		KeyboardLogic keyboardLogic = GetKeyboardLogic();
		if ((keyboardLogic?.KeyboardInfo)?.ContinueAction != null)
		{
			keyboardLogic.KeyboardInfo.ContinueAction(keyboardText);
		}
		BaseViewController.ProcessWPFHit();
	}

	private static void ProcessEmailCodeAction(string keyboardText, SecureString keyboardCode)
	{
		KeyboardLogic keyboardLogic = GetKeyboardLogic();
		_ = keyboardLogic?.KeyboardInfo;
		keyboardLogic?.KeyboardInfo?.ContinueCodeAction(keyboardText, keyboardCode);
	}

	private static KeyboardModel GetkeyboardModel()
	{
		return BaseViewController.GetViewModel<KeyboardModel>();
	}

	private static KeyboardLogic GetKeyboardLogic()
	{
		return GetkeyboardModel()?.Logic;
	}

	private static void InitForAccountSignIn(KeyboardLogic keyboardLogic)
	{
		KeyboardModel model = keyboardLogic.Model;
		model.TitleText = Resources.keyboard_sign_in_tag;
		model.MessageText = Resources.keyboard_sign_in_message;
		model.EmailPromptText = Resources.keyboard_input_email_prompt;
		model.CodePromptText = Resources.keyboard_sign_in_password_prompt;
		model.ForgotPasswordButtonText = Resources.keyboard_forgot_password_button_text.ToUpper();
		model.EmailButtonText = Resources.keyboard_email_button_text.ToUpper();
		model.PasswordButtonText = Resources.keyboard_password_button_text.ToUpper();
		model.CancelButtonText = Resources.keyboard_cancel_button_text.ToUpper();
		model.ContinueButtonText = Resources.keyboard_continue_button_text.ToUpper();
		if (BaseViewController.GetViewUserControl() is KeyboardSignInViewControl keyboardSignInViewControl)
		{
			keyboardSignInViewControl.UpdateKeyStates();
		}
		BaseViewController.StartIdleTimer(reset: true);
	}

	private static void InitForAccountEmailChanged(KeyboardLogic keyboardLogic)
	{
		KeyboardModel model = keyboardLogic.Model;
		model.MessageText = Resources.keyboard_change_message;
		model.MarketingText = Resources.keyboard_change_marketing;
		model.CancelButtonText = Resources.keyboard_change_cancel_btn.ToUpper();
		model.ContinueButtonText = Resources.keyboard_change_continue_btn.ToUpper();
		model.EmailPromptText = Resources.keyboard_input_email_prompt;
		BaseViewController.StartIdleTimer(reset: true);
	}

	private static void InitForReceiptPromoEmail(KeyboardLogic keyboardLogic)
	{
		KeyboardModel model = keyboardLogic.Model;
		model.TitleText = Resources.email_tag;
		model.MessageText = Resources.email_prompt_message;
		model.MarketingText = null;
		model.CancelButtonText = Resources.keyboard_receipt_cancel_btn.ToUpper();
		model.ContinueButtonText = Resources.keyboard_receipt_continue_btn.ToUpper();
		model.EmailPromptText = Resources.keyboard_input_email_prompt;
		StartKeyboardIdleTimer(TimeOutAction);
	}

	private static void InitForCommingSoonEmail(KeyboardLogic keyboardLogic)
	{
		KeyboardModel model = keyboardLogic.Model;
		model.MessageTextSize = 18;
		model.MessageText = Resources.coming_soon_email_optin_email_prompt_message;
		model.MarketingText = Resources.keyboard_change_marketing;
		model.CancelButtonText = Resources.coming_soon_cancel_btn.ToUpper();
		model.ContinueButtonText = Resources.coming_soon_continue_btn.ToUpper();
		model.EmailPromptText = Resources.keyboard_input_email_prompt;
		BaseViewController.StartIdleTimer(reset: true);
	}

	private static void InitForPromotionsCode(KeyboardLogic keyboardLogic)
	{
		KeyboardModel model = keyboardLogic.Model;
		OrchestrationService.SetMacro("TTSKeyboard:PromoEntry", "true");
		model.TitleText = Resources.promo_code_tag;
		model.MessageText = Resources.promo_code_prompt_message;
		model.MarketingText = "";
		model.CancelButtonText = Resources.promo_code_cancel_btn.ToUpper();
		model.ContinueButtonText = Resources.promo_code_continue_btn.ToUpper();
		model.EmailPromptText = Resources.promo_code_input_prompt;
		BaseViewController.StartIdleTimer(reset: true);
	}

	private static void InitForPerksSignup(KeyboardLogic keyboardLogic)
	{
		KeyboardModel model = keyboardLogic.Model;
		model.TitleText = Resources.perks_sign_up_title;
		model.MessageText = Resources.email_privacy_message;
		model.CancelButtonText = Resources.back_button.ToUpper();
		model.ContinueButtonText = Resources.go_to_final_step_button;
		model.EmailPromptText = Resources.keyboard_input_email_prompt;
	}

	private static void InitForRedboxPlusSignup(KeyboardLogic keyboardLogic)
	{
		KeyboardModel model = keyboardLogic.Model;
		model.TitleText = Resources.redbox_plus_sign_up_title;
		model.MessageText = Resources.email_privacy_message;
		model.CancelButtonText = Resources.back_button.ToUpper();
		model.ContinueButtonText = Resources.go_to_final_step_button;
		model.EmailPromptText = Resources.keyboard_input_email_prompt;
	}

	private static void InitForLeadGenerationSignup(KeyboardLogic keyboardLogic)
	{
		KeyboardModel model = keyboardLogic.Model;
		model.TitleText = Resources.lead_generation_signup_title_text;
		model.EmailPromptText = Resources.keyboard_input_email_prompt;
		model.CancelButtonText = Resources.no_thanks_button.ToUpper();
		model.ContinueButtonText = Resources.finish_button.ToUpper();
		StartKeyboardIdleTimer(TimeOutAction2);
	}

	private static void StartKeyboardIdleTimer(Action timeOutAction)
	{
		int num = 15000;
		num = (OrchestrationService.TTSEnabled ? OrchestrationService.TTSGetTimeout("medium", num) : num);
		OrchestrationService.StartIdleTimer(num, timeOutAction, "keyboard_idle_timer");
	}

	private static void TimeOutAction()
	{
		OrchestrationService.RemoveIdleTimer("keyboard_idle_timer");
		KeyboardLogic keyboardLogic = GetKeyboardLogic();
		KeyboardModel model = keyboardLogic.Model;
		if (keyboardLogic == null || model == null)
		{
			return;
		}
		if (keyboardLogic.IsValidEntry != null && keyboardLogic.IsValidEntry(model.KeyboardText))
		{
			IShoppingSessionService service = ServiceLocator.Instance.GetService<IShoppingSessionService>();
			if (service != null)
			{
				IShoppingSession currentSession = service.GetCurrentSession();
				if (currentSession != null && currentSession.ShoppingCart != null)
				{
					currentSession.ShoppingCart.SetEmail(model.KeyboardText);
				}
				else
				{
					LogHelper.Instance.Log("Problem: currentSession or currentSession.ShoppingCart == null");
				}
			}
			else
			{
				LogHelper.Instance.Log("Problem: shoppingSessionService == null");
			}
		}
		if (keyboardLogic != null && keyboardLogic.CancelAction != null)
		{
			keyboardLogic.CancelAction();
		}
		else
		{
			BaseViewController.GetViewService()?.UnwindToSafe(BaseViewController.GetViewService().GetStartOverViewName(), discard: true, "TimeOutAction from idle_timer (KeyboardInfo.CancelAction = null)");
		}
	}

	private static void TimeOutAction2()
	{
		OrchestrationService.RemoveIdleTimer("keyboard_idle_timer");
		GetKeyboardLogic()?.CancelAction?.Invoke();
	}

	private static void ConfigureTTS(KeyboardModel keyboardModel)
	{
		ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (service != null && service.TTSEnabled && viewUserControl != null)
		{
			keyboardModel.ProcessGetSpeechControl = () => BaseTTSController<KeyboardTTSController, KeyboardLogic>.GetSpeechControl(GetKeyboardLogic);
		}
	}
}
