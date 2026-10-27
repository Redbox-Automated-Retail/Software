using System.Collections.Generic;
using ProductPricing.Model;

namespace Redbox.Rental.Services.Pricing;

public class FormatPricing
{
	public PriceLevelType Name { get; set; }

	public IList<KCSRentalPricing> Prices { get; set; }
}
