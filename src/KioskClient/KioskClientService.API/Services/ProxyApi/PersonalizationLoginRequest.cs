namespace KioskClientService.API.Services.ProxyApi;

public class PersonalizationLoginRequest
{
	public long KioskId { get; set; }

	public string ActivityId { get; set; }

	public string EmailAddress { get; set; }

	public string Password { get; set; }

	public string PhoneNumber { get; set; }

	public string Pin { get; set; }

	public bool IsPhoneAndPinLogin
	{
		get
		{
			if (!string.IsNullOrWhiteSpace(PhoneNumber) && Pin != null)
			{
				return Pin.Length > 0;
			}
			return false;
		}
	}

	public bool IsEmailLogin
	{
		get
		{
			if (!string.IsNullOrWhiteSpace(EmailAddress) && Password != null)
			{
				return Password.Length > 0;
			}
			return false;
		}
	}
}
