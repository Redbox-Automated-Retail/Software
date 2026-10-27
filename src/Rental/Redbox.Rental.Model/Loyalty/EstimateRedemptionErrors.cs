using Redbox.KioskEngine.ComponentModel.KioskServices;

namespace Redbox.Rental.Model.Loyalty;

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
