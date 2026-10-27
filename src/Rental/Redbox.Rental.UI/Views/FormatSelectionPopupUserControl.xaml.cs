using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "FormatSelectionPopupView")]
public partial class FormatSelectionPopupUserControl : TextToSpeechUserControl, IComponentConnector
{
	private FormatSelectionPopupViewModel FormatSelectionPopupViewModel => base.DataContext as FormatSelectionPopupViewModel;

	public FormatSelectionPopupUserControl()
	{
		InitializeComponent();
		base.Loaded += FormatSelectionPopupUserControl_Loaded;
	}

	private void FormatSelectionPopupUserControl_Loaded(object sender, RoutedEventArgs e)
	{
		ApplyTheme();
	}

	private void ApplyTheme()
	{
		ITheme theme = ServiceLocator.Instance.GetService<IThemeService>()?.CurrentTheme;
		for (int i = 0; i < FormatButtonsItemsControl.Items.Count; i++)
		{
			FrameworkElement frameworkElement = (FrameworkElement)FormatButtonsItemsControl.ItemContainerGenerator.ContainerFromIndex(i);
			if (frameworkElement != null)
			{
				theme?.SetStyle(frameworkElement);
			}
		}
		theme?.SetStyle(PurchaseButton);
		theme?.SetStyle(CancelButton);
		theme?.SetStyle(DigitalCodeInfoButton);
	}

	private void FormatCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		FormatSelectionPopupViewModel?.ProcessFormatButtonClicked((TitleType)((Control)e.OriginalSource).Tag);
	}

	private void PurchaseCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		FormatSelectionPopupViewModel?.ProcessPurchaseButtonClicked();
	}

	private void CancelCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		FormatSelectionPopupViewModel?.ProcessCancelButtonClicked();
	}

	private void DigitalCodeInfoCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		FormatSelectionPopupViewModel?.ProcessDigitalCodeInfoButtonClicked();
	}

	public override ISpeechControl GetSpeechControl()
	{
		return FormatSelectionPopupViewModel?.OnGetSpeechControl?.Invoke();
	}
}
