using System;
using System.ComponentModel;

namespace Redbox.HAL.Core;

public static class ConversionHelper
{
	public static Type GetNullableTypeIfWrapped(Type type)
	{
		if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
		{
			return type.GetGenericArguments()[0];
		}
		return type;
	}

	public static object ChangeType(object value, Type convertToType)
	{
		if (value == null)
		{
			return null;
		}
		if (convertToType.IsEnum && value is string)
		{
			return Enum.Parse(convertToType, (string)value, ignoreCase: true);
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
