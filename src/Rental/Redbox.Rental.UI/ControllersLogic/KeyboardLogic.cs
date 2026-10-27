using System;
using System.Security;
using System.Windows.Input;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.UI.ControllersLogic;

public class KeyboardLogic : BaseLogic
{
	public KeyboardModel Model { get; set; }

	public KeyboardInfo KeyboardInfo { get; set; }

	public bool Instructions { get; set; }

	public int KeyboardTextMaxLength { get; set; }

	public Action ForgotPasswordAction { get; set; }

	public Action CancelAction { get; set; }

	public Action<string> ContinueAction { get; set; }

	public Action<string, SecureString> ContinueCodeAction { get; set; }

	public Predicate<string> IsValidEntry { get; set; }

	public Func<string, string, bool> IsValidEntries { get; set; }

	public static Action StopIdleTimerAction { get; set; }

	public KeyboardMode KeyboardMode { get; set; }

	public KeyboardLogic(KeyboardModel model)
	{
		Model = model;
		Model.KeyboardError = "";
		Model.KeyButtonCommand = DynamicRouting.RegisterRoutedCommand(Commands.KeyButtonCommand, KeyButtonCommand_Execute);
		Model.BackButtonCommand = DynamicRouting.RegisterRoutedCommand(Commands.BackButtonCommand, BackButtonCommand_Execute);
		Model.ClearButtonCommand = DynamicRouting.RegisterRoutedCommand(Commands.ClearButtonCommand, ClearButtonCommand_Execute);
		Model.DomainButtonCommand = DynamicRouting.RegisterRoutedCommand(Commands.DomainButtonCommand, DomainButtonCommand_Execute);
		Model.DotComButtonCommand = DynamicRouting.RegisterRoutedCommand(Commands.DotComButtonCommand, DotComButtonCommand_Execute);
		Model.SpaceButtonCommand = DynamicRouting.RegisterRoutedCommand(Commands.SpaceButtonCommand, SpaceButtonCommand_Execute);
		Model.ForgotPasswordButtonCommand = DynamicRouting.RegisterRoutedCommand(Commands.ForgotPasswordButtonCommand, ForgotPasswordButtonCommand_Execute);
		Model.CancelButtonCommand = DynamicRouting.RegisterRoutedCommand(Commands.CancelButtonCommand, CancelButtonCommand_Execute);
		Model.ContinueButtonCommand = DynamicRouting.RegisterRoutedCommand(Commands.ContinueButtonCommand, ContinueButtonCommand_Execute, ContinueButtonCommand_canExecute);
		Model.EmailSelectedCommand = DynamicRouting.RegisterRoutedCommand(Commands.EmailSelectedCommand, EmailSelectedCommand_Execute);
	}

	public bool InitControlTextMode()
	{
		bool keyEmailMode = Model.KeyEmailMode;
		Model.SetControlTextMode?.Invoke(keyEmailMode);
		return keyEmailMode;
	}

	public void SetControlTextMode(bool mode)
	{
		Model.SetControlTextMode?.Invoke(mode);
	}

	public bool GetControlTextMode()
	{
		return Model.KeyEmailMode;
	}

	private string GetErrorMessage()
	{
		BaseLogic.SetCurrentUICulture();
		switch (KeyboardInfo.KeyboardMode)
		{
		case KeyboardMode.EMAIL_COMING_SOON:
		case KeyboardMode.ACCOUNT_SIGN_IN:
		case KeyboardMode.PERKS_SIGN_UP:
		case KeyboardMode.REDBOX_PLUS_SIGN_UP:
		case KeyboardMode.LEAD_GENERATION_SIGN_UP:
			return Resources.email_error_message;
		case KeyboardMode.EMAIL_RECEIPTS_PROMO:
			return Resources.receipt_email_error;
		case KeyboardMode.PROMOTIONS_CODE:
			return Resources.promo_code_error_message;
		default:
			return "";
		}
	}

	public void CancelButton_Execute()
	{
		if (CancelAction != null)
		{
			CancelAction();
			Model.KeyboardText = "";
		}
	}

	public bool ValidateKeyboardText()
	{
		Model.KeyboardText = Model.KeyboardText ?? "";
		Model.KeyboardCode = Model.KeyboardCode ?? "";
		Model.KeyboardError = "";
		bool flag;
		switch (KeyboardMode)
		{
		case KeyboardMode.ACCOUNT_SIGN_IN:
			flag = IsValidEntries != null && IsValidEntries(Model.KeyboardText, Model.KeyboardCode);
			if (!flag)
			{
				Model.KeyboardError = GetErrorMessage();
			}
			break;
		case KeyboardMode.ACCOUNT_EMAIL_CHANGED:
			flag = IsValidEntries != null && IsValidEntries(Model.KeyboardText, Model.KeyboardCode);
			if (!flag)
			{
				Model.KeyboardError = GetErrorMessage();
			}
			break;
		default:
			flag = IsValidEntry != null && IsValidEntry(Model.KeyboardText);
			if (!flag)
			{
				Model.KeyboardError = GetErrorMessage();
			}
			break;
		}
		return flag;
	}

	public string ContinueButtonExecute()
	{
		if (ValidateKeyboardText())
		{
			if (KeyboardMode == KeyboardMode.ACCOUNT_SIGN_IN)
			{
				if (ContinueCodeAction != null)
				{
					StopIdleTimerAction?.Invoke();
					ContinueCodeAction(Model.KeyboardText, BaseLogic.CreateSecureString(Model.KeyboardCode));
					Model.KeyboardCode = string.Empty;
					Model.KeyEmailMode = false;
				}
			}
			else if (ContinueAction != null)
			{
				StopIdleTimerAction?.Invoke();
				ContinueAction(Model.KeyboardText);
			}
		}
		return Model.KeyboardError;
	}

	private void UpdateContinue(bool clearError = true)
	{
		if (Model.KeyEmailMode)
		{
			if ((Model.KeyboardText?.Length ?? 0) > KeyboardTextMaxLength)
			{
				Model.KeyboardText = Model.KeyboardText.Remove(KeyboardTextMaxLength);
			}
		}
		else if ((Model.KeyboardCode?.Length ?? 0) > KeyboardTextMaxLength)
		{
			Model.KeyboardCode = Model.KeyboardCode.Remove(KeyboardTextMaxLength);
		}
		if (clearError)
		{
			Model.KeyboardError = string.Empty;
		}
		BaseLogic.HandleWPFHit();
		CommandManager.InvalidateRequerySuggested();
		Model.ContinueButtonEnabled = Model.ContinueButtonCommand.CanExecute(null);
	}

	public void KeyButtonCommand_Execute(string text)
	{
		if (Model.KeyEmailMode)
		{
			Model.KeyboardText += text;
		}
		else
		{
			Model.KeyboardCode += text;
		}
		UpdateContinue();
	}

	public void BackButtonCommand_Execute()
	{
		if (Model.KeyboardText == null)
		{
			return;
		}
		if (Model.KeyEmailMode)
		{
			int num = Model.KeyboardText.Length - 1;
			if (num > -1)
			{
				Model.KeyboardText = Model.KeyboardText.Remove(num);
			}
		}
		else
		{
			int num = Model.KeyboardCode.Length - 1;
			if (num > -1)
			{
				Model.KeyboardCode = Model.KeyboardCode.Remove(num);
			}
		}
		UpdateContinue();
	}

	public void ClearButtonCommand_Execute()
	{
		if (Model.KeyEmailMode)
		{
			Model.KeyboardText = string.Empty;
		}
		else
		{
			Model.KeyboardCode = string.Empty;
		}
		UpdateContinue();
	}

	public void DomainButtonCommand_Execute(string text)
	{
		if (Model.KeyboardText != null)
		{
			int num = Model.KeyboardText.IndexOf('@');
			if (num > -1)
			{
				Model.KeyboardText = Model.KeyboardText.Remove(num);
			}
			if (Model.KeyboardText.Length > 0)
			{
				Model.KeyboardText += text;
				SetControlTextMode(mode: false);
			}
			UpdateContinue();
		}
	}

	public void DotComButtonCommand_Execute(string text)
	{
		if (Model.KeyboardText == null)
		{
			return;
		}
		BaseLogic.SetCurrentUICulture();
		int num = Model.KeyboardText.IndexOf('@');
		if (num == -1)
		{
			Model.KeyboardError = Resources.email_error_message;
		}
		string text2 = Model.KeyboardText.Substring(num + 1);
		if (text2.Length == 0)
		{
			Model.KeyboardError = Resources.email_error_message;
		}
		else
		{
			int num2 = text2.IndexOf('.') + 1;
			if (num2 > 0)
			{
				Model.KeyboardText = Model.KeyboardText.Remove(num + num2);
			}
			Model.KeyboardText += text;
			SetControlTextMode(mode: false);
		}
		UpdateContinue();
	}

	public void SpaceButtonCommand_Execute()
	{
		if (!Model.KeyEmailMode)
		{
			Model.KeyboardCode += " ";
		}
		UpdateContinue();
	}

	public void ForgotPasswordButtonCommand_Execute()
	{
		BaseLogic.AnalyticsService?.AddButtonPressEvent("KeyboardLogicSignInForgotPassword");
		ForgotPasswordAction?.Invoke();
		BaseLogic.HandleWPFHit();
	}

	public void CancelButtonCommand_Execute()
	{
		BaseLogic.AnalyticsService?.AddButtonPressEvent("KeyboardLogicStartBrowsing");
		CancelButton_Execute();
		BaseLogic.HandleWPFHit();
	}

	public bool ContinueButtonCommand_canExecute()
	{
		if (KeyboardMode != KeyboardMode.ACCOUNT_SIGN_IN)
		{
			return true;
		}
		if (!string.IsNullOrWhiteSpace(Model.KeyboardText))
		{
			return !string.IsNullOrEmpty(Model.KeyboardCode);
		}
		return false;
	}

	public void ContinueButtonCommand_Execute()
	{
		BaseLogic.AnalyticsService?.AddButtonPressEvent("KeyboardLogicStartBrowsing");
		Model.KeyboardError = ContinueButtonExecute();
		if (!string.IsNullOrEmpty(Model.KeyboardError))
		{
			UpdateContinue(clearError: false);
		}
		else if (KeyboardMode != KeyboardMode.ACCOUNT_SIGN_IN && KeyboardMode != KeyboardMode.PROMOTIONS_CODE)
		{
			Model.KeyboardText = "";
		}
	}

	public void EmailSelectedCommand_Execute(bool emailMode)
	{
		Model.KeyEmailMode = emailMode;
	}
}
