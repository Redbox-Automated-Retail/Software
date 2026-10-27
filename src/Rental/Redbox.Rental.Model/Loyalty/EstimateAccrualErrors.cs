using Redbox.KioskEngine.ComponentModel.KioskServices;

namespace Redbox.Rental.Model.Loyalty;

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
