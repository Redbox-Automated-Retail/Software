using System.Collections.Generic;

namespace Redbox.Rental.Model.KioskClientService.Authentication;

public class AuthenticateResponse
{
	public bool Success { get; set; }

	public List<Error> Errors { get; set; }
}
