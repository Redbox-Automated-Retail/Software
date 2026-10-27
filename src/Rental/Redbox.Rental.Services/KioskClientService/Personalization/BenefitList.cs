using System.Collections.Generic;
using System.Linq;

namespace Redbox.Rental.Services.KioskClientService.Personalization;

public class BenefitList : List<Benefit>
{
	public Benefit FreeNightRentalBenefit => this.FirstOrDefault((Benefit x) => x.Id == 2);

	public Benefit EarnPointsFor1ExtraNightBenefit => this.FirstOrDefault((Benefit x) => x.Id == 3);
}
