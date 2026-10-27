using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.UI.Controls;

namespace Redbox.Rental.UI.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "KeyboardPerksSignupView")]
public partial class KeyboardPerksSignupViewControl : KeyboardControl, IComponentConnector
{
	public KeyboardPerksSignupViewControl()
	{
		base.KeyboardMode = KeyboardMode.PERKS_SIGN_UP;
		InitializeComponent();
		KeyboardElem.KeyboardMode = base.KeyboardMode;
		InitKeyboardUserControl(KeyboardElem, EmailElem);
		ApplyTheme();
	}

	private void ApplyTheme()
	{
		ITheme theme = ServiceLocator.Instance.GetService<IThemeService>()?.CurrentTheme;
		foreach (Control item in RentalUserControl.FindLogicalChildren<Control>(MainControl))
		{
			theme?.SetStyle(item);
		}
	}

	private void ContinueTouched(object sender, RoutedEventArgs e)
	{
		ContinueTouched();
	}

	private void EmailSelected(object sender, RoutedEventArgs e)
	{
		KeyTouched();
	}
}
