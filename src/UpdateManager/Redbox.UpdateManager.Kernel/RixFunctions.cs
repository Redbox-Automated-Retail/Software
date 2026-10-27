using System.Diagnostics;
using System.IO;
using Microsoft.Win32;
using Redbox.Core;
using Redbox.Lua;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.Kernel;

public static class RixFunctions
{
	[KernelFunction(Name = "kernel.rixinstall")]
	internal static int InstallPackage(string path, LuaTable parameters)
	{
		string text = "--force --silent";
		foreach (object key in parameters.Keys)
		{
			text += $" -D:{key}=\"{parameters[key]}\"";
		}
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		Process process = new Process();
		process.StartInfo = new ProcessStartInfo(service.ExpandProperties(path), text)
		{
			CreateNoWindow = true,
			WindowStyle = ProcessWindowStyle.Hidden,
			WorkingDirectory = path
		};
		process.Start();
		process.WaitForExit();
		return process.ExitCode;
	}

	[KernelFunction(Name = "kernel.rixuninstall")]
	internal static int UninstallPackage(string path, LuaTable parameters)
	{
		string text = "--force --silent";
		foreach (object key in parameters.Keys)
		{
			text += $" -D:{key}=\"{parameters[key]}\"";
		}
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		Process process = new Process();
		process.StartInfo = new ProcessStartInfo(service.ExpandProperties(path), text)
		{
			CreateNoWindow = true,
			WindowStyle = ProcessWindowStyle.Hidden,
			WorkingDirectory = path
		};
		process.Start();
		process.WaitForExit();
		return process.ExitCode;
	}

	[KernelFunction(Name = "kernel.rixisproductinstalled")]
	internal static bool IsRixPackageInstalled(string productGuid)
	{
		if (string.IsNullOrEmpty(productGuid))
		{
			return false;
		}
		return !string.IsNullOrEmpty((string)Registry.GetValue($"HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\{productGuid}", "SilentUninstallString", null));
	}

	[KernelFunction(Name = "kernel.rixfindinstalldirectory")]
	internal static string FindInstalledDirectory(string productGuid)
	{
		if (string.IsNullOrEmpty(productGuid))
		{
			return null;
		}
		string text = (string)Registry.GetValue($"HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\{productGuid}", "InstallPath", null);
		if (!string.IsNullOrEmpty(text))
		{
			return text;
		}
		return null;
	}

	[KernelFunction(Name = "kernel.rixuninstallproduct")]
	internal static int UninstallPackage(string productGuid)
	{
		if (string.IsNullOrEmpty(productGuid))
		{
			return -1;
		}
		string text = (string)Registry.GetValue($"HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\{productGuid}", "SilentUninstallString", null);
		if (string.IsNullOrEmpty(text))
		{
			return -1;
		}
		int num = text.IndexOf(".exe");
		string text2 = text.Substring(0, num + 5);
		string arguments = text.Substring(num + 5);
		if (!File.Exists(text2))
		{
			LogHelper.Instance.Log("In rixuninstallproduct, {0} does not exists from product {1}.", text2, productGuid);
			return -1;
		}
		Process process = new Process();
		process.StartInfo = new ProcessStartInfo
		{
			FileName = text2,
			Arguments = arguments,
			CreateNoWindow = true,
			UseShellExecute = false,
			WindowStyle = ProcessWindowStyle.Hidden
		};
		process.Start();
		process.WaitForExit();
		return process.ExitCode;
	}
}
