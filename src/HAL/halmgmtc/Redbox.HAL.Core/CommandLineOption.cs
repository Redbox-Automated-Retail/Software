using System;

namespace Redbox.HAL.Core;

public static class CommandLineOption
{
	public const string ColonDelimiter = ":";

	public const string AssignDelimiter = "=";

	public static string GetOptionValue(string option)
	{
		return GetOptionValue(option, ":");
	}

	public static string GetOptionValue(string option, string delimiter)
	{
		if (!option.Contains(delimiter))
		{
			return string.Empty;
		}
		string[] array = option.Split(new string[1] { delimiter }, StringSplitOptions.RemoveEmptyEntries);
		if (array.Length != 2)
		{
			return string.Empty;
		}
		return array[1];
	}

	public static T GetOptionVal<T>(string option, T defVal)
	{
		string optionValue = GetOptionValue(option);
		if (string.IsNullOrEmpty(optionValue))
		{
			return defVal;
		}
		return (T)ConversionHelper.ChangeType(optionValue, typeof(T));
	}

	public static T GetOptionVal<T>(string option, string delimiter, T defVal)
	{
		string optionValue = GetOptionValue(option, delimiter);
		if (string.IsNullOrEmpty(optionValue))
		{
			return defVal;
		}
		return (T)ConversionHelper.ChangeType(optionValue, typeof(T));
	}
}
