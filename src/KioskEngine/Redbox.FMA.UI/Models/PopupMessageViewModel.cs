using Redbox.Rental.UI;

namespace Redbox.FMA.Models;

public class PopupMessageViewModel
{
	// Both are set in PopupMessageViewController.ConfigureView (object initializer).
	public string MessageText { get; set; } = null!;

	public DynamicRoutedCommand ContinueCommand { get; set; } = null!;
}
