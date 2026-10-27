using System;

namespace Redbox.Rental.Model.Reservation;

public interface IReservation
{
	string HashedCardId { get; set; }

	Guid Id { get; set; }

	string ReferenceNumber { get; set; }

	IReservationDetails ReservationDetails { get; set; }
}
