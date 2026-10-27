using System;
using System.Collections.Generic;
using ProductPricing.Model;

namespace Redbox.Rental.Services.Pricing;

public class PricingDataInstance : IPricingDataInstance
{
	public DateTime VersionTimeStamp { get; set; }

	public GetKioskPricing KioskPricing { get; set; }

	public List<DefaultRentalPricing> DefaultRentalPricing { get; set; }

	public List<RentalPricing> RentalPricing { get; set; }

	public List<PurchasePricing> PurchasePricing { get; set; }

	public PricingDataInstance()
	{
		VersionTimeStamp = new DateTime(2000, 1, 1);
	}
}
