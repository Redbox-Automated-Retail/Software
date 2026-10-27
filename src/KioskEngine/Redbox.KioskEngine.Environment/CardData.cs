using System.Collections.Generic;

namespace Redbox.KioskEngine.Environment;

internal class CardData
{
	public string CardSource { get; set; }

	public int FallbackStatusAction { get; set; }

	public bool EncryptedFlag { get; set; }

	public string Name { get; set; }

	public int NameLength { get; set; }

	public string LanguageCode { get; set; }

	public virtual bool HasChip { get; }

	public string ServiceCode { get; set; }

	public string Expiry { get; set; }

	public string Mod10CheckFlag { get; set; }

	public int PANLength { get; set; }

	public string LastFour { get; set; }

	public string FirstSix { get; set; }

	public int? FallbackReason { get; set; }

	public string ErrorCode { get; set; }

	public string EncFormat { get; set; }

	public string KSN { get; set; }

	public int ICEncDataLength { get; set; }

	public string ICEncData { get; set; }

	public int AESPANLength { get; set; }

	public string AESPAN { get; set; }

	public int LSEncDataLength { get; set; }

	public string LSEncData { get; set; }

	public string ExtLangCode { get; set; }

	public string MfgSerialNumber { get; set; }

	public string InjectedSerialNumber { get; set; }

	public string Track1 { get; set; }

	public string Track2 { get; set; }

	public string VasData { get; set; }

	public Dictionary<string, string> Tags { get; set; }
}
