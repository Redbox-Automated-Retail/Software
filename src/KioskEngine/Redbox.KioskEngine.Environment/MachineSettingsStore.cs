using System;
using Microsoft.Win32;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Environment;

public class MachineSettingsStore : IMachineSettingsStore
{
	public static MachineSettingsStore Instance => Singleton<MachineSettingsStore>.Instance;

	public string RootPath { get; set; }

	public void Initialize(string rootPath)
	{
		LogHelper.Instance.Log("Initialize machine settings store, root path: {0}", rootPath);
		RootPath = rootPath;
	}

	public T GetValue<T>(string path, string key)
	{
		return GetValue(path, key, default(T));
	}

	public T GetValue<T>(string path, string key, T defaultValue)
	{
		RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(FormatPath(path), writable: false);
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
		RegistryKey registryKey = Registry.LocalMachine.CreateSubKey(FormatPath(path));
		if (registryKey != null)
		{
			registryKey.SetValue(key, value);
			EnvironmentManager.Instance.NotifyConfigurationCallbacks("machine", "set", path, key, value?.ToString());
		}
	}

	public void RemoveKey(string path)
	{
		Registry.LocalMachine.DeleteSubKeyTree(FormatPath(path));
		EnvironmentManager.Instance.NotifyConfigurationCallbacks("machine", "removekey", path, null, null);
	}

	public void RemoveValue(string path, string key)
	{
		RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(FormatPath(path), writable: true);
		if (registryKey == null)
		{
			return;
		}
		try
		{
			registryKey.DeleteValue(key);
			registryKey.Close();
			EnvironmentManager.Instance.NotifyConfigurationCallbacks("machine", "removevalue", path, key, null);
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("Failed to remove value " + key + " at path " + path + ".", e);
		}
	}

	private MachineSettingsStore()
	{
	}

	private string FormatPath(string path)
	{
		return $"{RootPath}\\{path}";
	}
}
