using System.Collections.Generic;

namespace DeviceService.ComponentModel.Responses;

public class EncryptedCardReadModel : Base87CardReadModel, IEncryptedCardReadModel
{
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

	public EncryptedCardReadModel()
	{
	}

	public EncryptedCardReadModel(IEnumerable<Error> errors)
		: base(errors)
	{
	}

	public override void ObfuscateSensitiveData()
	{
		base.ObfuscateSensitiveData();
		KSN = ObfuscationHelper.ObfuscateValue(KSN);
		ICEncData = ObfuscationHelper.ObfuscateValue(ICEncData);
		AESPAN = ObfuscationHelper.ObfuscateValue(AESPAN);
		LSEncData = ObfuscationHelper.ObfuscateValue(LSEncData);
	}
}
