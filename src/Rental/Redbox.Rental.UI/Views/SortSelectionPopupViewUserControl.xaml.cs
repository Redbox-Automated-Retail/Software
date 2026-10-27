using System.Windows.Input;
using System.Windows.Markup;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "SortSelectionPopupView")]
public partial class SortSelectionPopupViewUserControl : TextToSpeechUserControl, IComponentConnector
{
	private SortSelectionPopupViewModel SortSelectionPopupViewModel => base.DataContext as SortSelectionPopupViewModel;

	public SortSelectionPopupViewUserControl()
	{
		InitializeComponent();
		ApplyTheme();
	}

	private void ApplyTheme()
	{
		ITheme obj = ServiceLocator.Instance.GetService<IThemeService>()?.CurrentTheme;
		obj?.SetStyle(CancelButton);
		obj?.SetStyle(ApplyButton);
	}

	private void CancelCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		SortSelectionPopupViewModel?.ProcessOnCancelButtonClicked();
	}

	private void ApplyCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		SortSelectionPopupViewModel?.ProcessOnApplyButtonClicked();
	}
}
