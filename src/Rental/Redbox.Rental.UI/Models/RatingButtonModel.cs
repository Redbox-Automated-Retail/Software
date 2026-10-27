using System.Windows;
using Redbox.Rental.Model.KioskProduct;

namespace Redbox.Rental.UI.Models;

public class RatingButtonModel : CheckMarkButtonModel
{
	public Ratings Rating { get; set; }

	public RatingButtonModel(Style unselectedStyle, Style selectedStyle)
		: base(unselectedStyle, selectedStyle)
	{
	}
}
