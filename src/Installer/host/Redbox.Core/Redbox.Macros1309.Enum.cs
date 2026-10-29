using System;
using System.Collections.Generic;

namespace Redbox.Core;

internal static class Redbox_002EMacros1309_002EEnum<T>
{
	public static T Parse(string value, T defaultValue)
	{
		T result = defaultValue;
		try
		{
			result = (T)Enum.Parse(typeof(T), value);
		}
		catch (ArgumentException)
		{
		}
		return result;
	}

	public static T Parse(string value)
	{
		return (T)Enum.Parse(typeof(T), value, ignoreCase: true);
	}

	public static T ParseIgnoringCase(string value, T defaultValue)
	{
		T result = defaultValue;
		try
		{
			result = (T)Enum.Parse(typeof(T), value, ignoreCase: true);
		}
		catch (ArgumentException)
		{
		}
		return result;
	}

	public static IList<T> GetValues()
	{
		List<T> list = new List<T>();
		foreach (object value in Enum.GetValues(typeof(T)))
		{
			list.Add((T)value);
		}
		return list;
	}
}
