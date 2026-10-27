using System.Windows.Input;
using System.Windows.Markup;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "MultiNightPricingPopupView")]
public partial class MultiNightPricingPopupViewUserControl : TextToSpeechUserControl, IWPFActor, IComponentConnector
{
	private MultiNightPricingPopupViewModel MultiNightPricingPopupViewModel => base.DataContext as MultiNightPricingPopupViewModel;

	public MultiNightPricingPopupViewUserControl()
	{
		InitializeComponent();
		ApplyTheme();
	}

	public override ISpeechControl GetSpeechControl()
	{
		return MultiNightPricingPopupViewModel?.ProcessGetSpeechControl();
	}

	private void ApplyTheme()
	{
		ITheme obj = ServiceLocator.Instance.GetService<IThemeService>()?.CurrentTheme;
		obj?.SetStyle(OneNightButton);
		obj?.SetStyle(MultiNightButton);
		obj?.SetStyle(BuyButton);
		obj?.SetStyle(CancelButton);
	}

	private void OneNightCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		MultiNightPricingPopupViewModel?.ProcessOnOneNightButtonClicked();
	}

	private void MultiNightCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		MultiNightPricingPopupViewModel?.ProcessOnMultiNightButtonClicked();
	}

	private void BuyCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		MultiNightPricingPopupViewModel?.ProcessOnBuyButtonClicked();
	}

	private void CancelCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		MultiNightPricingPopupViewModel?.ProcessOnCancelButtonClicked();
	}
}
