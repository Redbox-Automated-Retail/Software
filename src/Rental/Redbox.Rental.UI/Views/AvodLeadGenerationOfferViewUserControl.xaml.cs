using System.Windows.Input;
using System.Windows.Markup;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "AvodLeadGenerationOfferView")]
public partial class AvodLeadGenerationOfferViewUserControl : TextToSpeechUserControl, IWPFActor, IComponentConnector
{
	private AvodLeadGenerationOfferViewModel AvodLeadGenerationOfferViewModel => base.DataContext as AvodLeadGenerationOfferViewModel;

	public AvodLeadGenerationOfferViewUserControl()
	{
		InitializeComponent();
	}

	private void NoButtonCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		AvodLeadGenerationOfferViewModel?.ProcessOnNoButtonClicked();
	}

	private void EmailButtonCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		AvodLeadGenerationOfferViewModel?.ProcessOnEmailButtonClicked();
	}

	private void QRCodeButtonCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		AvodLeadGenerationOfferViewModel?.ProcessOnQRCodeButtonClicked();
	}
}
