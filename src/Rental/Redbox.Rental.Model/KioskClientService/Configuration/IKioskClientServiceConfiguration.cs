namespace Redbox.Rental.Model.KioskClientService.Configuration;

public interface IKioskClientServiceConfiguration
{
	KioskConfiguration GetCurrentKioskConfiguration(string kioskClientServiceBaseUrl);

	KioskConfiguration GetKioskSessionKioskConfiguration(string kioskClientServiceBaseUrl);
}
