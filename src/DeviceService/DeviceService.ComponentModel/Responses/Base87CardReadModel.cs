using System.Collections.Generic;

namespace DeviceService.ComponentModel.Responses;

public abstract class Base87CardReadModel : ResponseBaseModel
{
	public string FirstSix { get; set; }

	public string LastFour { get; set; }

	public int PANLength { get; set; }

	public string Mod10CheckFlag { get; set; }

	public string Expiry { get; set; }

	public string ServiceCode { get; set; }

	public virtual bool HasChip
	{
		get
		{
			if (ServiceCode == null || ServiceCode.Length <= 0)
			{
				return false;
			}
			string text = ServiceCode.Substring(0, 1);
			if (!(text == "2"))
			{
				return text == "6";
			}
			return true;
		}
	}

	public string LanguageCode { get; set; }

	public int NameLength { get; set; }

	public string Name { get; set; }

	public bool EncryptedFlag { get; set; }

	public CardSourceType CardSource { get; set; }

	public FallbackStatusAction FallbackStatusAction { get; set; }

	public FallbackType? FallbackReason { get; set; }

	public string ErrorCode { get; set; }

	public string VasData { get; set; }

	public WalletType WalletFormat { get; set; }

	public bool HasVasData => !string.IsNullOrWhiteSpace(VasData);

	public bool HasPayData => !string.IsNullOrWhiteSpace(FirstSix);

	public string VasErrorCode { get; set; }

	public Base87CardReadModel()
	{
	}

	public Base87CardReadModel(IEnumerable<Error> errors)
		: base(errors)
	{
	}

	public override void ObfuscateSensitiveData()
	{
		base.ObfuscateSensitiveData();
		Name = ObfuscationHelper.ObfuscateValue(Name);
		Expiry = ObfuscationHelper.ObfuscateValue(Expiry);
	}
}
