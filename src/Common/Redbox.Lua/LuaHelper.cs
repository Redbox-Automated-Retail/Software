using System.Collections.Generic;
using System.Text;

namespace Redbox.Lua;

public static class LuaHelper
{
	public static string FormatLuaValue(object value)
	{
		if (value == null)
		{
			return "nil";
		}
		if (value is string)
		{
			StringBuilder stringBuilder = new StringBuilder();
			string text = (string)value;
			foreach (char c in text)
			{
				if (char.IsControl(c))
				{
					switch (c)
					{
					case '\r':
						stringBuilder.Append("\\r");
						break;
					case '\n':
						stringBuilder.Append("\\n");
						break;
					case '\t':
						stringBuilder.Append("\\t");
						break;
					}
				}
				else if (char.IsLetterOrDigit(c) || char.IsWhiteSpace(c))
				{
					stringBuilder.Append(c);
				}
				else if (char.IsPunctuation(c))
				{
					if (c == '\'')
					{
						stringBuilder.Append("\\'");
					}
					else
					{
						stringBuilder.Append(c);
					}
				}
				else
				{
					stringBuilder.AppendFormat("\\{0:000}", (byte)c);
				}
			}
			return $"'{stringBuilder}'";
		}
		if (value is LuaTable luaTable)
		{
			StringBuilder stringBuilder2 = new StringBuilder();
			luaTable.FormatTable(stringBuilder2, new List<LuaTable>());
			return stringBuilder2.ToString();
		}
		if (value is LuaFunction)
		{
			return "(function)";
		}
		if (value is bool)
		{
			return value.ToString().ToLower();
		}
		return (value.GetType().ToString() == "Lua511.LuaCSFunction") ? "(kernel function)" : value.ToString();
	}
}
