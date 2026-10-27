using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Kernel;

internal static class SettingsFunctions
{
	[KernelFunction(Name = "kernel.getuservalue")]
	internal static string GetUserSettingValue(string path, string key)
	{
		return ServiceLocator.Instance.GetService<IUserSettingsStore>().GetValue<string>(path, key);
	}

	[KernelFunction(Name = "kernel.getuservalueordefault")]
	internal static string GetUserSettingValue(string path, string key, string defaultValue)
	{
		return ServiceLocator.Instance.GetService<IUserSettingsStore>().GetValue(path, key, defaultValue);
	}

	[KernelFunction(Name = "kernel.setuservalue")]
	internal static void SetUserSettingValue(string path, string key, string value)
	{
		ServiceLocator.Instance.GetService<IUserSettingsStore>().SetValue(path, key, value);
	}

	[KernelFunction(Name = "kernel.removeuserkey")]
	internal static void RemoveUserSettingKey(string path)
	{
		ServiceLocator.Instance.GetService<IUserSettingsStore>().RemoveKey(path);
	}

	[KernelFunction(Name = "kernel.removeuservalue")]
	internal static void RemoveUserSettingValue(string path, string key)
	{
		ServiceLocator.Instance.GetService<IUserSettingsStore>().RemoveValue(path, key);
	}

	[KernelFunction(Name = "kernel.getmachinevalue")]
	internal static string GetMachineSettingValue(string path, string key)
	{
		return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue<string>(path, key);
	}

	[KernelFunction(Name = "kernel.getmachinevalueordefault")]
	internal static string GetMachineSettingValue(string path, string key, string defaultValue)
	{
		return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue(path, key, defaultValue);
	}

	[KernelFunction(Name = "kernel.setmachinevalue")]
	internal static void SetMachineSettingValue(string path, string key, string value)
	{
		ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue(path, key, value);
	}

	[KernelFunction(Name = "kernel.removemachinekey")]
	internal static void RemoveMachineSettingKey(string path)
	{
		ServiceLocator.Instance.GetService<IMachineSettingsStore>().RemoveKey(path);
	}

	[KernelFunction(Name = "kernel.removemachinevalue")]
	internal static void RemoveMachineSettingValue(string path, string key)
	{
		ServiceLocator.Instance.GetService<IMachineSettingsStore>().RemoveValue(path, key);
	}

	[KernelFunction(Name = "kernel.registerconfigurationcallback")]
	internal static void RegisterConfigurationCallback(string name, ConfigurationCallback callback)
	{
		ServiceLocator.Instance.GetService<IEnvironmentNotificationService>().RegisterConfigurationCallback(name, callback);
	}

	[KernelFunction(Name = "kernel.removeconfigurationcallback")]
	internal static void RemoveConfigurationCallback(string name)
	{
		ServiceLocator.Instance.GetService<IEnvironmentNotificationService>().UnregisterConfigurationCallback(name);
	}
}
