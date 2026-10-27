using Redbox.KioskEngine.ComponentModel.KioskServices;

namespace Redbox.KioskServices.Proxy.ComponentModel;

public enum UpdateAccountResponseErrors
{
	None = 0,
	[ResponseCode(Name = "(UPA001)")]
	PhoneNumberConflict = 1,
	[ResponseCode(Name = "(UPA002)")]
	CommunicationError = 2,
	Error = 99
}
