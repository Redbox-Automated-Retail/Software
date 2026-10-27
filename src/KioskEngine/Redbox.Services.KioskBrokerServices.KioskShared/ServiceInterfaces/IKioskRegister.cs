namespace Redbox.Services.KioskBrokerServices.KioskShared.ServiceInterfaces;

public interface IKioskRegister
{
	void Register(int kioskID, IKioskOperations kiosk, string kioskVersion);

	bool IsRegistered(int kioskID);
}
