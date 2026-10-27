using System;
using System.Collections.Generic;
using System.Globalization;
using Redbox.Core;

namespace Redbox.Rental.Data;

internal static class DataExtensions
{
	private const string LuaDateTimeFormat = "yyyyMMddHHmmss";

	private const string LuaDateFormat = "yyyyMMdd";

	internal static string ToString(this Dictionary<string, object> data, string key, string defaultValue)
	{
		if (!data.TryGetValue(key, out var value))
		{
			return defaultValue;
		}
		if (value == null)
		{
			return defaultValue;
		}
		string text = Convert.ToString(value);
		if (string.IsNullOrEmpty(text))
		{
			return defaultValue;
		}
		return text;
	}

	internal static string ToString(this Dictionary<string, object> data, string key)
	{
		if (!data.TryGetValue(key, out var value))
		{
			return string.Empty;
		}
		if (value == null)
		{
			return string.Empty;
		}
		string text = Convert.ToString(value);
		if (string.IsNullOrEmpty(text))
		{
			return string.Empty;
		}
		return text;
	}

	internal static int? ToInt32(this Dictionary<string, object> data, string key, int? defaultValue)
	{
		if (!data.TryGetValue(key, out var value))
		{
			return defaultValue;
		}
		if (value == null)
		{
			return defaultValue;
		}
		return Convert.ToInt32(value);
	}

	internal static int ToInt32(this Dictionary<string, object> data, string key)
	{
		if (!data.TryGetValue(key, out var value))
		{
			return 0;
		}
		if (value == null)
		{
			return 0;
		}
		return Convert.ToInt32(value);
	}

	internal static long? ToInt64(this Dictionary<string, object> data, string key, long? defaultValue)
	{
		if (!data.TryGetValue(key, out var value))
		{
			return defaultValue;
		}
		if (value == null)
		{
			return defaultValue;
		}
		return Convert.ToInt64(value);
	}

	internal static long ToInt64(this Dictionary<string, object> data, string key)
	{
		if (!data.TryGetValue(key, out var value))
		{
			return 0L;
		}
		if (value == null)
		{
			return 0L;
		}
		return Convert.ToInt64(value);
	}

	internal static decimal? ToDecimal(this Dictionary<string, object> data, string key, decimal? defaultValue)
	{
		if (!data.TryGetValue(key, out var value))
		{
			return defaultValue;
		}
		if (value == null)
		{
			return defaultValue;
		}
		return Convert.ToDecimal(value);
	}

	internal static decimal ToDecimal(this Dictionary<string, object> data, string key)
	{
		if (!data.TryGetValue(key, out var value))
		{
			return 0m;
		}
		if (value == null)
		{
			return 0m;
		}
		return Convert.ToDecimal(value);
	}

	internal static DateTime? ToDateTime(this Dictionary<string, object> data, string key, DateTime? defaultValue)
	{
		if (!data.TryGetValue(key, out var value))
		{
			return defaultValue;
		}
		if (value == null)
		{
			return defaultValue;
		}
		string text = Convert.ToString(value);
		if (text.Length == "yyyyMMdd".Length)
		{
			return DateTime.ParseExact(text, "yyyyMMdd", CultureInfo.CurrentCulture);
		}
		return DateTime.ParseExact(text, "yyyyMMddHHmmss", CultureInfo.CurrentCulture);
	}

	internal static bool? ToBoolean(this Dictionary<string, object> data, string key, bool? defaultValue)
	{
		if (!data.TryGetValue(key, out var value))
		{
			return defaultValue;
		}
		if (value == null)
		{
			return defaultValue;
		}
		return Convert.ToBoolean(value);
	}

	internal static bool ToBoolean(this Dictionary<string, object> data, string key)
	{
		if (!data.TryGetValue(key, out var value))
		{
			return false;
		}
		if (value == null)
		{
			return false;
		}
		return Convert.ToBoolean(value);
	}

	internal static List<long> ToListLong(this Dictionary<string, object> data, string key)
	{
		List<long> result = new List<long>();
		if (!data.TryGetValue(key, out var value))
		{
			return result;
		}
		if (value == null || !(value is List<string>))
		{
			return result;
		}
		((List<string>)value).ForEach(delegate(string item)
		{
			result.Add(Convert.ToInt64(item));
		});
		return result;
	}

	internal static List<string> ToListString(this Dictionary<string, object> data, string key)
	{
		List<string> result = new List<string>();
		if (!data.TryGetValue(key, out var value))
		{
			return result;
		}
		if (value == null || !(value is List<string>))
		{
			return result;
		}
		((List<string>)value).ForEach(delegate(string item)
		{
			result.Add(Convert.ToString(item));
		});
		return result;
	}

	internal static List<object> ToDictionaryObject(this Dictionary<string, object> data, string key)
	{
		List<object> result = new List<object>();
		if (!data.TryGetValue(key, out var value))
		{
			return result;
		}
		if (value == null || !(value is Dictionary<string, object>))
		{
			return result;
		}
		((Dictionary<string, object>)value).ForEach(delegate(KeyValuePair<string, object> item)
		{
			result.Add(item);
		});
		return result;
	}
}
