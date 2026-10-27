using Redbox.Rental.Model.KioskProduct;

namespace Redbox.Rental.Services.KioskProduct;

public class RedboxPlusBenefit : IRedboxPlusBenefit
{
	public string Id { get; set; }

	public string Description { get; set; }

	public int Quantity { get; set; }

	public RedboxPlusBenefitPeriod Period { get; set; }

	public RedboxPlusBenefitType RedboxPlusBenefitType { get; set; }
}
