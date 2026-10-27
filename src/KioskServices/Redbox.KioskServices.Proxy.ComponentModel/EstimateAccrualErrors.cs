using Redbox.KioskEngine.ComponentModel.KioskServices;

namespace Redbox.KioskServices.Proxy.ComponentModel;

public enum EstimateAccrualErrors
{
	Error,
	[ResponseCode(Name = "(EA001)")]
	AccountNotFound,
	[ResponseCode(Name = "(EA002)")]
	LoyaltyError,
	[ResponseCode(Name = "(EA003)")]
	CommunicationError
}
