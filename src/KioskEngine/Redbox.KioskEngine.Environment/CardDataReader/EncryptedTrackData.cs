using Redbox.KioskEngine.ComponentModel.TrackData;

namespace Redbox.KioskEngine.Environment.CardDataReader;

public class EncryptedTrackData : TrackData, IEncryptedTrackData, ITrackData
{
	public int PANLength { get; set; }

	public string ExtLangCode { get; set; }

	public string LSEncData { get; set; }

	public int LSEncDataLength { get; set; }

	public string AESPAN { get; set; }

	public int AESPANLength { get; set; }

	public string ICEncData { get; set; }

	public int ICEncDataLength { get; set; }

	public string KSN { get; set; }

	public string MfgSerialNumber { get; set; }

	public string EncFormat { get; set; }

	public string Name { get; set; }

	public int NameLength { get; set; }

	public string LanguageCode { get; set; }

	public string ServiceCode { get; set; }

	public string Expiry { get; set; }

	public string Mod10CheckFlag { get; set; }

	public bool EncryptedFlag { get; set; }

	public string InjectedSerialNumber { get; set; }

	public override bool HasValidData()
	{
		return base.HasValidData();
	}
}
