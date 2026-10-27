using KioskClientService.API.App;

namespace KioskClientService.API.Services.ProxyApi;

public class KioskCustomer
{
	public string CPN { get; set; }

	public string FirstName { get; set; }

	public string LoginEmail { get; set; }

	public string MobilePhoneNumber { get; set; }

	public bool IsMobilePhoneNumberVerified { get; set; }

	public bool? IsEmailVerified { get; set; }

	public bool? PromptForPerks { get; set; }

	public bool? PromptForEarlyId { get; set; }

	public bool? TextClubEnabled { get; set; }

	public void ScrubData()
	{
		FirstName = ObfuscationHelper.ObfuscateStringData(FirstName, 1, 0);
		LoginEmail = ObfuscationHelper.ObfuscateStringData(LoginEmail, 1, 0);
		MobilePhoneNumber = ObfuscationHelper.ObfuscateNumericData(MobilePhoneNumber, 1, 0);
	}
}
