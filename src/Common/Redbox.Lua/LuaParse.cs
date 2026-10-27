using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Redbox.Lua;

public class LuaParse
{
	private readonly List<string> m_tokens = new List<string>();

	public string Id { get; set; }

	public LuaParseObject Value { get; set; }

	protected bool IsLiteral => Regex.IsMatch(m_tokens[0], "^[a-zA-Z]+[0-9a-zA-Z_]*");

	protected bool IsString => Regex.IsMatch(m_tokens[0], "^[\"']([^\"]*)[\"']");

	protected bool IsNumber => Regex.IsMatch(m_tokens[0], "^\\d+");

	protected bool IsFloat => Regex.IsMatch(m_tokens[0], "^\\d*\\.\\d+");

	public void Parse(string s, bool allowAnonymous)
	{
		string text = string.Format("({0}[^{0}]*{0})", "\"");
		string[] array = Regex.Split(s, text + "|(=)|(,)|(\\[)|(\\])|(\\{)|(\\})|(--[^\\n\\r]*)");
		foreach (string text2 in array)
		{
			if (text2.Trim().Length != 0 && !text2.StartsWith("--"))
			{
				m_tokens.Add(text2.Trim());
			}
		}
		Assign(!allowAnonymous);
	}

	protected void Assign(bool requiresIdentifier)
	{
		if (requiresIdentifier)
		{
			if (!IsLiteral)
			{
				throw new Exception("expect identifier");
			}
			Id = GetToken();
			if (!IsToken("="))
			{
				throw new Exception("expect '='");
			}
			NextToken();
		}
		Value = RVal();
	}

	protected LuaParseObject RVal()
	{
		if (IsToken("{"))
		{
			return LuaObject();
		}
		if (IsString)
		{
			return GetString();
		}
		if (IsNumber)
		{
			return GetNumber();
		}
		if (IsFloat)
		{
			return GetFloat();
		}
		throw new Exception("expecting '{', a string or a number");
	}

	protected LuaParseObject LuaObject()
	{
		Dictionary<string, LuaParseObject> dictionary = new Dictionary<string, LuaParseObject>();
		NextToken();
		while (!IsToken("}"))
		{
			if (IsLiteral)
			{
				string token = GetToken();
				NextToken();
				dictionary.Add(token, RVal());
			}
			else if (IsToken("["))
			{
				NextToken();
				string key = GetString();
				if (!IsToken("]"))
				{
					throw new Exception("expecting ']'");
				}
				NextToken();
				if (!IsToken("="))
				{
					throw new Exception("expecting '='");
				}
				NextToken();
				dictionary.Add(key, RVal());
			}
			else
			{
				dictionary.Add(dictionary.Count.ToString(), RVal());
			}
			if (!HasMoreTokens() || PeekToken() == "}")
			{
				break;
			}
			if (!IsToken(","))
			{
				throw new Exception("expecting ','");
			}
			NextToken();
		}
		NextToken();
		return dictionary;
	}

	protected string GetToken()
	{
		string result = m_tokens[0];
		m_tokens.RemoveAt(0);
		return result;
	}

	protected LuaParseObject GetString()
	{
		string value = Regex.Match(m_tokens[0], "^[\"']([^\"]*)[\"']").Groups[1].Captures[0].Value;
		m_tokens.RemoveAt(0);
		return value;
	}

	protected LuaParseObject GetNumber()
	{
		int num = Convert.ToInt32(m_tokens[0]);
		m_tokens.RemoveAt(0);
		return num;
	}

	protected LuaParseObject GetFloat()
	{
		double num = Convert.ToDouble(m_tokens[0]);
		m_tokens.RemoveAt(0);
		return num;
	}

	protected void NextToken()
	{
		if (m_tokens.Count != 0)
		{
			m_tokens.RemoveAt(0);
		}
	}

	protected string PeekToken()
	{
		if (m_tokens.Count <= 0)
		{
			return null;
		}
		return m_tokens[0];
	}

	protected bool IsToken(string s)
	{
		if (m_tokens.Count <= 0)
		{
			return false;
		}
		return m_tokens[0] == s;
	}

	protected bool HasMoreTokens()
	{
		return m_tokens.Count > 0;
	}
}
