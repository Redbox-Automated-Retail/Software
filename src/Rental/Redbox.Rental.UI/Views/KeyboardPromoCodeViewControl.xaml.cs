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

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "KeyboardPromoView")]
public partial class KeyboardPromoCodeViewControl : KeyboardControl, IComponentConnector
{
	public KeyboardPromoCodeViewControl()
	{
		base.KeyboardMode = KeyboardMode.PROMOTIONS_CODE;
		InitializeComponent();
		KeyboardElem.KeyboardMode = base.KeyboardMode;
		InitKeyboardUserControl(KeyboardElem, PromoElem);
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

	private void PromoSelected(object sender, RoutedEventArgs e)
	{
		KeyTouched();
	}
}
