using System;
using Microsoft.Win32;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Kernel;

internal static class RegistryFunctions
{
	[KernelFunction(Name = "kernel.getregistryvalue")]
	internal static string GetValue(string path, string key)
	{
		try
		{
			RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(path, writable: false);
			if (registryKey == null)
			{
				return null;
			}
			object value = registryKey.GetValue(key);
			registryKey.Close();
			if (value == null)
			{
				return null;
			}
			return (string)ConversionHelper.ChangeType(value, typeof(string));
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in RegistryFunctions.GetValue.", e);
		}
		return null;
	}

	[KernelFunction(Name = "kernel.setregistryvalue")]
	internal static void SetValue(string path, string key, string value)
	{
		try
		{
			Registry.LocalMachine.CreateSubKey(path)?.SetValue(key, value);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in RegistryFunctions.SetValue.", e);
		}
	}

	[KernelFunction(Name = "kernel.removeregistrykey")]
	internal static void RemoveKey(string path)
	{
		try
		{
			Registry.LocalMachine.DeleteSubKeyTree(path);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in RegistryFunctions.RemoveKey.", e);
		}
	}

	[KernelFunction(Name = "kernel.removeregistryvalue")]
	internal static void RemoveValue(string path, string key)
	{
		try
		{
			RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(path, writable: true);
			if (registryKey != null)
			{
				registryKey.DeleteValue(key);
				registryKey.Close();
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in RegistryFunctions.RemoveValue.", e);
		}
	}
}
