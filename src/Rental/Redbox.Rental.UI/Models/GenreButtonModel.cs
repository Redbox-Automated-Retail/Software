using System.Windows;
using Redbox.Rental.Model.KioskProduct;

namespace Redbox.Rental.UI.Models;

public class GenreButtonModel : CheckMarkButtonModel
{
	public Genres Genre { get; set; }

	public GenreButtonModel(Style unselectedStyle, Style selectedStyle)
		: base(unselectedStyle, selectedStyle)
	{
	}
}
