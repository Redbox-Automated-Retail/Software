namespace Redbox.Rental.Model.Personalization;

public class BenefitUsage : IBenefitUsage
{
	public string BenefitId { get; set; }

	public int Issued { get; set; }

	public int Used { get; set; }
}
