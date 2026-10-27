using System;
using System.Collections.Specialized;

namespace Redbox.Macros;

public static class StringUtils
{
	public static bool EndsWith(string value, char c)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		int length = value.Length;
		if (length != 0)
		{
			return value[length - 1] == c;
		}
		return false;
	}

	public static string ConvertEmptyToNull(string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			return null;
		}
		return value;
	}

	public static string ConvertNullToEmpty(string value)
	{
		return value ?? string.Empty;
	}

	public static string Join(string separator, StringCollection value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (separator == null)
		{
			separator = string.Empty;
		}
		string[] array = new string[value.Count];
		value.CopyTo(array, 0);
		return string.Join(separator, array);
	}

	public static StringCollection Clone(StringCollection stringCollection)
	{
		string[] array = new string[stringCollection.Count];
		stringCollection.CopyTo(array, 0);
		StringCollection stringCollection2 = new StringCollection();
		stringCollection2.AddRange(array);
		return stringCollection2;
	}
}
