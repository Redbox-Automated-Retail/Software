using Redbox.KioskEngine.ComponentModel.KioskServices;

namespace Redbox.Rental.Model.Personalization;

public enum KioskLoginResponseErrors
{
	None = 0,
	[ResponseCode(Name = "(KLI001)")]
	AccountLocked = 1,
	[ResponseCode(Name = "(KLI002)")]
	InvalidPasswordOrPin = 2,
	[ResponseCode(Name = "(KLI003)")]
	AccountNotFound = 3,
	[ResponseCode(Name = "(KLI004)")]
	CommunicationError = 4,
	[ResponseCode(Name = "(KLI005)")]
	LoyaltyIsDown = 5,
	[ResponseCode(Name = "(KLI006)")]
	LoyaltyAccountNotFound = 6,
	[ResponseCode(Name = "(KLI007)")]
	SubscriptionServiceIsDown = 7,
	MobilePassReadError = 90,
	MobilePassGoogleNoPassFoundError = 91,
	Error = 99
}
