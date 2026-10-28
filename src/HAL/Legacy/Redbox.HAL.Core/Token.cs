using System;

namespace Redbox.HAL.Core;

[Serializable]
public class Token : ICloneable<Token>
{
	public bool IsSymbolOrConst
	{
		get
		{
			if (Type != TokenType.Symbol)
			{
				return Type == TokenType.ConstSymbol;
			}
			return true;
		}
	}

	public string Value { get; private set; }

	public TokenType Type { get; private set; }

	public bool IsKeyValuePair { get; private set; }

	public string PairSeparator { get; private set; }

	public Token(TokenType type, string value, string pairSeparator)
		: this(type, value, isKeyValuePair: true)
	{
		PairSeparator = pairSeparator;
	}

	public Token(TokenType type, string value, bool isKeyValuePair)
	{
		Type = type;
		Value = value;
		IsKeyValuePair = isKeyValuePair;
		PairSeparator = "=";
	}

	public override bool Equals(object obj)
	{
		if (object.ReferenceEquals(this, obj))
		{
			return true;
		}
		if (obj is Token token)
		{
			if (Value == token.Value && Type == token.Type)
			{
				return IsKeyValuePair == token.IsKeyValuePair;
			}
			return false;
		}
		return false;
	}

	public override string ToString()
	{
		return $"Type={Type}, Value={Value}, IsKeyValuePair={IsKeyValuePair}";
	}

	public override int GetHashCode()
	{
		return ToString().GetHashCode();
	}

	public object ConvertValue()
	{
		if (IsKeyValuePair)
		{
			return new KeyValuePair(Value, Type, PairSeparator);
		}
		if (Type == TokenType.NumericLiteral)
		{
			if (Value.IndexOf(".") != -1)
			{
				return decimal.Parse(Value);
			}
			return int.Parse(Value);
		}
		if (Type == TokenType.Operator && Value.IndexOf("..") != -1)
		{
			return new Range(Value);
		}
		if (string.Compare(Value, bool.TrueString, ignoreCase: true) == 0)
		{
			return true;
		}
		if (string.Compare(Value, bool.FalseString, ignoreCase: true) == 0)
		{
			return false;
		}
		return Value;
	}

	public Token Clone(params object[] parms)
	{
		Token token = new Token(Type, Value, IsKeyValuePair);
		token.PairSeparator = PairSeparator;
		return token;
	}
}
