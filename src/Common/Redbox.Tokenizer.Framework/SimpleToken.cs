using System;
using Redbox.Core;

namespace Redbox.Tokenizer.Framework;

[Serializable]
public class SimpleToken : TokenBase<SimpleTokenType>, ICloneable<SimpleToken>
{
	public bool IsKeyValuePair { get; private set; }

	public string PairSeparator { get; private set; }

	public SimpleToken(SimpleTokenType type, string value, string pairSeparator)
		: this(type, value, isKeyValuePair: true)
	{
		PairSeparator = pairSeparator;
	}

	public SimpleToken(SimpleTokenType type, string value, bool isKeyValuePair)
		: base(type, value)
	{
		IsKeyValuePair = isKeyValuePair;
		PairSeparator = "=";
	}

	public override bool Equals(object obj)
	{
		if (this == obj)
		{
			return true;
		}
		if (obj is SimpleToken simpleToken)
		{
			if (base.Value == simpleToken.Value && base.Type == simpleToken.Type)
			{
				return IsKeyValuePair == simpleToken.IsKeyValuePair;
			}
			return false;
		}
		return false;
	}

	public override string ToString()
	{
		return $"Type={base.Type}, Value={base.Value}, IsKeyValuePair={IsKeyValuePair}";
	}

	public override int GetHashCode()
	{
		return ToString().GetHashCode();
	}

	public object ConvertValue()
	{
		if (IsKeyValuePair)
		{
			return new KeyValuePair(base.Value, base.Type, PairSeparator);
		}
		if (base.Type == SimpleTokenType.NumericLiteral)
		{
			if (base.Value.IndexOf(".") != -1)
			{
				return decimal.Parse(base.Value);
			}
			return int.Parse(base.Value);
		}
		if (base.Type == SimpleTokenType.Operator && base.Value.IndexOf("..") != -1)
		{
			return new Range(base.Value);
		}
		if (string.Compare(base.Value, bool.TrueString, ignoreCase: true) == 0)
		{
			return true;
		}
		if (string.Compare(base.Value, bool.FalseString, ignoreCase: true) == 0)
		{
			return false;
		}
		return base.Value;
	}

	public SimpleToken Clone(params object[] parms)
	{
		return new SimpleToken(base.Type, base.Value, IsKeyValuePair)
		{
			PairSeparator = PairSeparator
		};
	}
}
