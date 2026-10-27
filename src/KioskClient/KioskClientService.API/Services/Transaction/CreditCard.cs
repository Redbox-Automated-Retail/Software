using System.Collections.Generic;
using KioskClientService.API.App;

namespace KioskClientService.API.Services.Transaction;

public class CreditCard
{
	public string FirstName { get; set; }

	public string LastName { get; set; }

	public string Number { get; set; }

	public string PostalCode { get; set; }

	public string ExpirationMonth { get; set; }

	public string ExpirationYear { get; set; }

	public int? KeyId { get; set; }

	public string CardId { get; set; }

	public byte CardType { get; set; }

	public string Track2 { get; set; }

	public bool IsEncrypted { get; set; }

	public string BIN { get; set; }

	public string LastFour { get; set; }

	public string KSN { get; set; }

	public string ICEncryptedData { get; set; }

	public string ReaderSerialNumber { get; set; }

	public CardSourceTypeEnum? CardSourceType { get; set; }

	public bool EmvEnabledTerminal { get; set; }

	public bool ContactlessEnabled { get; set; }

	public byte? FallbackType { get; set; }

	public bool IsTechnicalFallback => FallbackType.HasValue;

	public IDictionary<string, string> EmvTags { get; set; }

	public void ScrubData()
	{
		CardId = null;
		Track2 = null;
		Number = null;
		FirstName = ObfuscationHelper.ObfuscateStringData(FirstName, 1, 0);
		LastName = ObfuscationHelper.ObfuscateStringData(LastName, 2, 0);
		BIN = ObfuscationHelper.ObfuscateNumericData(BIN, 0, 0);
		KSN = ObfuscationHelper.ObfuscateNumericData(KSN, 0, 0);
		ICEncryptedData = ObfuscationHelper.ObfuscateStringData(ICEncryptedData, 0, 0);
		ReaderSerialNumber = ObfuscationHelper.ObfuscateStringData(ReaderSerialNumber, 0, 0);
		LastFour = ObfuscationHelper.ObfuscateNumericData(LastFour, 0, 1);
		PostalCode = ObfuscationHelper.ObfuscateNumericData(PostalCode, 0, 1);
		ExpirationMonth = ObfuscationHelper.ObfuscateNumericData(ExpirationMonth, 0, 0);
		ExpirationYear = ObfuscationHelper.ObfuscateNumericData(ExpirationYear, 0, 0);
	}
}
