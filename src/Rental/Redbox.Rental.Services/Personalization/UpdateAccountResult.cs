using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.Personalization;

namespace Redbox.Rental.Services.Personalization;

public class UpdateAccountResult : IUpdateAccountResult
{
	public bool Success { get; set; }

	public string CustomerProfileNumber { get; set; }

	public string MobilePhoneNumber { get; set; }

	public string TempPassword { get; set; }

	public UpdateAccountResponseErrors UpdateAccountResponseError { get; set; }

	public string ErrorMessage { get; set; }

	public ErrorList RemoteServiceErrors { get; set; }
}
