using System;
using System.Collections.ObjectModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;

namespace Redbox.Rental.UI.Models;

public class OptionSelectionDialogViewModel : BaseModel<OptionSelectionDialogViewModel>
{
	public Func<ISpeechControl> OnGetSpeechControl;

	public string HeaderText { get; set; }

	public string DescriptionText { get; set; }

	public ObservableCollection<ButtonData> ButtonData { get; set; }

	public string CancelButtonText { get; set; }

	public int DialogWidth { get; set; }

	public DynamicRoutedCommand CancelCommand { get; set; }

	public OptionSelectionDialogViewModel()
	{
		ButtonData = new ObservableCollection<ButtonData>();
	}
}
