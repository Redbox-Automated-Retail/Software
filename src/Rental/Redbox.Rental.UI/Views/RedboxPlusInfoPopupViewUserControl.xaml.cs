using System.Windows.Input;
using System.Windows.Markup;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "RedboxPlusInfoPopupView")]
public partial class RedboxPlusInfoPopupViewUserControl : TextToSpeechUserControl, IComponentConnector
{
	private RedboxPlusInfoPopupViewModel RedboxPlusInfoPopupViewModel => base.DataContext as RedboxPlusInfoPopupViewModel;

	public RedboxPlusInfoPopupViewUserControl()
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
		RedboxPlusInfoPopupViewModel?.ProcessOnCloseButtonClicked();
	}
}
