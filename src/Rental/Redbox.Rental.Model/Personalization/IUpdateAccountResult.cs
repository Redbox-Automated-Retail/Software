using Redbox.KioskEngine.ComponentModel;

namespace Redbox.Rental.Model.Personalization;

public interface IUpdateAccountResult
{
	bool Success { get; set; }

	string CustomerProfileNumber { get; set; }

	string MobilePhoneNumber { get; set; }

	string TempPassword { get; set; }

	UpdateAccountResponseErrors UpdateAccountResponseError { get; set; }

	string ErrorMessage { get; set; }

	ErrorList RemoteServiceErrors { get; set; }
}
