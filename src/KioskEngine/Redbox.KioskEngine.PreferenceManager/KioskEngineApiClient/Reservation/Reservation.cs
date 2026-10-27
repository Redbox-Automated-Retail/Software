using System;

namespace Redbox.KioskEngine.PreferenceManager.KioskEngineApiClient.Reservation;

public class Reservation
{
	public string HashedCardId { get; set; }

	public Guid Id { get; set; }

	public string ReferenceNumber { get; set; }

	public ReservationDetails ReservationDetails { get; set; }
}
