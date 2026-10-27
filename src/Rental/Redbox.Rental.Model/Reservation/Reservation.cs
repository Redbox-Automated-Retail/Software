using System;

namespace Redbox.Rental.Model.Reservation;

public class Reservation : IReservation
{
	public string HashedCardId { get; set; }

	public string ReferenceNumber { get; set; }

	public Guid Id { get; set; }

	public IReservationDetails ReservationDetails { get; set; }
}
