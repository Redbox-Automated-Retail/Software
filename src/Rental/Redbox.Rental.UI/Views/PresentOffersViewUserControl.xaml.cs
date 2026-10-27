using System.Windows.Markup;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "PresentOffersView")]
public partial class PresentOffersViewUserControl : TextToSpeechUserControl, IComponentConnector
{
	private PresentOffersViewModel ViewModel => base.DataContext as PresentOffersViewModel;

	public PresentOffersViewUserControl()
	{
		InitializeComponent();
	}

	public override ISpeechControl GetSpeechControl()
	{
		return ViewModel?.ProcessGetSpeechControl();
	}
}
