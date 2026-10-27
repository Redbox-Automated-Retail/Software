namespace Redbox.Rental.UI.Models;

public interface IPerksOfferListItem
{
	int AdaButtonNumber { get; set; }

	bool IsAdaMode { get; set; }

	int NumberOfSpaces { get; }
}
