namespace Redbox.Rental.Model.KioskProduct;

public interface IRedboxPlusBenefit
{
	string Id { get; set; }

	string Description { get; set; }

	int Quantity { get; set; }

	RedboxPlusBenefitPeriod Period { get; set; }

	RedboxPlusBenefitType RedboxPlusBenefitType { get; set; }
}
