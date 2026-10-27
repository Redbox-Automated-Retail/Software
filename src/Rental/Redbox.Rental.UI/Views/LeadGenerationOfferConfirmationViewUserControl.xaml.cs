using System.Windows.Input;
using System.Windows.Markup;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "LeadGenerationOfferConfirmationView")]
public partial class LeadGenerationOfferConfirmationViewUserControl : TextToSpeechUserControl, IWPFActor, IComponentConnector
{
	private LeadGenerationOfferConfirmationViewModel LeadGenerationOfferConfirmationViewModel => base.DataContext as LeadGenerationOfferConfirmationViewModel;

	public LeadGenerationOfferConfirmationViewUserControl()
	{
		InitializeComponent();
	}

	private void GotItButtonCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		LeadGenerationOfferConfirmationViewModel?.ProcessOnGotItButtonClicked();
	}
}
