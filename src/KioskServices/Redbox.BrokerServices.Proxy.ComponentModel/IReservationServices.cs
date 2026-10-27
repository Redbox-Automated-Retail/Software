using System.Collections.Generic;

namespace Redbox.BrokerServices.Proxy.ComponentModel;

public interface IReservationServices
{
	bool CanSwitch();

	void Reset();

	void Register(string storeNumber, IDictionary<string, IDictionary<string, IDictionary<string, decimal>>> prices, ReservationRequestCallback3 reservationRequestCallback, CancelReservationCallback cancelRequestCallback);

	IReservedItem NewReservedItem();

	IReservationResult NewReservationResult();

	ILocalCancelReservationResult NewCancelReservationResult();
}
