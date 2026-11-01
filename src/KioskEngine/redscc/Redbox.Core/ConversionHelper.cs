using System;
using System.ComponentModel;

namespace Redbox.Core;

public static class ConversionHelper
{
	public static bool IsNullableType(Type type)
	{
		return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
	}

	public static Type GetNullableTypeIfWrapped(Type type)
	{
		return IsNullableType(type) ? type.GetGenericArguments()[0] : type;
	}

	public static object ChangeType(object value, Type convertToType)
	{
		if (value == null)
		{
			return null;
		}
		if (convertToType.IsEnum)
		{
			if (value is string)
			{
				FlagsAttribute[] flagsAttributes = (FlagsAttribute[])convertToType.GetCustomAttributes(typeof(FlagsAttribute), inherit: false);
				if (flagsAttributes != null && flagsAttributes.Length > 0)
				{
					int flags = 0;
					string names = (string)value;
					foreach (string name in names.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
					{
						object flagValue = Enum.Parse(convertToType, name.Trim(), ignoreCase: true);
						flags |= Convert.ToInt32(flagValue);
					}
					return flags;
				}
				return Enum.Parse(convertToType, (string)value, ignoreCase: true);
			}
			if (value is int || value is byte || value is long || value is short || value is float || value is double)
			{
				return Enum.ToObject(convertToType, Convert.ToInt32(value));
			}
		}
		if (convertToType == typeof(Type) && value is string)
		{
			return Type.GetType((string)value);
		}
		TypeConverter converter = TypeDescriptor.GetConverter(convertToType);
		if (converter != null && converter.CanConvertFrom(typeof(string)) && value is string)
		{
			return converter.ConvertFrom(value);
		}
		if (convertToType == typeof(string))
		{
			converter = TypeDescriptor.GetConverter(value.GetType());
			if (converter != null && converter.CanConvertTo(typeof(string)))
			{
				return converter.ConvertToString(value);
			}
		}
		if (!convertToType.IsInstanceOfType(value))
		{
			try
			{
				return Convert.ChangeType(value, GetNullableTypeIfWrapped(convertToType));
			}
			catch (InvalidCastException)
			{
			}
		}
		return value;
	}
}
