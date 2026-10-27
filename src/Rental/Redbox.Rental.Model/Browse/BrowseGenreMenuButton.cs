using Redbox.Rental.Model.KioskProduct;

namespace Redbox.Rental.Model.Browse;

public class BrowseGenreMenuButton : BrowseMenuButton
{
	public Genres? FilterValue { get; set; }

	public bool IsDeselectable { get; set; }
}
