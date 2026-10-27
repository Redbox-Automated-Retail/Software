using System.Collections.Generic;

namespace Redbox.Rental.Model.Pricing;

public interface IPricingSet
{
	string Name { get; set; }

	string ProgramName { get; set; }

	string PriceSetId { get; set; }

	List<IPricingRecord> PriceRecords { get; set; }

	bool IsEqual(IPricingSet comparePricingSet);
}
