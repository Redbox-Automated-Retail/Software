using System;
using System.Diagnostics;
using System.Security.Principal;
using Microsoft.Win32;
using Redbox.Core;
using Redbox.Lua;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.Kernel;

public static class RegistryFunctions
{
	[KernelFunction(Name = "kernel.getuserregistryprefix")]
	internal static string GetUserRegistryPrefix(string user)
	{
		try
		{
			SecurityIdentifier securityIdentifier = (SecurityIdentifier)new NTAccount(user).Translate(typeof(SecurityIdentifier));
			if (securityIdentifier != null)
			{
				return $"HKEY_USERS\\{securityIdentifier.Value}";
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Failed to get sid for user.", e);
		}
		return string.Empty;
	}

	[KernelFunction(Name = "kernel.getuserregistryprefixwithdomain")]
	internal static string GetUserRegistryPrefixWithDomain(string domain, string user)
	{
		try
		{
			SecurityIdentifier securityIdentifier = (SecurityIdentifier)new NTAccount(domain, user).Translate(typeof(SecurityIdentifier));
			if (securityIdentifier != null)
			{
				return $"HKEY_USERS\\{securityIdentifier.Value}";
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Failed to get sid for user.", e);
		}
		return string.Empty;
	}

	[KernelFunction(Name = "kernel.deleteregistrysubkey")]
	internal static bool DeleteSubKey(string path, string k)
	{
		try
		{
			RegistryKey registryKey = null;
			if (path.StartsWith("HKEY_CURRENT_USER"))
			{
				registryKey = Registry.CurrentUser.OpenSubKey(path.Replace("HKEY_CURRENT_USER\\", ""), writable: true);
			}
			else if (path.StartsWith("HKEY_LOCAL_MACHINE"))
			{
				registryKey = Registry.LocalMachine.OpenSubKey(path.Replace("HKEY_LOCAL_MACHINE\\", ""), writable: true);
			}
			else if (path.StartsWith("HKEY_CURRENT_CONFIG"))
			{
				registryKey = Registry.CurrentConfig.OpenSubKey(path.Replace("HKEY_CURRENT_CONFIG\\", ""), writable: true);
			}
			else if (path.StartsWith("HKEY_USERS"))
			{
				registryKey = Registry.Users.OpenSubKey(path.Replace("HKEY_USERS\\", ""), writable: true);
			}
			else if (path.StartsWith("HKEY_CLASSES_ROOT"))
			{
				registryKey = Registry.ClassesRoot.OpenSubKey(path.Replace("HKEY_CLASSES_ROOT\\", ""), writable: true);
			}
			if (registryKey != null)
			{
				registryKey.DeleteSubKeyTree(k);
				registryKey.Close();
				return true;
			}
		}
		catch (Exception)
		{
			return false;
		}
		return false;
	}

	[KernelFunction(Name = "kernel.deleteregistryvalue")]
	internal static bool DeleteValue(string path, string value)
	{
		try
		{
			RegistryKey registryKey = null;
			if (path.StartsWith("HKEY_CURRENT_USER"))
			{
				registryKey = Registry.CurrentUser.OpenSubKey(path.Replace("HKEY_CURRENT_USER\\", ""), writable: true);
			}
			else if (path.StartsWith("HKEY_LOCAL_MACHINE"))
			{
				registryKey = Registry.LocalMachine.OpenSubKey(path.Replace("HKEY_LOCAL_MACHINE\\", ""), writable: true);
			}
			else if (path.StartsWith("HKEY_CURRENT_CONFIG"))
			{
				registryKey = Registry.CurrentConfig.OpenSubKey(path.Replace("HKEY_CURRENT_CONFIG\\", ""), writable: true);
			}
			else if (path.StartsWith("HKEY_USERS"))
			{
				registryKey = Registry.Users.OpenSubKey(path.Replace("HKEY_USERS\\", ""), writable: true);
			}
			else if (path.StartsWith("HKEY_CLASSES_ROOT"))
			{
				registryKey = Registry.ClassesRoot.OpenSubKey(path.Replace("HKEY_CLASSES_ROOT\\", ""), writable: true);
			}
			if (registryKey != null)
			{
				registryKey.DeleteValue(value);
				registryKey.Close();
				return true;
			}
		}
		catch (Exception)
		{
			return false;
		}
		return false;
	}

	[KernelFunction(Name = "kernel.getregistrykey")]
	internal static object GetKey(string path, string name)
	{
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		return Registry.GetValue(service.ExpandProperties(path), service.ExpandProperties(name), string.Empty);
	}

	[KernelFunction(Name = "kernel.setregistrykey")]
	internal static void SetKey(string path, string name, object value)
	{
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		object value2 = value;
		if (value is string)
		{
			value2 = service.ExpandProperties((string)value);
		}
		Registry.SetValue(service.ExpandProperties(path), service.ExpandProperties(name), value2);
	}

	[KernelFunction(Name = "kernel.setdwordregistrykey")]
	internal static void SetDWORDKey(string path, string name, int value)
	{
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		Registry.SetValue(service.ExpandProperties(path), service.ExpandProperties(name), value, RegistryValueKind.DWord);
	}

	[KernelFunction(Name = "kernel.getdotnetversions")]
	internal static LuaTable GetDotNetVersions()
	{
		LuaTable luaTable = new LuaTable(KernelService.Instance.LuaRuntime);
		LuaTable luaTable2 = new LuaTable(KernelService.Instance.LuaRuntime);
		luaTable["success"] = false;
		luaTable["versions"] = luaTable2;
		RegistryKey registryKey = null;
		try
		{
			registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\NET Framework Setup\\NDP");
			if (registryKey != null)
			{
				int num = 1;
				string[] subKeyNames = registryKey.GetSubKeyNames();
				foreach (string name in subKeyNames)
				{
					RegistryKey registryKey2 = registryKey.OpenSubKey(name);
					if (registryKey2 != null)
					{
						LuaTable luaTable3 = new LuaTable(KernelService.Instance.LuaRuntime);
						luaTable3["version"] = registryKey2.GetValue("Version");
						luaTable3["service_pack"] = (int)(registryKey2.GetValue("SP") ?? ((object)0));
						luaTable3["install_path"] = registryKey2.GetValue("InstallPath");
						luaTable2[num++] = luaTable3;
						registryKey2.Close();
					}
				}
				luaTable["success"] = true;
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in RegistryFunctions.GetDotNetVersions.", e);
		}
		finally
		{
			registryKey?.Close();
		}
		return luaTable;
	}

	[KernelFunction(Name = "kernel.exportregistryfile")]
	internal static void ExportRegistryKey(string fileName, string key)
	{
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		string text = service.ExpandProperties(key);
		string text2 = service.ExpandProperties(fileName);
		if (!string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(text2))
		{
			ProcessStartInfo startInfo = new ProcessStartInfo
			{
				CreateNoWindow = true,
				UseShellExecute = false,
				FileName = "regedit.exe",
				WindowStyle = ProcessWindowStyle.Hidden,
				Arguments = $"/E \"{text2}\" {text}"
			};
			Process process = new Process();
			process.StartInfo = startInfo;
			process.Start();
			process.WaitForExit();
		}
	}

	[KernelFunction(Name = "kernel.importregistryfile")]
	internal static void ImportRegistryFile(string fileName)
	{
		string text = ServiceLocator.Instance.GetService<IMacroService>().ExpandProperties(fileName);
		if (!string.IsNullOrEmpty(text))
		{
			ProcessStartInfo startInfo = new ProcessStartInfo
			{
				CreateNoWindow = true,
				UseShellExecute = false,
				FileName = "regedit.exe",
				WindowStyle = ProcessWindowStyle.Hidden,
				Arguments = $"/S \"{text}\""
			};
			Process process = new Process();
			process.StartInfo = startInfo;
			process.Start();
			process.WaitForExit();
		}
	}
}
