using DeviceService.ComponentModel;

namespace Redbox.Rental.Model.KioskClientService.CustomerProfile;

public interface ICustomerProfileLoginRequest : IMessageScrub
{
	long KioskId { get; set; }

	string ActivityId { get; set; }

	string EmailAddress { get; set; }

	string Password { get; set; }

	string PhoneNumber { get; set; }

	bool IsEncrypted { get; set; }

	string Pin { get; set; }

	string CustomerProfileNumber { get; set; }

	string LanguageCode { get; set; }
}
