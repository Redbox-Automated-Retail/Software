namespace Redbox.Rental.Services.KioskClientService.Personalization;

public class BenefitPlan
{
	public int Id { get; set; }

	public string Name { get; set; }

	public BenefitList Benefits { get; set; }
}
