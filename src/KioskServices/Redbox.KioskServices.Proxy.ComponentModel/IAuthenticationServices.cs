using Redbox.KioskEngine.ComponentModel.KioskServices;

namespace Redbox.KioskServices.Proxy.ComponentModel;

public interface IAuthenticationServices
{
	bool EnableLocalAccountCaching { get; set; }

	bool LiveCall { get; set; }

	bool SimulateCommunicationsFailure { get; set; }

	bool SimulateServerBusyError { get; set; }

	string[] Users { get; set; }

	void Authenticate(string storeNumber, string userName, string password, bool useLdapAuthentication, RemoteServiceCallback completeCallback);

	bool CanSwitch();

	void Reset();
}
