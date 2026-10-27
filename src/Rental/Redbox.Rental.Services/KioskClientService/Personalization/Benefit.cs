using Redbox.Rental.Model.KioskClientService.Personalization;

namespace Redbox.Rental.Services.KioskClientService.Personalization;

public class Benefit
{
	public long Id { get; set; }

	public string Name { get; set; }

	public int? UsagesPerPeriod { get; set; }

	public BenefitPeriod Period { get; set; }

	public bool IsAvailable { get; set; }

	public decimal? MaxValue { get; set; }
}
