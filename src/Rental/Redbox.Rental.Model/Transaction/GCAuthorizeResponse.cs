using Redbox.KioskEngine.ComponentModel.KioskServices;

namespace Redbox.Rental.Model.Transaction;

public enum GCAuthorizeResponse
{
	Approved,
	[ResponseCode(Name = "(A001)")]
	MissingKiosk,
	[ResponseCode(Name = "(A002)")]
	CustomerDisabled,
	[ResponseCode(Name = "(A003)")]
	RentalLimitExceeded,
	[ResponseCode(Name = "(A004)")]
	CreditCardError,
	[ResponseCode(Name = "(A005)")]
	AvsFailure,
	[ResponseCode(Name = "(A006)")]
	Declined,
	[ResponseCode(Name = "(D001)")]
	CodeNotValid,
	[ResponseCode(Name = "(D003)")]
	CodeAlreadyUsed,
	[ResponseCode(Name = "(D002)")]
	CodeNotValidAtThisLocation,
	[ResponseCode(Name = "(A008)")]
	ProcesorError,
	[ResponseCode(Name = "(U001)")]
	[ResponseCode(Name = "(A009)")]
	Error,
	[ResponseCode(Name = "(D004)")]
	NewCustomerOnly,
	[ResponseCode(Name = "(A010)")]
	GameLimitExceeded,
	[ResponseCode(Name = "(A014)")]
	CheckIfCustomerWantsToUseCredits,
	[ResponseCode(Name = "(GCA001)")]
	GCInsufficientBalance,
	[ResponseCode(Name = "(GCA002)")]
	GCRentalLimitExceeded,
	[ResponseCode(Name = "(GCA003)")]
	GCDeclined
}
