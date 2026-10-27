using System.Collections.ObjectModel;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views;

public class OptionSelectionDialogInfo : IViewAnalyticsName
{
	public string ViewAnalyticsName { get; set; }

	public string ViewAnalyticsContext { get; set; }

	public string HeaderText { get; set; }

	public string DescriptionText { get; set; }

	public ObservableCollection<ButtonData> ButtonData { get; set; }

	public string CancelButtonText { get; set; }

	public DynamicRoutedCommand CancelCommand { get; set; }

	public OptionSelectionDialogInfo()
	{
		ButtonData = new ObservableCollection<ButtonData>();
	}
}
