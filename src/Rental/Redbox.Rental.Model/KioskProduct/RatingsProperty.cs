namespace Redbox.Rental.Model.KioskProduct;

public class RatingsProperty
{
	public string Name { get; set; }

	public string Code { get; set; }

	public string CultureStringName { get; set; }

	public string Description { get; set; }

	public Ratings MPAARatingEquivalent { get; set; }

	public bool IsMPAARating { get; set; }
}
