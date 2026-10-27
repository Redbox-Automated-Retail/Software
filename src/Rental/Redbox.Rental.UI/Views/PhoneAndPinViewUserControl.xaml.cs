using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Markup;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "PhoneAndPinView")]
public partial class PhoneAndPinViewUserControl : TextToSpeechUserControl, IWPFActor, IComponentConnector
{
	private PhoneAndPinViewModel PhoneAndPinViewModel => base.DataContext as PhoneAndPinViewModel;

	public PhoneAndPinViewUserControl()
	{
		InitializeComponent();
		ApplyTheme();
	}

	public override ISpeechControl GetSpeechControl()
	{
		return PhoneAndPinViewModel?.ProcessGetSpeechControl();
	}

	private void ApplyTheme()
	{
		ITheme obj = ServiceLocator.Instance.GetService<IThemeService>()?.CurrentTheme;
		obj?.SetStyle(StartButton);
		obj?.SetStyle(SignInButton);
		obj?.SetStyle(TermsButton);
	}

	private void PhoneDisplayCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		PhoneAndPinViewModel?.ProcessOnDisplayClicked(PhoneAndPinViewModel.DisplayType.Phone);
	}

	private void PinDisplayCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		PhoneAndPinViewModel?.ProcessOnDisplayClicked(PhoneAndPinViewModel.DisplayType.Pin);
	}

	private void PinConfirmDisplayCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		PhoneAndPinViewModel?.ProcessOnDisplayClicked(PhoneAndPinViewModel.DisplayType.PinConfirm);
	}

	private void StartCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		RunCommandOnce(delegate
		{
			try
			{
				PhoneAndPinViewModel?.ProcessOnStartButtonClicked();
			}
			finally
			{
				CompleteRunOnce();
			}
		});
	}

	private void SignInCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		RunCommandOnce(delegate
		{
			try
			{
				PhoneAndPinViewModel?.ProcessOnSignInButtonClicked();
			}
			finally
			{
				CompleteRunOnce();
			}
		});
	}

	private void TermsButtonCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		RunCommandOnce(delegate
		{
			try
			{
				PhoneAndPinViewModel?.ProcessOnTermsButtonClicked();
			}
			finally
			{
				CompleteRunOnce();
			}
		});
	}
}
