namespace Redbox.Rental.Model.Personalization;

public interface ILoginResult : IBaseResponse
{
	IPersonalizationSession PersonalizationSession { get; set; }

	KioskLoginResponseErrors KioskLoginResponseError { get; set; }

	string ErrorMessage { get; }
}
