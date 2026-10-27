using System.Windows.Markup;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "PromoConfirmationView")]
public partial class PromoConfirmationViewUserControl : TextToSpeechUserControl, IComponentConnector
{
	private PromoConfirmationViewModel Model => base.DataContext as PromoConfirmationViewModel;

	public PromoConfirmationViewUserControl()
	{
		InitializeComponent();
	}

	private void ApplyTheme()
	{
		ITheme obj = ServiceLocator.Instance.GetService<IThemeService>()?.CurrentTheme;
		obj?.SetStyle(CancelButton);
		obj?.SetStyle(ApplyButton);
	}

	public override ISpeechControl GetSpeechControl()
	{
		return Model?.OnGetSpeechControl();
	}
}
