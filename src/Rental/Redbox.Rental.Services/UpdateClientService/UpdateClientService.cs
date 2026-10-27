using Redbox.Rental.Model.UpdateClientService;
using Redbox.Rental.Model.UpdateClientService.Configuration;
using Redbox.Rental.Model.UpdateClientService.KioskHealth;
using Redbox.Rental.Model.UpdateClientService.Reservation;
using Redbox.Rental.Services.UpdateClientService.Configuration;
using Redbox.Rental.Services.UpdateClientService.KioskHealth;
using Redbox.Rental.Services.UpdateClientService.Reservation;

namespace Redbox.Rental.Services.UpdateClientService;

public class UpdateClientService : IUpdateClientService
{
	public IUpdateClientServiceReservation Reservation { get; private set; }

	public IUpdateClientServiceKioskHealth KioskHealth { get; private set; }

	public IUpdateClientServiceConfiguration Configuration { get; private set; }

	public UpdateClientService()
	{
		Reservation = new UpdateClientServiceReservation();
		KioskHealth = new UpdateClientServiceKioskHealth();
		Configuration = new UpdateClientServiceConfiguration();
	}
}
