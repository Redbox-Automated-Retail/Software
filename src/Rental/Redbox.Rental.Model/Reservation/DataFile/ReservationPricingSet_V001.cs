using System.Collections.Generic;

namespace Redbox.Rental.Model.Reservation.DataFile;

public class ReservationPricingSet_V001
{
	public string Name { get; set; }

	public string ProgramName { get; set; }

	public string PriceSetId { get; set; }

	public List<ReservationPricingRecord_V001> PriceRecords { get; set; }
}
