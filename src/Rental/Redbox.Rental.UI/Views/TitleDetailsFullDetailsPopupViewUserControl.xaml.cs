using System.Windows.Input;
using System.Windows.Markup;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "TitleDetailsFullDetailsPopupView")]
public partial class TitleDetailsFullDetailsPopupViewUserControl : TextToSpeechUserControl, IComponentConnector
{
	private TitleDetailsFullDetailsPopupViewModel TitleDetailsFullDetailsPopupViewModel => base.DataContext as TitleDetailsFullDetailsPopupViewModel;

	public TitleDetailsFullDetailsPopupViewUserControl()
	{
		InitializeComponent();
		ApplyTheme();
	}

	private void ApplyTheme()
	{
		(ServiceLocator.Instance.GetService<IThemeService>()?.CurrentTheme)?.SetStyle(CloseButton);
	}

	private void CloseCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		TitleDetailsFullDetailsPopupViewModel?.ProcessOnFullDetailsCloseButtonClicked();
	}
}
