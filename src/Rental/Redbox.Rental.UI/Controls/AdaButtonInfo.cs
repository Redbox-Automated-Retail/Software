using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Controls;

public class AdaButtonInfo
{
	public int AdaButtonNumber { get; set; }

	public DynamicRoutedCommand ButtonCommand { get; set; }

	public IPerksOfferListItem Item { get; set; }
}
