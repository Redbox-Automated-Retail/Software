using System;
using System.ComponentModel;
using Microsoft.Win32;

namespace HALUtilities;

internal sealed class RegistryStore
{
	private readonly string Key;

	internal T GetSecretValue<T>(string option, T def)
	{
		RegistryKey registryKey = null;
		try
		{
			registryKey = Registry.LocalMachine.CreateSubKey(Key);
			if (registryKey == null)
			{
				return def;
			}
			object value = registryKey.GetValue(option);
			if (value == null)
			{
				return def;
			}
			return (T)ChangeType(value, typeof(T));
		}
		catch (Exception)
		{
			Console.WriteLine("Unable to read secret option {0}", option);
			return def;
		}
		finally
		{
			registryKey?.Close();
		}
	}

	internal void SetSecretValue<T>(string option, T value)
	{
		RegistryKey registryKey = null;
		try
		{
			registryKey = Registry.LocalMachine.CreateSubKey(Key);
			registryKey?.SetValue(option, value);
		}
		catch (Exception)
		{
			Console.WriteLine("Unable to set secret option {0}", option);
		}
		finally
		{
			registryKey?.Close();
		}
	}

	internal void RemoveValue(string option)
	{
		RegistryKey registryKey = null;
		try
		{
			registryKey = Registry.LocalMachine.OpenSubKey(Key, writable: true);
			registryKey?.DeleteValue(option);
		}
		catch (Exception)
		{
			Console.WriteLine("Unable to delete secret option {0}", option);
		}
		finally
		{
			registryKey?.Close();
		}
	}

	internal RegistryStore(string key)
	{
		Key = key;
	}

	private object ChangeType(object value, Type convertToType)
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
		if ((object)convertToType == typeof(string))
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

	private Type GetNullableTypeIfWrapped(Type type)
	{
		if (type.IsGenericType && (object)type.GetGenericTypeDefinition() == typeof(Nullable<>))
		{
			return type.GetGenericArguments()[0];
		}
		return type;
	}
}
