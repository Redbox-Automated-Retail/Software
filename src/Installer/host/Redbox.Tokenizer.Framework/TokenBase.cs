using System;

namespace Redbox.Tokenizer.Framework;

[Serializable]
internal class TokenBase<T>
{
	public string Value { get; private set; }

	public T Type { get; private set; }

	public TokenBase(T type, string value)
	{
		Type = type;
		Value = value;
	}

	public override string ToString()
	{
		return $"Type={Type}, Value={Value}";
	}

	public override int GetHashCode()
	{
		return ToString().GetHashCode();
	}
}
