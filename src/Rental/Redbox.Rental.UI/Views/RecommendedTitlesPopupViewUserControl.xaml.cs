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

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "RecommendedTitlesPopupView")]
public partial class RecommendedTitlesPopupViewUserControl : TextToSpeechUserControl, IWPFActor, IComponentConnector
{
	private RecommendedTitlesPopupViewModel RecommendedTitlesPopupViewModel => base.DataContext as RecommendedTitlesPopupViewModel;

	public RecommendedTitlesPopupViewUserControl()
	{
		InitializeComponent();
		ApplyTheme();
	}

	public override ISpeechControl GetSpeechControl()
	{
		return RecommendedTitlesPopupViewModel?.ProcessGetSpeechControl();
	}

	private void ApplyTheme()
	{
		(ServiceLocator.Instance.GetService<IThemeService>()?.CurrentTheme)?.SetStyle(CancelButton);
	}

	private void ResetIdleTimerCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		HandleWPFHit();
	}

	private void HowPointsWorkCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		RecommendedTitlesPopupViewModel?.ProcessOnHowPointsWorkButtonClicked();
	}

	private void PickupCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		RecommendedTitlesPopupViewModel?.ProcessOnPickupButtonClicked();
	}

	private void CancelCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		RecommendedTitlesPopupViewModel?.ProcessOnCancelButtonClicked();
	}
}
