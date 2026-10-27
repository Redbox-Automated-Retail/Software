using System;
using KioskClientService.API.App;

namespace KioskClientService.API.Models;

public class EmailConfirmationPayload : IMessageScrub
{
	public Guid MessageId { get; set; }

	public long KioskId { get; set; }

	public string Email { get; set; }

	public string AccountNumber { get; set; }

	public DateTime TransactionDate { get; set; }

	public object Scrub()
	{
		return new EmailConfirmationPayload
		{
			MessageId = MessageId,
			KioskId = KioskId,
			Email = ObfuscationHelper.ObfuscateStringData(Email, 2, 2),
			AccountNumber = ObfuscationHelper.ObfuscateStringData(AccountNumber, 3, 0),
			TransactionDate = TransactionDate
		}.ToJsonIndented();
	}
}
