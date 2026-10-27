using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;

namespace Redbox.Rental.Model;

public static class LuaExtensions
{
	public static LuaTable ToLuaTable(this Dictionary<string, object> value)
	{
		IKernelService service = ServiceLocator.Instance.GetService<IKernelService>();
		LuaTable table = (LuaTable)service.NewTable();
		value.ForEach(delegate(KeyValuePair<string, object> row)
		{
			if (row.Value is ICollection)
			{
				if (row.Value is Dictionary<string, object>)
				{
					table[row.Key] = (row.Value as Dictionary<string, object>).ToLuaTable();
				}
				else if (row.Value is Dictionary<long, object>)
				{
					table[row.Key] = (row.Value as Dictionary<long, object>).ToLuaTable();
				}
				else
				{
					table[row.Key] = (row.Value as ICollection).ToLuaTable();
				}
			}
			else if (row.Value is DateTime)
			{
				table[row.Key] = ((DateTime)row.Value).ToString("yyyyMMddHHmmss");
			}
			else
			{
				table[row.Key] = row.Value;
			}
		});
		return table;
	}

	public static LuaTable ToLuaTable(this Dictionary<long, object> value)
	{
		IKernelService service = ServiceLocator.Instance.GetService<IKernelService>();
		LuaTable table = (LuaTable)service.NewTable();
		value.ForEach(delegate(KeyValuePair<long, object> row)
		{
			if (row.Value is ICollection)
			{
				if (row.Value is Dictionary<string, object>)
				{
					table[row.Key] = (row.Value as Dictionary<string, object>).ToLuaTable();
				}
				else if (row.Value is Dictionary<long, object>)
				{
					table[row.Key] = (row.Value as Dictionary<long, object>).ToLuaTable();
				}
				else
				{
					table[row.Key] = (row.Value as ICollection).ToLuaTable();
				}
			}
			else if (row.Value is DateTime)
			{
				table[row.Key] = ((DateTime)row.Value).ToString("yyyyMMddHHmmss");
			}
			else
			{
				table[row.Key] = row.Value;
			}
		});
		return table;
	}

	public static LuaTable ToLuaTable(this ICollection value)
	{
		LuaTable luaTable = (LuaTable)ServiceLocator.Instance.GetService<IKernelService>().NewTable();
		int num = 1;
		foreach (object item in value)
		{
			if (item is ICollection)
			{
				if (item is Dictionary<string, object>)
				{
					luaTable[num++] = (item as Dictionary<string, object>).ToLuaTable();
				}
				else if (item is Dictionary<long, object>)
				{
					luaTable[num++] = (item as Dictionary<long, object>).ToLuaTable();
				}
				else
				{
					luaTable[num++] = (item as ICollection).ToLuaTable();
				}
			}
			else if (item is DateTime)
			{
				luaTable[num++] = ((DateTime)item).ToString("yyyyMMddHHmmss");
			}
			else
			{
				luaTable[num++] = item;
			}
		}
		return luaTable;
	}

	public static LuaTable ToLuaDataTable(this Dictionary<string, object> value)
	{
		LuaTable luaTable = value.ToLuaTable();
		luaTable["keys"] = luaTable.Keys.ToLuaTable();
		return luaTable;
	}

	public static Dictionary<string, object> ParseLuaValue(this string value)
	{
		ParseLuaValue parseLuaValue = new ParseLuaValue();
		parseLuaValue.Parse(value);
		return parseLuaValue.Properties;
	}

	public static string LuaDictToString(this Dictionary<string, object> dict)
	{
		StringBuilder stringBuilder = new StringBuilder();
		DictionaryToStrings(dict, stringBuilder, string.Empty);
		return stringBuilder.ToString();
	}

	private static void DictionaryToStrings(Dictionary<string, object> dict, StringBuilder sb, string spaces)
	{
		foreach (KeyValuePair<string, object> item in dict)
		{
			if (item.Value is IDictionary<string, object>)
			{
				sb.AppendLine($"{spaces}[{item.Key} = {{");
				DictionaryToStrings(item.Value as Dictionary<string, object>, sb, spaces + "    ");
				sb.AppendLine("}]");
			}
			else if (item.Value is IList<string>)
			{
				StringBuilder sub = new StringBuilder();
				sub.Append($"{spaces}[{item.Key} = {{ ");
				int pos = 0;
				(item.Value as IList<string>).ToList().ForEach(delegate(string s)
				{
					if (pos++ == 0)
					{
						sub.Append($"\"{s}\"");
					}
					else
					{
						sub.Append($", \"{s}\"");
					}
				});
				sub.Append(" }]");
				sb.AppendLine(sub.ToString());
			}
			else if (item.Value is string)
			{
				sb.AppendLine(string.Format("{0}[{1} = \"{2}\"]", spaces, item.Key, string.IsNullOrEmpty(item.Value as string) ? "null" : item.Value));
			}
			else
			{
				sb.AppendLine(string.Format("{0}[{1} = {2}]", spaces, item.Key, item.Value ?? "null"));
			}
		}
	}
}
