using Redbox.Services.KioskBrokerServices.KioskShared.DomainObjects;

namespace Redbox.Services.KioskBrokerServices.KioskShared.ServiceInterfaces;

public interface IKioskOperations
{
	ReservationResult? Reserve(ReservationCart cart);

	ReservationResult? Reserve2(ReservationCart2 cart);

	ReservationResult Reserve3(ReservationCart3 cart);

	void Ping();

	CancelReservationResult CancelReservation(long referenceNumber);
}
