using System.Collections.Generic;
using Redbox.Rental.Model.Reservation;

namespace Redbox.KioskEngine.PreferenceManager.KioskEngineApiClient.Reservation;

public class ReservationRequestResult
{
	public ReservationRequestStatus Status { get; set; }

	public string ErrorMessage { get; set; }

	public List<ReservationRequestResultItem> Items { get; set; }
}
