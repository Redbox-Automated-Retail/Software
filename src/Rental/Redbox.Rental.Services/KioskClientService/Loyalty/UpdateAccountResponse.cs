using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.App;

namespace Redbox.Rental.Services.KioskClientService.Loyalty;

internal class UpdateAccountResponse : IMessageScrub
{
	public string Error { get; set; }

	public string CustomerProfileNumber { get; set; }

	public string MobilePhoneNumber { get; set; }

	public string TempPassword { get; set; }

	public object Scrub()
	{
		ObfuscationHelper obfuscationHelper = new ObfuscationHelper();
		return new
		{
			CustomerProfileNumber = obfuscationHelper.ObfuscateStringData(CustomerProfileNumber, 1, 1),
			MobilePhoneNumber = obfuscationHelper.ObfuscateStringData(MobilePhoneNumber, 1, 1),
			TempPassword = obfuscationHelper.ObfuscateStringData(TempPassword, 1, 1),
			Error = Error
		}.ToJson().ToPrettyJson();
	}
}
