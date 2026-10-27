using System;
using System.Collections.Generic;
using ProductPricing.Model;

namespace Redbox.Rental.Services.Pricing;

public interface IPricingDataInstance
{
	DateTime VersionTimeStamp { get; set; }

	GetKioskPricing KioskPricing { get; }

	List<DefaultRentalPricing> DefaultRentalPricing { get; set; }

	List<RentalPricing> RentalPricing { get; set; }

	List<PurchasePricing> PurchasePricing { get; set; }
}
