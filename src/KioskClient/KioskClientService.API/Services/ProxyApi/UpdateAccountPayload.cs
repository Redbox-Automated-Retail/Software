using System;
using System.ComponentModel.DataAnnotations;
using KioskClientService.API.App;
using KioskClientService.API.Validation;

namespace KioskClientService.API.Services.ProxyApi;

public class UpdateAccountPayload : IMessageScrub
{
	[NotDefault]
	public Guid MessageId { get; set; }

	[Range(1, int.MaxValue, ErrorMessage = "{0} is not within the valid range of {1} and {2}")]
	public long KioskId { get; set; }

	public string CustomerProfileNumber { get; set; }

	[Required]
	[EmailAddress]
	public string Email { get; set; }

	public string MobilePhoneNumber { get; set; }

	public string Pin { get; set; }

	public bool PunchcardOptin { get; set; }

	public bool TextClubOptin { get; set; }

	public AccountSourceType AccountSourceType { get; set; }

	public bool Replay { get; set; }

	public object Scrub()
	{
		return new
		{
			MessageId = MessageId,
			KioskId = KioskId,
			CustomerProfileNumber = ObfuscationHelper.ObfuscateNumericData(CustomerProfileNumber, 1, 1),
			Email = ObfuscationHelper.ObfuscateStringData(Email, 1, 1),
			MobilePhoneNumber = ObfuscationHelper.ObfuscateNumericData(MobilePhoneNumber, 1, 1),
			Pin = Pin,
			PunchcardOptin = PunchcardOptin,
			TextClubOptin = TextClubOptin,
			AccountSourceType = AccountSourceType,
			Replay = Replay
		}.ToJsonIndented();
	}
}
