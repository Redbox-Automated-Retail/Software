namespace Redbox.Rental.Model.KioskClientService.Authentication;

public class AuthenticateRequest
{
	public string Username { get; set; }

	public string Password { get; set; }

	public long KioskId { get; set; }

	public bool UseNTAuthentication { get; set; }
}
