using System.Security;
using Redbox.KioskEngine.ComponentModel.KioskServices;

namespace Redbox.KioskServices.Proxy.ComponentModel;

public interface IPersonalizationServices
{
	string CustomerProfileNumber { get; set; }

	void UpdateAccount(int kioskId, string customerProfileNumber, string emailAddress, string phoneNumber, SecureString pin, bool punchcardOptIn, bool textClubOptIn, RemoteServiceCallback completeCallback, bool queued = false, AccountSourceType source = AccountSourceType.General);
}
