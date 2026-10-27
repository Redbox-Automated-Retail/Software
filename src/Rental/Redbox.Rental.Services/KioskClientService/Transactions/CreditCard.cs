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
		IObfuscationService service = ServiceLocator.Instance.GetService<IObfuscationService>();
		FirstName = service?.ObfuscateStringData(FirstName, 1, 0);
		LastName = service?.ObfuscateStringData(LastName, 2, 0);
		BIN = service?.ObfuscateNumericData(BIN, 0, 0);
		KSN = service?.ObfuscateNumericData(KSN, 0, 0);
		ICEncryptedData = service?.ObfuscateStringData(ICEncryptedData, 0, 0);
		ReaderSerialNumber = service?.ObfuscateStringData(ReaderSerialNumber, 0, 0);
		LastFour = service?.ObfuscateNumericData(LastFour, 0, 1);
		PostalCode = service?.ObfuscateNumericData(PostalCode, 0, 1);
		ExpirationMonth = service?.ObfuscateNumericData(ExpirationMonth, 0, 0);
		ExpirationYear = service?.ObfuscateNumericData(ExpirationYear, 0, 0);
	}
}
