using Redbox.Rental.Model;

namespace Redbox.Rental.Services.KioskClientService.TestData;

public class Error : IError
{
	public string Code { get; set; }

	public string Message { get; set; }
}
