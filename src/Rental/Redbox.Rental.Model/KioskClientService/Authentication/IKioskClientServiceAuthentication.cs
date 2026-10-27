namespace Redbox.Rental.Model.KioskClientService.Authentication;

public interface IKioskClientServiceAuthentication
{
	AuthenticateResponse Authenticate(AuthenticateRequest request);
}
