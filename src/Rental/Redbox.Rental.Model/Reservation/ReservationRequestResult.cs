using System.Collections.Generic;

namespace Redbox.Rental.Model.Reservation;

public class ReservationRequestResult : IReservationRequestResult
{
	public ReservationRequestStatus Status { get; set; }

	public string ErrorMessage { get; set; }

	public List<IReservationRequestResultItem> Items { get; set; } = new List<IReservationRequestResultItem>();
}
