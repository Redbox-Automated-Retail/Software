using System.Collections.ObjectModel;

namespace Redbox.Rental.UI.Models.DesignTime;

public class DesignOptionSelectionDialogViewModel
{
	public string HeaderText { get; set; }

	public string DescriptionText { get; set; }

	public ObservableCollection<ButtonData> ButtonData { get; set; }

	public string CancelButtonText { get; set; }

	public int DialogHeight { get; set; }

	public int DialogWidth { get; set; }

	public DynamicRoutedCommand CancelCommand { get; set; }

	public DesignOptionSelectionDialogViewModel()
	{
		ButtonData = new ObservableCollection<ButtonData>();
		ButtonData.Add(new ButtonData("Test Button 1", new DynamicRoutedCommand()));
		ButtonData.Add(new ButtonData("Test Button 2", new DynamicRoutedCommand()));
		ButtonData.Add(new ButtonData("Test Button 3", new DynamicRoutedCommand()));
		ButtonData.Add(new ButtonData("Test Button 4", new DynamicRoutedCommand()));
		HeaderText = "These are your options";
		DescriptionText = "Please choose an option\r\nLine 2:";
		CancelButtonText = "CANCEL";
		DialogHeight = 526;
		DialogWidth = 720;
	}
}
