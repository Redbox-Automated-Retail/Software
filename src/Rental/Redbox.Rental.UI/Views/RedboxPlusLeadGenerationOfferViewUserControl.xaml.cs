using System.Windows.Input;
using System.Windows.Markup;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "RedboxPlusLeadGenerationOfferView")]
public partial class RedboxPlusLeadGenerationOfferViewUserControl : TextToSpeechUserControl, IWPFActor, IComponentConnector
{
	private RedboxPlusLeadGenerationOfferViewModel RedboxPlusLeadGenerationOfferViewModel => base.DataContext as RedboxPlusLeadGenerationOfferViewModel;

	public RedboxPlusLeadGenerationOfferViewUserControl()
	{
		InitializeComponent();
	}

	private void NoButtonCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		RedboxPlusLeadGenerationOfferViewModel?.ProcessOnNoButtonClicked();
	}

	private void EmailButtonCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		RedboxPlusLeadGenerationOfferViewModel?.ProcessOnEmailButtonClicked();
	}

	private void QRCodeButtonCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		RedboxPlusLeadGenerationOfferViewModel?.ProcessOnQRCodeButtonClicked();
	}
}
