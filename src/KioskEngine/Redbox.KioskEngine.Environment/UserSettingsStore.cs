using Microsoft.Win32;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Environment;

public class UserSettingsStore : IUserSettingsStore
{
	public static UserSettingsStore Instance => Singleton<UserSettingsStore>.Instance;

	public string RootPath { get; set; }

	public T GetValue<T>(string path, string key)
	{
		return GetValue(path, key, default(T));
	}

	public T GetValue<T>(string path, string key, T defaultValue)
	{
		RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(FormatPath(path), writable: false);
		if (registryKey == null)
		{
			return defaultValue;
		}
		object value = registryKey.GetValue(key, defaultValue);
		registryKey.Close();
		if (value == null || value.Equals(default(T)))
		{
			return defaultValue;
		}
		return (T)ConversionHelper.ChangeType(value, typeof(T));
	}

	public void SetValue<T>(string path, string key, T value)
	{
		RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(FormatPath(path));
		if (registryKey != null)
		{
			registryKey.SetValue(key, value);
			EnvironmentManager.Instance.NotifyConfigurationCallbacks("user", "set", path, key, value?.ToString());
		}
	}

	public void RemoveKey(string path)
	{
		try
		{
			Registry.CurrentUser.DeleteSubKeyTree(FormatPath(path));
			EnvironmentManager.Instance.NotifyConfigurationCallbacks("user", "removekey", path, null, null);
		}
		catch
		{
		}
	}

	public void RemoveValue(string path, string key)
	{
		RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(FormatPath(path), writable: true);
		if (registryKey != null)
		{
			registryKey.DeleteValue(key);
			registryKey.Close();
			EnvironmentManager.Instance.NotifyConfigurationCallbacks("machine", "removevalue", path, key, null);
		}
	}

	private UserSettingsStore()
	{
	}

	private string FormatPath(string path)
	{
		return $"{RootPath}\\{path}";
	}
}
