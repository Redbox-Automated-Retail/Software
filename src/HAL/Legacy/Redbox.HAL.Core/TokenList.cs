using System;
using System.Collections.Generic;

namespace Redbox.HAL.Core;

[Serializable]
public class TokenList : List<Token>, ICloneable<TokenList>
{
	public TokenList()
	{
	}

	public TokenList(IEnumerable<Token> tokens)
		: base(tokens)
	{
	}

	public Token GetLabel()
	{
		return Find((Token each) => each.Type == TokenType.Label);
	}

	public Token GetMnemonic()
	{
		return Find((Token each) => each.Type == TokenType.Mnemonic);
	}

	public bool HasOnlyLabel()
	{
		if (base.Count > 0)
		{
			return FindAll((Token each) => each.Type == TokenType.Label).Count == base.Count;
		}
		return false;
	}

	public bool HasOnlyComments()
	{
		if (base.Count > 0)
		{
			return FindAll((Token each) => each.Type == TokenType.Comment).Count == base.Count;
		}
		return false;
	}

	public TokenList GetSymbols()
	{
		return new TokenList(FindAll((Token each) => (each.Type == TokenType.Symbol || each.Type == TokenType.ConstSymbol) && !each.Value.Equals("TRUE") && !each.Value.Equals("FALSE")));
	}

	public TokenList GetSymbolsAndLiterals()
	{
		return new TokenList(FindAll((Token each) => each.Type == TokenType.Symbol || each.Type == TokenType.ConstSymbol || each.Type == TokenType.StringLiteral || each.Type == TokenType.NumericLiteral));
	}

	public TokenList GetTokensAfterMnemonic()
	{
		int num = FindIndex((Token each) => each.Type == TokenType.Mnemonic);
		if (num != -1)
		{
			TokenList tokenList = new TokenList();
			for (int num2 = num + 1; num2 < base.Count; num2++)
			{
				tokenList.Add(base[num2]);
			}
			return tokenList;
		}
		return new TokenList();
	}

	public TokenList GetAllLiterals()
	{
		return new TokenList(FindAll((Token each) => each.Type == TokenType.StringLiteral || each.Type == TokenType.NumericLiteral));
	}

	public TokenList GetKeyValuePairs()
	{
		return new TokenList(FindAll((Token each) => each.IsKeyValuePair));
	}

	public KeyValuePair GetKeyValuePair(string key)
	{
		Token token = Find((Token each) => each.IsKeyValuePair && string.Compare(((KeyValuePair)each.ConvertValue()).Key, key, ignoreCase: true) == 0);
		if (token != null)
		{
			return (KeyValuePair)token.ConvertValue();
		}
		return null;
	}

	public TokenList GetNumericLiterals()
	{
		return new TokenList(FindAll((Token each) => each.Type == TokenType.NumericLiteral));
	}

	public TokenList GetStringLiterals()
	{
		return new TokenList(FindAll((Token each) => each.Type == TokenType.StringLiteral));
	}

	public TokenList Clone(params object[] parms)
	{
		TokenList tokenList = new TokenList();
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			Token current = enumerator.Current;
			tokenList.Add(current.Clone());
		}
		return tokenList;
	}
}
