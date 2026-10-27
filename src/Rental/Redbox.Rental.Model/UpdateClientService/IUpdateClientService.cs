using Redbox.Rental.Model.UpdateClientService.Configuration;
using Redbox.Rental.Model.UpdateClientService.KioskHealth;
using Redbox.Rental.Model.UpdateClientService.Reservation;

namespace Redbox.Rental.Model.UpdateClientService;

public interface IUpdateClientService
{
	IUpdateClientServiceReservation Reservation { get; }

	IUpdateClientServiceKioskHealth KioskHealth { get; }

	IUpdateClientServiceConfiguration Configuration { get; }
}
