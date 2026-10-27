using System.Windows;
using System.Windows.Markup;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "OptionSelectionDialog")]
public partial class OptionSelectionDialogUserControl : TextToSpeechUserControl, IComponentConnector
{
	private OptionSelectionDialogViewModel OptionSelectionDialogViewModel => base.DataContext as OptionSelectionDialogViewModel;

	public OptionSelectionDialogUserControl()
	{
		InitializeComponent();
	}

	private void FormatSelectionPopupUserControl_Loaded(object sender, RoutedEventArgs e)
	{
		ApplyTheme();
	}

	private void ApplyTheme()
	{
		ITheme theme = ServiceLocator.Instance.GetService<IThemeService>()?.CurrentTheme;
		for (int i = 0; i < ButtonsItemsControl.Items.Count; i++)
		{
			FrameworkElement frameworkElement = (FrameworkElement)ButtonsItemsControl.ItemContainerGenerator.ContainerFromIndex(i);
			if (frameworkElement != null)
			{
				theme?.SetStyle(frameworkElement);
			}
		}
		theme?.SetStyle(CancelButton);
	}

	public override ISpeechControl GetSpeechControl()
	{
		return OptionSelectionDialogViewModel?.OnGetSpeechControl?.Invoke();
	}
}
