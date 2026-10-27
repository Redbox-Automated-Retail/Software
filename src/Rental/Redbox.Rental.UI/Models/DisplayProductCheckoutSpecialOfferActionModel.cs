using System.Windows;

namespace Redbox.Rental.UI.Models;

public class DisplayProductCheckoutSpecialOfferActionModel
{
	public string TextLine1 { get; set; }

	public string TextLine2 { get; set; }

	public string ButtonText { get; set; }

	public Visibility ButtonVisibility { get; set; }

	public DynamicRoutedCommand ButtonCommand { get; set; }
}
