using System.Windows.Markup;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "SubscriptionSignUpConfirmationView")]
public partial class SubscriptionSignUpConfirmationViewUserControl : TextToSpeechUserControl, IWPFActor, IComponentConnector
{
	private SubscriptionSignUpConfirmationViewModel SubscriptionSignUpConfirmationViewModel => base.DataContext as SubscriptionSignUpConfirmationViewModel;

	public SubscriptionSignUpConfirmationViewUserControl()
	{
		InitializeComponent();
	}

	public override ISpeechControl GetSpeechControl()
	{
		return SubscriptionSignUpConfirmationViewModel?.ProcessGetSpeechControl();
	}
}
