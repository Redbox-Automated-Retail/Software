using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows;
using System.Windows.Markup;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.UI.Controls;

namespace Redbox.Rental.UI.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "KeyboardLeadGenerationSignupView")]
public partial class KeyboardLeadGenerationSignupViewUserControl : KeyboardControl, IComponentConnector
{
	public KeyboardLeadGenerationSignupViewUserControl()
	{
		base.KeyboardMode = KeyboardMode.LEAD_GENERATION_SIGN_UP;
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
