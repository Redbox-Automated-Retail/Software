namespace Redbox.Rental.Model.Personalization;

public interface IBenefitUsage
{
	string BenefitId { get; set; }

	int Issued { get; set; }

	int Used { get; set; }
}
