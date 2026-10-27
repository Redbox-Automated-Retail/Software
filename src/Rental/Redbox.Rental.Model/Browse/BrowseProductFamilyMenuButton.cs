using Redbox.Rental.Model.KioskProduct;

namespace Redbox.Rental.Model.Browse;

public class BrowseProductFamilyMenuButton : BrowseMenuButton
{
	public TitleFamily FilterValue { get; set; }

	public ArrowDirection ArrowDirection { get; set; }

	public BrowseProductFamilyMenuButton()
	{
		base.Height = 56;
		base.Width = 184;
		base.TextStyleName = "font_montserrat_extrabold_14_correct";
	}
}
