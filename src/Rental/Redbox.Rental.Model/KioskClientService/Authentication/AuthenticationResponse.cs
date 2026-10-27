using Redbox.KioskEngine.ComponentModel.KioskServices;

namespace Redbox.Rental.Model.KioskClientService.Authentication;

public enum AuthenticationResponse
{
	Valid,
	[ResponseCode(Name = "(B001)")]
	MissingKiosk,
	[ResponseCode(Name = "(B002)")]
	MissingUser,
	[ResponseCode(Name = "(B003)")]
	MarketAccessDenied,
	[ResponseCode(Name = "(B004)")]
	AuthenticationFailed,
	[ResponseCode(Name = "(U001)")]
	Error,
	AccountLocked
}
