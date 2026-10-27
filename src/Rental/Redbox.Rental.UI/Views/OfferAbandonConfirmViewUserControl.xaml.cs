using System.Windows.Markup;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "OfferAbandonConfirmView")]
public partial class OfferAbandonConfirmViewUserControl : ViewUserControl, IComponentConnector
{
	private OfferAbandonConfirmModel Model => base.DataContext as OfferAbandonConfirmModel;

	public OfferAbandonConfirmViewUserControl()
	{
		InitializeComponent();
	}

	public override ISpeechControl GetSpeechControl()
	{
		return Model.ProcessGetSpeechControl();
	}
}
