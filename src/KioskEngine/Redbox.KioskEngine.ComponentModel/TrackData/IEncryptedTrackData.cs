namespace Redbox.KioskEngine.ComponentModel.TrackData;

public interface IEncryptedTrackData : ITrackData
{
	int PANLength { get; set; }

	string ExtLangCode { get; set; }

	string LSEncData { get; set; }

	int LSEncDataLength { get; set; }

	string AESPAN { get; set; }

	int AESPANLength { get; set; }

	string ICEncData { get; set; }

	int ICEncDataLength { get; set; }

	string KSN { get; set; }

	string MfgSerialNumber { get; set; }

	string EncFormat { get; set; }

	string Name { get; set; }

	int NameLength { get; set; }

	string LanguageCode { get; set; }

	string ServiceCode { get; set; }

	string Expiry { get; set; }

	string Mod10CheckFlag { get; set; }

	bool EncryptedFlag { get; set; }

	string InjectedSerialNumber { get; set; }
}
