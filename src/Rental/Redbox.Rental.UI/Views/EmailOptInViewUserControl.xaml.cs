using System.Windows.Input;
using System.Windows.Markup;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "EmailOptInView")]
public partial class EmailOptInViewUserControl : TextToSpeechUserControl, IComponentConnector
{
	private EmailOptInViewModel Model => base.DataContext as EmailOptInViewModel;

	public EmailOptInViewUserControl()
	{
		InitializeComponent();
	}

	private void Button1_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		Model?.ProcessOnYesButtonClicked();
	}

	private void Button2_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		Model?.ProcessOnNoThanksButtonClicked();
	}
}
