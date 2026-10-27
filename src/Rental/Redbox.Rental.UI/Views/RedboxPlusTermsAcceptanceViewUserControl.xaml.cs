using System.Windows.Markup;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "RedboxPlusTermsAcceptanceView")]
public partial class RedboxPlusTermsAcceptanceViewUserControl : TextToSpeechUserControl, IWPFActor, IComponentConnector
{
	private RedboxPlusTermsAcceptanceViewModel RedboxPlusTermsAcceptanceViewModel => base.DataContext as RedboxPlusTermsAcceptanceViewModel;

	public RedboxPlusTermsAcceptanceViewUserControl()
	{
		InitializeComponent();
	}

	public override ISpeechControl GetSpeechControl()
	{
		return RedboxPlusTermsAcceptanceViewModel?.ProcessGetSpeechControl();
	}
}
