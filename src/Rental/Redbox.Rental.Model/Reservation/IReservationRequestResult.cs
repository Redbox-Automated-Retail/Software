using System.Collections.Generic;

namespace Redbox.Rental.Model.Reservation;

public interface IReservationRequestResult
{
	ReservationRequestStatus Status { get; set; }

	string ErrorMessage { get; set; }

	List<IReservationRequestResultItem> Items { get; set; }
}
