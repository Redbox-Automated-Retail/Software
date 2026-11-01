using System.Collections.Generic;
using Redbox.Core;
using Redbox.Rental.Model;

namespace Redbox.Rental.Services.KioskClientService.Transactions;

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
		IObfuscationService obfuscationService = ServiceLocator.Instance.GetService<IObfuscationService>();
		FirstName = obfuscationService?.ObfuscateStringData(FirstName, 1, 0);
		LastName = obfuscationService?.ObfuscateStringData(LastName, 2, 0);
		BIN = obfuscationService?.ObfuscateNumericData(BIN, 0, 0);
		KSN = obfuscationService?.ObfuscateNumericData(KSN, 0, 0);
		ICEncryptedData = obfuscationService?.ObfuscateStringData(ICEncryptedData, 0, 0);
		ReaderSerialNumber = obfuscationService?.ObfuscateStringData(ReaderSerialNumber, 0, 0);
		LastFour = obfuscationService?.ObfuscateNumericData(LastFour, 0, 1);
		PostalCode = obfuscationService?.ObfuscateNumericData(PostalCode, 0, 1);
		ExpirationMonth = obfuscationService?.ObfuscateNumericData(ExpirationMonth, 0, 0);
		ExpirationYear = obfuscationService?.ObfuscateNumericData(ExpirationYear, 0, 0);
	}
}
