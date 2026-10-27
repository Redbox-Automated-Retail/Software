using System.Collections.Generic;

namespace ProductPricing.Model;

public class KioskPricing
{
	public long KioskId { get; set; }

	public List<long> DefaultRentalPricing { get; set; }

	public List<long> RentalPricing { get; set; }

	public List<long> PurchasePricing { get; set; }
}
