using Redbox.Core;

namespace Redbox.Rental.Model.KioskClientService.CustomerProfile;

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

	public virtual void ScrubData()
	{
		IObfuscationService obfuscationService = ServiceLocator.Instance.GetService<IObfuscationService>();
		FirstName = obfuscationService?.ObfuscateStringData(FirstName, 1, 0);
		LoginEmail = obfuscationService?.ObfuscateStringData(LoginEmail, 1, 0);
		MobilePhoneNumber = obfuscationService?.ObfuscateNumericData(MobilePhoneNumber, 1, 0);
	}
}
