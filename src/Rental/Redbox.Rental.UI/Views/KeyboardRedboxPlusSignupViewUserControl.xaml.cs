using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows;
using System.Windows.Markup;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.UI.Controls;

namespace Redbox.Rental.UI.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "KeyboardRedboxPlusSignupView")]
public partial class KeyboardRedboxPlusSignupViewUserControl : KeyboardControl, IComponentConnector
{
	public KeyboardRedboxPlusSignupViewUserControl()
	{
		base.KeyboardMode = KeyboardMode.REDBOX_PLUS_SIGN_UP;
		InitializeComponent();
		KeyboardElem.KeyboardMode = base.KeyboardMode;
		InitKeyboardUserControl(KeyboardElem, EmailElem);
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
