using System;
using System.Collections.Generic;
using Redbox.Core;

namespace Redbox.Tokenizer.Framework;

[Serializable]
public class SimpleTokenList : List<SimpleToken>, ICloneable<SimpleTokenList>
{
	public SimpleTokenList()
	{
	}

	public SimpleTokenList(IEnumerable<SimpleToken> tokens)
		: base(tokens)
	{
	}

	public SimpleToken GetLabel()
	{
		return Find((SimpleToken each) => each.Type == SimpleTokenType.Label);
	}

	public SimpleToken GetMnemonic()
	{
		return Find((SimpleToken each) => each.Type == SimpleTokenType.Mnemonic);
	}

	public bool HasOnlyLabel()
	{
		if (base.Count > 0)
		{
			return FindAll((SimpleToken each) => each.Type == SimpleTokenType.Label).Count == base.Count;
		}
		return false;
	}

	public bool HasOnlyComments()
	{
		if (base.Count > 0)
		{
			return FindAll((SimpleToken each) => each.Type == SimpleTokenType.Comment).Count == base.Count;
		}
		return false;
	}

	public SimpleTokenList GetSymbols()
	{
		return new SimpleTokenList(FindAll((SimpleToken each) => each.Type == SimpleTokenType.Symbol));
	}

	public SimpleTokenList GetSymbolsAndLiterals()
	{
		return new SimpleTokenList(FindAll((SimpleToken each) => each.Type == SimpleTokenType.Symbol || each.Type == SimpleTokenType.StringLiteral || each.Type == SimpleTokenType.NumericLiteral));
	}

	public SimpleTokenList GetTokensAfterMnemonic()
	{
		int mnemonicIndex = FindIndex((SimpleToken each) => each.Type == SimpleTokenType.Mnemonic);
		if (mnemonicIndex != -1)
		{
			SimpleTokenList simpleTokenList = new SimpleTokenList();
			for (int i = mnemonicIndex + 1; i < base.Count; i++)
			{
				simpleTokenList.Add(base[i]);
			}
			return simpleTokenList;
		}
		return new SimpleTokenList();
	}

	public SimpleTokenList GetAllLiterals()
	{
		return new SimpleTokenList(FindAll((SimpleToken each) => each.Type == SimpleTokenType.StringLiteral || each.Type == SimpleTokenType.NumericLiteral));
	}

	public SimpleTokenList GetKeyValuePairs()
	{
		return new SimpleTokenList(FindAll((SimpleToken each) => each.IsKeyValuePair));
	}

	public KeyValuePair GetKeyValuePair(string key)
	{
		SimpleToken simpleToken = Find((SimpleToken each) => each.IsKeyValuePair && string.Compare(((KeyValuePair)each.ConvertValue()).Key, key, ignoreCase: true) == 0);
		if (simpleToken != null)
		{
			return (KeyValuePair)simpleToken.ConvertValue();
		}
		return null;
	}

	public SimpleTokenList GetNumericLiterals()
	{
		return new SimpleTokenList(FindAll((SimpleToken each) => each.Type == SimpleTokenType.NumericLiteral));
	}

	public SimpleTokenList GetStringLiterals()
	{
		return new SimpleTokenList(FindAll((SimpleToken each) => each.Type == SimpleTokenType.StringLiteral));
	}

	public SimpleTokenList Clone(params object[] parms)
	{
		SimpleTokenList simpleTokenList = new SimpleTokenList();
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			SimpleToken current = enumerator.Current;
			simpleTokenList.Add(current.Clone());
		}
		return simpleTokenList;
	}
}
