using System.Collections.Generic;

namespace ProductPricing.Model;

public class GetKioskPricing
{
	public IDictionary<long, KioskPricing> KioskPricing { get; set; }

	public IDictionary<long, DefaultRentalPricing> DefaultRentalPricing { get; set; }

	public IDictionary<long, RentalPricing> RentalPricing { get; set; }

	public IDictionary<long, PurchasePricing> PurchasePricing { get; set; }
}
