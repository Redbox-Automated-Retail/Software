using System.Collections.Generic;

namespace DeviceService.ComponentModel.Responses;

public class EMVCardReadModel : EncryptedCardReadModel
{
	public bool CardRemoved { get; set; }

	public override bool HasChip => true;

	public IDictionary<string, string> Tags { get; set; }

	public string AID { get; set; }

	public EMVCardReadModel()
	{
		Tags = new Dictionary<string, string>();
	}

	public EMVCardReadModel(IEnumerable<Error> errors)
		: base(errors)
	{
		Tags = new Dictionary<string, string>();
	}

	public override void ObfuscateSensitiveData()
	{
		if (Tags != null)
		{
			foreach (string eachTagEntryKey in new List<string>(Tags.Keys))
			{
				if (TagConstants.Details.Exists((TagDetails x) => x.Tag == eachTagEntryKey && x.IsPii))
				{
					Tags[eachTagEntryKey] = ObfuscationHelper.ObfuscateValue(Tags[eachTagEntryKey]);
				}
			}
		}
		base.ObfuscateSensitiveData();
	}
}
