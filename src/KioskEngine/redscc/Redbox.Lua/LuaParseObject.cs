using System.Collections;
using System.Collections.Generic;

namespace Redbox.Lua;

public class LuaParseObject : IEnumerable
{
	private readonly object m_instance;

	public LuaParseObject this[int index]
	{
		get
		{
			string key = index.ToString();
			if (!(m_instance is Dictionary<string, LuaParseObject> dictionary) || !dictionary.ContainsKey(key))
			{
				return null;
			}
			return dictionary[key];
		}
	}

	public LuaParseObject this[string index]
	{
		get
		{
			if (!(m_instance is Dictionary<string, LuaParseObject> dictionary) || !dictionary.ContainsKey(index))
			{
				return null;
			}
			return dictionary[index];
		}
	}

	public object Instance => m_instance;

	public LuaParseObject(object o)
	{
		m_instance = o;
	}

	public IEnumerator GetEnumerator()
	{
		if (!(m_instance is Dictionary<string, LuaParseObject> dictionary))
		{
			return null;
		}
		return dictionary.GetEnumerator();
	}

	public static implicit operator string(LuaParseObject m)
	{
		return m.m_instance as string;
	}

	public static implicit operator int(LuaParseObject m)
	{
		return (m.m_instance as int?) ?? 0;
	}

	public static implicit operator double(LuaParseObject m)
	{
		return (m.m_instance as double?) ?? 0.0;
	}

	public static implicit operator Dictionary<string, object>(LuaParseObject m)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (m.m_instance is Dictionary<string, LuaParseObject> dictionary2)
		{
			foreach (KeyValuePair<string, LuaParseObject> item in dictionary2)
			{
				dictionary[item.Key] = item.Value;
			}
		}
		return dictionary;
	}

	public static implicit operator LuaParseObject(string s)
	{
		return new LuaParseObject(s);
	}

	public static implicit operator LuaParseObject(int i)
	{
		return new LuaParseObject(i);
	}

	public static implicit operator LuaParseObject(double d)
	{
		return new LuaParseObject(d);
	}

	public static implicit operator LuaParseObject(Dictionary<string, LuaParseObject> dic)
	{
		return new LuaParseObject(dic);
	}
}
