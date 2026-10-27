using Redbox.Rental.Model;

namespace Redbox.KioskEngine.PreferenceManager.KioskClientServiceClient.TestData;

public class Error : IError
{
	public string Code { get; set; }

	public string Message { get; set; }
}
