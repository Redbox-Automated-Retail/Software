using System;
using Redbox.Rental.Model.KioskClientService.Authentication;

namespace Redbox.Rental.Services.KioskClientService.Authentication;

public class KioskClientServicesAuthentication : BaseKioskClientService, IKioskClientServiceAuthentication
{
	private const string _apiPath = "api/kiosk";

	public AuthenticateResponse Authenticate(AuthenticateRequest request)
	{
		return ExecuteWebRequest<AuthenticateResponse>(new Uri(base.KioskClientServiceBaseUrl + "/api/kiosk/authenticate"), request, "POST", 20000)?.Response;
	}
}
