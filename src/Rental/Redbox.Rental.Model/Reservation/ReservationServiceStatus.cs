using System;

namespace Redbox.Rental.Model.Reservation;

public class ReservationServiceStatus
{
	public bool IsRegisteredWithBrokerService { get; set; }

	public DateTime? BrokerServiceRegistrationDateTime { get; set; }

	public bool RecommendationOnPickupEnabled { get; set; }

	public bool AuthorizeAtPickup { get; set; }

	public bool GiftCardsEnabled { get; set; }

	public string KioskEngineVersion { get; set; }
}
