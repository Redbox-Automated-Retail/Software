using System.Windows.Markup;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "MemberPerksPopupView")]
public partial class MemberPerksPopupView : TextToSpeechUserControl, IComponentConnector
{
	public MemberPerksPopupView()
	{
		InitializeComponent();
	}

	public override ISpeechControl GetSpeechControl()
	{
		return ((MemberPerksPopupViewModel)base.DataContext)?.ProcessGetSpeechControl?.Invoke();
	}
}
