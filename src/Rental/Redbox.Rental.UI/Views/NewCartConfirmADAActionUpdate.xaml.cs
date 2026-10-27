using System.Windows.Markup;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "NewCartConfirmADAActionUpdateView")]
public partial class NewCartConfirmADAActionUpdate : TextToSpeechUserControl, IComponentConnector
{
	private NewCartConfirmADAActionUpdateViewModel Model => base.DataContext as NewCartConfirmADAActionUpdateViewModel;

	public NewCartConfirmADAActionUpdate()
	{
		InitializeComponent();
	}

	public override ISpeechControl GetSpeechControl()
	{
		return Model?.ProcessGetSpeechControl();
	}
}
