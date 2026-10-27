using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Markup;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "TitleDetailsWatchOptionsPopupView")]
public partial class TitleDetailsWatchOptionsPopupViewUserControl : TextToSpeechUserControl, IComponentConnector
{
	private TitleDetailsWatchOptionsPopupViewModel TitleDetailsWatchOptionsPopupViewModel => base.DataContext as TitleDetailsWatchOptionsPopupViewModel;

	public TitleDetailsWatchOptionsPopupViewUserControl()
	{
		InitializeComponent();
		ApplyTheme();
	}

	private void ApplyTheme()
	{
		ITheme obj = ServiceLocator.Instance.GetService<IThemeService>()?.CurrentTheme;
		obj?.SetStyle(SeeFullDetailsButton);
		obj?.SetStyle(CloseButton);
	}

	private void SeeFullDetailsCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		TitleDetailsWatchOptionsPopupViewModel?.ProcessOnWatchOptionsSeeFullDetailsButtonClicked();
	}

	private void CloseCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		TitleDetailsWatchOptionsPopupViewModel?.ProcessOnWatchOptionsCloseButtonClicked();
	}
}
