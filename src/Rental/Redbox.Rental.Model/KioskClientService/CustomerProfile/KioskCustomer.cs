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
		IObfuscationService service = ServiceLocator.Instance.GetService<IObfuscationService>();
		FirstName = service?.ObfuscateStringData(FirstName, 1, 0);
		LoginEmail = service?.ObfuscateStringData(LoginEmail, 1, 0);
		MobilePhoneNumber = service?.ObfuscateNumericData(MobilePhoneNumber, 1, 0);
	}
}
