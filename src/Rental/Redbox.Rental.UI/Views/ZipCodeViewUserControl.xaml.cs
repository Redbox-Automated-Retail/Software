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

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "ZipCodeView")]
public partial class ZipCodeViewUserControl : TextToSpeechUserControl, IWPFActor, IComponentConnector
{
	private ZipCodeViewModel ZipCodeViewModel => base.DataContext as ZipCodeViewModel;

	public ZipCodeViewUserControl()
	{
		InitializeComponent();
		ApplyTheme();
	}

	public override ISpeechControl GetSpeechControl()
	{
		return ZipCodeViewModel?.ProcessGetSpeechControl();
	}

	private void ApplyTheme()
	{
		(ServiceLocator.Instance.GetService<IThemeService>()?.CurrentTheme)?.SetStyle(NextButton);
	}

	private void ResetIdleTimerCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		HandleWPFHit();
	}

	private void NextCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		ZipCodeViewModel?.ProcessOnNextButtonClicked(ZipCodeViewModel?.DisplayText);
		HandleWPFHit();
	}
}
