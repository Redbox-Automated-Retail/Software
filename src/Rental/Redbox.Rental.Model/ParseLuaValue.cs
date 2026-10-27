using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Redbox.Rental.Model;

public class ParseLuaValue
{
	private int _tokenPos;

	public List<string> _rawTokens = new List<string>();

	private Dictionary<string, object> _properties = new Dictionary<string, object>();

	private bool IsLiteral => Regex.IsMatch(Current, "^[a-zA-Z]+[0-9a-zA-Z_]*");

	private bool IsBool
	{
		get
		{
			if (!Current.Equals("true", StringComparison.CurrentCultureIgnoreCase))
			{
				return Current.Equals("false", StringComparison.CurrentCultureIgnoreCase);
			}
			return true;
		}
	}

	private bool IsNull
	{
		get
		{
			if (!string.IsNullOrEmpty(Current) && !Current.Equals("null", StringComparison.CurrentCultureIgnoreCase))
			{
				return Current.Equals("nil", StringComparison.CurrentCultureIgnoreCase);
			}
			return true;
		}
	}

	private bool IsString => Regex.IsMatch(Current, "^[\"']([^\"]*)[\"']");

	private bool IsNumber => Regex.IsMatch(Current, "^\\d+");

	private bool IsFloat => Regex.IsMatch(Current, "^\\d*\\.\\d+");

	private bool HasMore => _tokenPos < _rawTokens.Count;

	private string Current
	{
		get
		{
			if (!HasMore)
			{
				return null;
			}
			return _rawTokens[_tokenPos];
		}
	}

	public Dictionary<string, object> Properties => _properties;

	public void Parse(string s)
	{
		_rawTokens.Clear();
		s = s.Replace("\\\"", "&quot");
		string text = string.Format("({0}[^{0}]*{0})", "\"");
		string[] array = Regex.Split(s, text + "|(=)|(,)|(\\[)|(\\])|(\\{)|(\\})|(--[^\\n\\r]*)");
		foreach (string text2 in array)
		{
			if (text2.Trim().Length != 0 && !text2.StartsWith("--"))
			{
				_rawTokens.Add(text2.Trim());
			}
		}
		_tokenPos = 0;
		LoadProperties();
	}

	private void LoadProperties()
	{
		_properties.Clear();
		object obj = ReadValue();
		if (obj is Dictionary<string, object>)
		{
			_properties = obj as Dictionary<string, object>;
			return;
		}
		throw new Exception("expecting '{', a string or a number");
	}

	private object ReadValue()
	{
		if (IsNull)
		{
			return ReadNull();
		}
		if (IsToken("{"))
		{
			return ReadObject();
		}
		if (IsString)
		{
			return GetString();
		}
		if (IsFloat)
		{
			return GetFloat();
		}
		if (IsNumber)
		{
			return GetNumber();
		}
		if (IsBool)
		{
			return GetBoolean();
		}
		throw new Exception("expecting '{', a string or a number");
	}

	private object ReadObject()
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		List<string> list = new List<string>();
		Next();
		while (!IsToken("}"))
		{
			if (IsLiteral)
			{
				string current = GetCurrent();
				Next();
				dictionary.Add(current, ReadValue());
			}
			else if (IsToken("["))
			{
				Next();
				string key = GetString();
				if (!IsToken("]"))
				{
					throw new Exception("expecting ']'");
				}
				Next();
				if (!IsToken("="))
				{
					throw new Exception("expecting '='");
				}
				Next();
				dictionary.Add(key, ReadValue());
			}
			else
			{
				list.Add(GetString());
			}
			if (!HasMore || Current == "}")
			{
				break;
			}
			if (!IsToken(","))
			{
				throw new Exception("expecting ','");
			}
			Next();
		}
		Next();
		if (list.Count == 0)
		{
			return dictionary;
		}
		return list;
	}

	private bool IsToken(string s)
	{
		if (!HasMore)
		{
			return false;
		}
		return Current == s;
	}

	private void Next()
	{
		_tokenPos++;
	}

	private string GetCurrent()
	{
		string current = Current;
		Next();
		return current;
	}

	private object ReadNull()
	{
		Next();
		return null;
	}

	private string GetString()
	{
		string current = GetCurrent();
		Match match = Regex.Match(current, "^[\"']([^\"]*)[\"']");
		if (match.Groups[0].Captures.Count == 0)
		{
			return current;
		}
		return match.Groups[1].Captures[0].Value.Replace("&quot", "\"").Replace("\\r\\n", Environment.NewLine);
	}

	private int GetNumber()
	{
		return Convert.ToInt32(GetCurrent());
	}

	private bool GetBoolean()
	{
		return Convert.ToBoolean(GetCurrent());
	}

	private double GetFloat()
	{
		return Convert.ToDouble(GetCurrent());
	}
}
