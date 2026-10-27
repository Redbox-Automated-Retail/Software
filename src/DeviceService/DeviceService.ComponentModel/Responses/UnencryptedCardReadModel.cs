using System.Collections.Generic;

namespace DeviceService.ComponentModel.Responses;

public class UnencryptedCardReadModel : Base87CardReadModel
{
	public string Track1 { get; set; }

	public string Track2 { get; set; }

	public string Track3 { get; set; }

	public CardType CardType { get; set; }

	public SwipeType SwipeMode { get; set; }

	public new CardSourceType CardSource => CardSourceType.Swipe;

	public UnencryptedCardReadModel()
	{
	}

	public UnencryptedCardReadModel(IEnumerable<Error> errors)
		: base(errors)
	{
	}

	public override void ObfuscateSensitiveData()
	{
		base.ObfuscateSensitiveData();
		Track1 = ObfuscationHelper.ObfuscateValue(Track1);
		Track2 = ObfuscationHelper.ObfuscateValue(Track2);
		Track3 = ObfuscationHelper.ObfuscateValue(Track3);
	}
}
