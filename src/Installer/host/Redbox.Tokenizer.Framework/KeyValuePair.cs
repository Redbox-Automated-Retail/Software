using System;

namespace Redbox.Tokenizer.Framework;

[Serializable]
internal class KeyValuePair
{
	public string Key { get; private set; }

	public string Value { get; private set; }

	public SimpleTokenType Type { get; private set; }

	public string PairSeparator { get; private set; }

	public KeyValuePair(string value, SimpleTokenType type)
		: this(value, type, "=")
	{
	}

	public KeyValuePair(string value, SimpleTokenType type, string pairSeparator)
	{
		Type = type;
		PairSeparator = pairSeparator;
		SetParts(value.Split(pairSeparator.ToCharArray(), 2, StringSplitOptions.RemoveEmptyEntries));
	}

	public override string ToString()
	{
		return $"{Key}{PairSeparator}{Value}";
	}

	private void SetParts(string[] parts)
	{
		if (parts.Length == 2)
		{
			Key = parts[0].Trim();
			Value = parts[1].Trim();
		}
	}
}
