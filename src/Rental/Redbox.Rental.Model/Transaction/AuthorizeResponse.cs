using Redbox.KioskEngine.ComponentModel.KioskServices;

namespace Redbox.Rental.Model.Transaction;

public enum AuthorizeResponse
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
	[ResponseCode(Name = "(A024)")]
	EnforceAccertify,
	[ResponseCode(Name = "(A025)")]
	MissingReservation,
	[ResponseCode(Name = "(A026)")]
	ReservationCancelled,
	[ResponseCode(Name = "(A027)")]
	RopRentalLimitExceeded,
	[ResponseCode(Name = "(A028)")]
	NewCustomerRentalLimitExceeded,
	[ResponseCode(Name = "(A029)")]
	ReservationForOtherCustomer,
	[ResponseCode(Name = "(A030)")]
	CheckoutApiError,
	[ResponseCode(Name = "(A031)")]
	MissingOrExtraSubscriptionLineItem,
	[ResponseCode(Name = "(A032)")]
	CardCPDifferentFromSubCP,
	[ResponseCode(Name = "(A033)")]
	SubscriptionNotProcessedDueToAuthFailure,
	[ResponseCode(Name = "(A034)")]
	InvalidCheckoutApiResponse,
	[ResponseCode(Name = "(A100)")]
	AuthorizeProcessingError,
	[ResponseCode(Name = "(A101)")]
	MerchantServiceConnectionError,
	[ResponseCode(Name = "(A102)")]
	KioskDatConnectionError,
	[ResponseCode(Name = "(A203)")]
	CPServiceConnectionError,
	[ResponseCode(Name = "(A104)")]
	PromoServiceConnectionError,
	[ResponseCode(Name = "(A105)")]
	CheckoutApiConnectionError,
	[ResponseCode(Name = "(TBS004)")]
	BlockedMobileOrEMVContactless,
	[ResponseCode(Name = "(KCS500)")]
	CommunicationError
}
