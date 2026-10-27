using Redbox.KioskEngine.ComponentModel.KioskServices;

namespace Redbox.Rental.Model.Authentication;

public interface IAuthenticationServices
{
	void Authenticate(string storeNumber, string userName, string password, bool useLdapAuthentication, RemoteServiceCallback completeCallback);
}
