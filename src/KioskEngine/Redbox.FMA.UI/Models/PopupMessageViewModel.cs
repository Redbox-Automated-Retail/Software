using Redbox.Rental.UI;

namespace Redbox.FMA.Models;

public class PopupMessageViewModel
{
	public string MessageText { get; set; }

	public DynamicRoutedCommand ContinueCommand { get; set; }
}
