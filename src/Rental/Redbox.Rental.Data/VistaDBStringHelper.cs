using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Redbox.Rental.Data;

public class VistaDBStringHelper
{
	public class DictionaryList : Dictionary<string, object>
	{
	}

	public static string ToString(object dictionary)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("{ ");
		DictionaryToStrings(dictionary, stringBuilder);
		stringBuilder.Append(" }");
		return stringBuilder.ToString();
	}

	private static void DictionaryToStrings(object obj, StringBuilder sb)
	{
		bool needsBracketsAroundName = obj is DictionaryList;
		if (!(obj is Dictionary<string, object> dictionary))
		{
			return;
		}
		bool addComma = false;
		foreach (KeyValuePair<string, object> item in dictionary)
		{
			sb.Append($"{GetComma(addComma)}{GetOpenBracket(needsBracketsAroundName)}{item.Key}{GetCloseBracket(needsBracketsAroundName)} = ");
			if (item.Value is IDictionary<string, object>)
			{
				sb.Append("{ ");
				DictionaryToStrings(item.Value as Dictionary<string, object>, sb);
				sb.Append(" }");
			}
			else if (item.Value is IList<string>)
			{
				StringBuilder sub = new StringBuilder();
				sub.Append("{ ");
				int pos = 0;
				(item.Value as IList<string>).ToList().ForEach(delegate(string s)
				{
					sub.Append($"{GetComma(pos != 0)}\"{s}\"");
					pos++;
				});
				sub.Append(" }");
				sb.Append(sub.ToString());
			}
			else if (item.Value is string)
			{
				sb.Append(string.Format("\"{0}\"", string.IsNullOrEmpty(item.Value as string) ? "null" : item.Value));
			}
			else
			{
				sb.Append(string.Format("{0}", item.Value ?? "null"));
			}
			addComma = true;
		}
	}

	private static object GetOpenBracket(bool needsBracketsAroundName)
	{
		if (!needsBracketsAroundName)
		{
			return null;
		}
		return "[";
	}

	private static object GetCloseBracket(bool needsBracketsAroundName)
	{
		if (!needsBracketsAroundName)
		{
			return null;
		}
		return "]";
	}

	private static string GetComma(bool addComma)
	{
		if (!addComma)
		{
			return null;
		}
		return ", ";
	}
}
