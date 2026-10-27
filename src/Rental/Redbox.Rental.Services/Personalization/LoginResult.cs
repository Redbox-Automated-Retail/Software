using Redbox.Rental.Model;
using Redbox.Rental.Model.Personalization;

namespace Redbox.Rental.Services.Personalization;

public class LoginResult : BaseResponse, ILoginResult, IBaseResponse
{
	public IPersonalizationSession PersonalizationSession { get; set; }

	public KioskLoginResponseErrors KioskLoginResponseError { get; set; }

	public string ErrorMessage { get; set; }
}
