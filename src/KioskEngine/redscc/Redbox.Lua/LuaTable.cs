using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Text;
using Redbox.Core;

namespace Redbox.Lua;

public class LuaTable : LuaObject
{
	private readonly IDictionary<object, object> m_pairs = new Dictionary<object, object>();

	public object this[object key]
	{
		get
		{
			if (base.Reference.HasValue)
			{
				int count = base.VirtualMachineInstance.Stack.Count;
				base.VirtualMachineInstance.Stack.GetReference(base.Reference.Value);
				base.VirtualMachineInstance.Stack.Push(key);
				base.VirtualMachineInstance.Stack.GetTable();
				object top = base.VirtualMachineInstance.Stack.Top;
				base.VirtualMachineInstance.Stack.SetTop(count);
				return top;
			}
			if (!m_pairs.ContainsKey(key))
			{
				return null;
			}
			return m_pairs[key];
		}
		set
		{
			if (base.Reference.HasValue)
			{
				int count = base.VirtualMachineInstance.Stack.Count;
				base.VirtualMachineInstance.Stack.GetReference(base.Reference.Value);
				base.VirtualMachineInstance.Stack.Push(key);
				base.VirtualMachineInstance.Stack.Push(value);
				base.VirtualMachineInstance.Stack.SetTable();
				base.VirtualMachineInstance.Stack.SetTop(count);
			}
			else
			{
				m_pairs[key] = value;
			}
		}
	}

	public ICollection Keys
	{
		get
		{
			if (base.Reference.HasValue)
			{
				return GetTableDictionary().Keys;
			}
			List<object> list = new List<object>();
			m_pairs.Keys.ForEach(list.Add);
			return list;
		}
	}

	public ICollection Values
	{
		get
		{
			if (base.Reference.HasValue)
			{
				return GetTableDictionary().Values;
			}
			List<object> list = new List<object>();
			m_pairs.Values.ForEach(list.Add);
			return list;
		}
	}

	public LuaTable(LuaVirtualMachine virtualMachine)
		: base(virtualMachine, null)
	{
	}

	public LuaTable(LuaVirtualMachine virtualMachine, int reference)
		: base(virtualMachine, reference)
	{
	}

	public override void Push()
	{
		if (base.Reference.HasValue)
		{
			base.Push();
			return;
		}
		base.VirtualMachineInstance.Stack.NewTable();
		foreach (KeyValuePair<object, object> pair in m_pairs)
		{
			base.VirtualMachineInstance.Stack.Push(pair.Key);
			base.VirtualMachineInstance.Stack.Push(pair.Value);
			base.VirtualMachineInstance.Stack.SetTable();
		}
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		FormatTable(stringBuilder, new List<LuaTable>());
		return stringBuilder.ToString();
	}

	public string ToJson()
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		BuildPropertiesDictionary(dictionary, this);
		return dictionary.ToJson();
	}

	public void FormatTable(StringBuilder builder, List<LuaTable> visited)
	{
		if (visited.Contains(this))
		{
			return;
		}
		visited.Add(this);
		builder.Append("{ ");
		bool flag = false;
		foreach (object key in Keys)
		{
			if (this[key] is LuaTable luaTable)
			{
				if (!visited.Contains(luaTable))
				{
					if (flag)
					{
						builder.Append(", ");
					}
					builder.Append(FormatKey(key));
					luaTable.FormatTable(builder, visited);
				}
			}
			else
			{
				if (flag)
				{
					builder.Append(", ");
				}
				builder.AppendFormat("{0}{1}", FormatKey(key), LuaHelper.FormatLuaValue(this[key]));
			}
			flag = true;
		}
		builder.Append(" }");
	}

	public Color? ToColor()
	{
		int red = Convert.ToInt32(this["r"] ?? ((object)0));
		int green = Convert.ToInt32(this["g"] ?? ((object)0));
		int blue = Convert.ToInt32(this["b"] ?? ((object)0));
		return Color.FromArgb(red, green, blue);
	}

	public Point? ToPoint()
	{
		int x = Convert.ToInt32(this["x"] ?? ((object)0));
		int y = Convert.ToInt32(this["y"] ?? ((object)0));
		return new Point(x, y);
	}

	public Rectangle? ToRectangle()
	{
		int x = Convert.ToInt32(this["x"] ?? ((object)0));
		int y = Convert.ToInt32(this["y"] ?? ((object)0));
		int width = Convert.ToInt32(this["width"] ?? ((object)0));
		int height = Convert.ToInt32(this["height"] ?? ((object)0));
		return new Rectangle(x, y, width, height);
	}

	public List<T> ToList<T>()
	{
		Type typeFromHandle = typeof(T);
		List<T> list = new List<T>();
		foreach (object value in Values)
		{
			list.Add((T)ConversionHelper.ChangeType(value, typeFromHandle));
		}
		return list;
	}

	public DateTime? ToDateTime()
	{
		string s = string.Format("{0}/{1}/{2} {3}:{4}:{5}", this["month"], this["day"], this["year"], this["hour"], this["min"], this["sec"]);
		if (DateTime.TryParse(s, out var result))
		{
			return result;
		}
		return null;
	}

	public void ImportDictionary(IDictionary<string, object> properties)
	{
		BuildLuaTable(this, properties);
	}

	private static void BuildPropertiesDictionary(IDictionary<string, object> properties, LuaTable table)
	{
		foreach (object key2 in table.Keys)
		{
			string key = key2.ToString();
			object obj = table[key2];
			if (obj is LuaTable table2)
			{
				BuildPropertiesDictionary(properties, table2);
			}
			else
			{
				properties[key] = obj;
			}
		}
	}

	private static void BuildLuaTable(LuaTable table, IEnumerable<KeyValuePair<string, object>> properties)
	{
		foreach (KeyValuePair<string, object> property in properties)
		{
			if (property.Value is Dictionary<string, object> properties2)
			{
				BuildLuaTable(table, properties2);
			}
			else
			{
				table[property.Key] = property.Value;
			}
		}
	}

	private static string FormatKey(object eachKey)
	{
		if (eachKey is double)
		{
			return $"[{eachKey}]=";
		}
		if (eachKey is string)
		{
			string symbol = (string)eachKey;
			if (IsValidSymbol(symbol))
			{
				return $"{eachKey}=";
			}
			return $"['{eachKey}']=";
		}
		if (eachKey is LuaTable)
		{
			return $"[{LuaHelper.FormatLuaValue(eachKey)}]=";
		}
		return null;
	}

	private static bool IsValidSymbol(string symbol)
	{
		if (string.IsNullOrEmpty(symbol))
		{
			return false;
		}
		if (!char.IsLetter(symbol[0]) && symbol[0] != '_')
		{
			return false;
		}
		return !symbol.Contains(" ");
	}

	private ListDictionary GetTableDictionary()
	{
		ListDictionary listDictionary = new ListDictionary();
		int count = base.VirtualMachineInstance.Stack.Count;
		Push();
		base.VirtualMachineInstance.Stack.Push(null);
		while (base.VirtualMachineInstance.Stack.NextKey() != 0)
		{
			object value = base.VirtualMachineInstance.Stack.Pop();
			object top = base.VirtualMachineInstance.Stack.Top;
			listDictionary[top] = value;
		}
		base.VirtualMachineInstance.Stack.SetTop(count);
		return listDictionary;
	}
}
