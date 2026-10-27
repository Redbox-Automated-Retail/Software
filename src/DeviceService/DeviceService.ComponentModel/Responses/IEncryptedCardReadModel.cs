namespace DeviceService.ComponentModel.Responses;

public interface IEncryptedCardReadModel
{
	string FirstSix { get; set; }

	string LastFour { get; set; }

	int PANLength { get; set; }

	string Mod10CheckFlag { get; set; }

	string Expiry { get; set; }

	string ServiceCode { get; set; }

	string LanguageCode { get; set; }

	int NameLength { get; set; }

	string Name { get; set; }

	bool EncryptedFlag { get; set; }

	string EncFormat { get; set; }

	string KSN { get; set; }

	int ICEncDataLength { get; set; }

	string ICEncData { get; set; }

	int AESPANLength { get; set; }

	string AESPAN { get; set; }

	int LSEncDataLength { get; set; }

	string LSEncData { get; set; }

	string ExtLangCode { get; set; }

	string MfgSerialNumber { get; set; }

	string InjectedSerialNumber { get; set; }
}
