using System.Windows.Markup;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "PerksSignUpInfoPopupView")]
public partial class PerksSignUpInfoPopupViewUserControl : TextToSpeechUserControl, IComponentConnector
{
	public PerksSignUpInfoPopupViewUserControl()
	{
		InitializeComponent();
	}

	public override ISpeechControl GetSpeechControl()
	{
		return ((PerksSignUpInfoPopupViewModel)base.DataContext)?.ProcessGetSpeechControl?.Invoke();
	}
}
