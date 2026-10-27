using Redbox.KioskEngine.ComponentModel.KioskServices;

namespace Redbox.KioskServices.Proxy.ComponentModel;

public enum EstimateRedemptionErrors
{
	Error,
	[ResponseCode(Name = "(ER001)")]
	AccountNotFound,
	[ResponseCode(Name = "(ER002)")]
	LoyaltyError,
	[ResponseCode(Name = "(ER003)")]
	CommunicationError
}
