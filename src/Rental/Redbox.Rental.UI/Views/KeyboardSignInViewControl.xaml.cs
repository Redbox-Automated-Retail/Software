using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Redbox.Controls;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.UI.Controls;

namespace Redbox.Rental.UI.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "KeyboardSignInView")]
public partial class KeyboardSignInViewControl : KeyboardControl, IComponentConnector
{
	public KeyboardSignInViewControl()
	{
		base.KeyboardMode = KeyboardMode.ACCOUNT_SIGN_IN;
		InitializeComponent();
		InitKeyboardUserControl(KeyboardElem, EmailElem);
		ApplyTheme();
	}

	protected override void DataContextHasChanged()
	{
		Visibility visibility = ((!base.IsABEOn) ? Visibility.Collapsed : Visibility.Visible);
		EmailButtonElem.Visibility = visibility;
		CodeButtonElem.Visibility = visibility;
		if (base.Model != null)
		{
			base.Model.SetControlTextMode = SetControlTextMode;
			SetControlTextMode(base.Model.KeyEmailMode);
		}
		UpdateKeyboardControl();
	}

	private void ApplyTheme()
	{
		ITheme theme = ServiceLocator.Instance.GetService<IThemeService>()?.CurrentTheme;
		foreach (Control item in RentalUserControl.FindLogicalChildren<Control>(MainControl))
		{
			theme?.SetStyle(item);
		}
	}

	public void UpdateKeyStates()
	{
		KeyboardElem.CapKeyState = base.Model.KeyboardCapLock;
		UpdateKeyboardControl();
	}

	private void UpdateTextInputs(bool mode)
	{
		if (mode)
		{
			KeyboardElem.KeyboardMode = KeyboardMode.EMAIL_COMING_SOON;
		}
		else
		{
			KeyboardElem.KeyboardMode = KeyboardMode.ACCOUNT_SIGN_IN;
		}
	}

	private void UpdateKeyboardControl()
	{
		bool mode = base.Model == null || base.Model.KeyEmailMode;
		UpdateTextInputs(mode);
	}

	protected override void KeyTouched()
	{
		if (EmailElem.ButtonState != 0)
		{
			BorderButton emailElem = EmailElem;
			int buttonState = (CodeElem.ButtonState = 0);
			emailElem.ButtonState = buttonState;
		}
		base.Model.ContinueButtonEnabled = base.Model.ContinueButtonCommand.CanExecute(null);
	}

	protected override void KeyPressed(SymbolMode mode)
	{
		KeyboardMode lastKeyMode = base.LastKeyMode;
		base.LastKeyMode = KeyboardElem.KeyboardMode;
		switch (mode)
		{
		case SymbolMode.EMAIL_LETTER_KEY_PRESSED:
		case SymbolMode.CODE__LETTER_KEY_PRESSED:
			KeyboardElem.KeyboardMode = lastKeyMode;
			break;
		case SymbolMode.EMAIL_SYMBOL_KEY_PRESSED:
		case SymbolMode.CODE__SYMBOL_KEY_PRESSED:
			KeyboardElem.KeyboardMode = KeyboardMode.EMAIL_SYMBOLS;
			break;
		}
	}

	protected override void ContinueTouched()
	{
		base.Model.ContinueButtonEnabled = base.Model.ContinueButtonCommand.CanExecute(null);
		if (base.Model.ContinueButtonEnabled)
		{
			base.Model.ContinueButtonCommand.Execute(null);
		}
		if (!string.IsNullOrEmpty(base.Model?.KeyboardError))
		{
			BorderButton emailElem = EmailElem;
			int buttonState = (CodeElem.ButtonState = 1);
			emailElem.ButtonState = buttonState;
		}
	}

	private void NotifyUpdateTextInputs(bool state)
	{
		if (base.Model != null && base.Model.EmailSelectedCommand.CanExecute(null))
		{
			base.Model.EmailSelectedCommand.Execute(state);
		}
		UpdateKeyboardControl();
	}

	private void SetControlTextMode(bool mode)
	{
		if (mode != base.Model.KeyEmailMode)
		{
			NotifyUpdateTextInputs(mode);
		}
		(mode ? EmailElem : CodeElem).IsSelected = true;
		KeyTouched();
	}

	private void ForgotPasswordClicked(object sender, RoutedEventArgs e)
	{
		base.Model?.ForgotPasswordButtonCommand?.Execute(null);
	}

	private void EmailSelected(object sender, RoutedEventArgs e)
	{
		SetControlTextMode(mode: true);
	}

	private void CodeSelected(object sender, RoutedEventArgs e)
	{
		SetControlTextMode(mode: false);
	}

	private void ContinueTouched(object sender, RoutedEventArgs e)
	{
		ContinueTouched();
	}
}
