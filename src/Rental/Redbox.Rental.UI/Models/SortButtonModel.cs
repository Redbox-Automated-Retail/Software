using System.Windows;
using Redbox.Rental.Model.KioskProduct;

namespace Redbox.Rental.UI.Models;

public class SortButtonModel : CheckMarkButtonModel
{
	public BrowseSort Sort { get; set; }

	public SortButtonModel(Style unselectedStyle, Style selectedStyle)
		: base(unselectedStyle, selectedStyle)
	{
	}
}
