using System;
using Microsoft.Win32;

namespace Redbox.HAL.Core;

public class RegistryOptions
{
	public static RegistryOptions Instance => Singleton<RegistryOptions>.Instance;

	public T GetSecretValue<T>(string regKey, string option)
	{
		T result = default(T);
		RegistryKey registryKey = null;
		try
		{
			registryKey = Registry.LocalMachine.CreateSubKey(regKey);
			if (registryKey == null)
			{
				return result;
			}
			object value = registryKey.GetValue(option);
			result = (T)ConversionHelper.ChangeType(value, typeof(T));
			return result;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"Unable to read secret option {option}", e);
			return result;
		}
		finally
		{
			registryKey?.Close();
		}
	}

	public T GetSecretValue<T>(string regKey, string option, T def)
	{
		RegistryKey registryKey = null;
		try
		{
			registryKey = Registry.LocalMachine.CreateSubKey(regKey);
			if (registryKey == null)
			{
				return def;
			}
			object value = registryKey.GetValue(option);
			if (value == null)
			{
				return def;
			}
			return (T)ConversionHelper.ChangeType(value, typeof(T));
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"Unable to read secret option {option}", e);
			return def;
		}
		finally
		{
			registryKey?.Close();
		}
	}

	public void SetSecretValue<T>(string regKey, string option, T value)
	{
		RegistryKey registryKey = null;
		try
		{
			registryKey = Registry.LocalMachine.CreateSubKey(regKey);
			registryKey?.SetValue(option, value);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"Unable to set secret option {option}", e);
		}
		finally
		{
			registryKey?.Close();
		}
	}

	public void RemoveValue(string regKey, string option)
	{
		RegistryKey registryKey = null;
		try
		{
			registryKey = Registry.LocalMachine.OpenSubKey(regKey, writable: true);
			registryKey?.DeleteValue(option);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"Unable to delete secret option {option}", e);
		}
		finally
		{
			registryKey?.Close();
		}
	}

	private RegistryOptions()
	{
	}
}
