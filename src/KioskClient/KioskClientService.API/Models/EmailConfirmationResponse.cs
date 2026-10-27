using System;
using KioskClientService.API.App;

namespace KioskClientService.API.Models;

public class EmailConfirmationResponse : BaseResponse
{
	public Guid MessageId { get; set; }

	public ConfirmationStatus ConfirmationStatus { get; set; }

	public string CustomerProfileNumber { get; set; }

	public string MobilePhoneNumber { get; set; }

	public bool PromptForPerks { get; set; }

	public bool PromptForEarlyId { get; set; }

	public bool TextClubEnabled { get; set; }

	public bool? EmailVerified { get; set; }

	public EmailConfirmationResponse()
		: base(200)
	{
	}

	public override object Scrub()
	{
		return new EmailConfirmationResponse
		{
			Success = base.Success,
			Errors = base.Errors,
			ResponseTime = base.ResponseTime,
			StatusCode = base.StatusCode,
			MessageId = MessageId,
			ConfirmationStatus = ConfirmationStatus,
			CustomerProfileNumber = ObfuscationHelper.ObfuscateStringData(CustomerProfileNumber, 1, 1),
			MobilePhoneNumber = ObfuscationHelper.ObfuscateStringData(MobilePhoneNumber, 1, 1),
			PromptForPerks = PromptForPerks,
			PromptForEarlyId = PromptForEarlyId,
			TextClubEnabled = TextClubEnabled,
			EmailVerified = EmailVerified
		}.ToJsonIndented();
	}
}
