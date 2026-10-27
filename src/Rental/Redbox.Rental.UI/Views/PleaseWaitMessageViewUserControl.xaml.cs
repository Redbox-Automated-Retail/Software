using System.Windows.Markup;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "PleaseWaitMessageView")]
public partial class PleaseWaitMessageViewUserControl : TextToSpeechUserControl, IComponentConnector
{
	private PleaseWaitMessageViewModel ViewModel => base.DataContext as PleaseWaitMessageViewModel;

	public PleaseWaitMessageViewUserControl()
	{
		InitializeComponent();
	}

	public override ISpeechControl GetSpeechControl()
	{
		return ViewModel?.ProcessGetSpeechControl();
	}
}
