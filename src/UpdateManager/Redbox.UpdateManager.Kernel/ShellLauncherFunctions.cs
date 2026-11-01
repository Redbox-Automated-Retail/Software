using System;
using System.Management;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.Kernel;

[PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
public static class ShellLauncherFunctions
{
	private static readonly uint S_OK = 0u;

	private const string ShellLauncherNamespace = "\\\\.\\root\\StandardCimv2\\Embedded";

	private const string AdministratorsSid = "S-1-5-32-544";

	// Shell Launcher's DefaultAction 0: restart the shell when it exits.
	private const int RestartShell = 0;

	[DllImport("slc.dll", CharSet = CharSet.Unicode)]
	private static extern uint SLGetWindowsInformationDWORD([MarshalAs(UnmanagedType.LPWStr)] string valueName, out int value);

	[KernelFunction(Name = "kernel.shelllauncherfeatureinstalled")]
	internal static bool ShellLauncherWindowsFeatureInstalled()
	{
		try
		{
			uint status = SLGetWindowsInformationDWORD("EmbeddedFeature-ShellLauncher-Enabled", out var value);
			LogHelper.Instance.Log("ShellLauncherWindowsFeatureInstalled() - SLGetWindowsInformationDWORD returned: " + status);
			if (status == S_OK)
			{
				LogHelper.Instance.Log("ShellLauncherWindowsFeatureInstalled() - enabled: " + value);
				return value != 0;
			}
			return false;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Exception calling ShellLauncherWindowsFeatureInstalled()!", e);
		}
		return false;
	}

	[KernelFunction(Name = "kernel.replaceshellwith")]
	internal static bool ReplaceShellWith(string shellExecutable)
	{
		// The original ran three "wmic ... CLASS WESL_UserSetting CALL ..." commands; wmic.exe is gone from
		// Windows 11, so the same static methods are called through WMI directly, in the same order.
		try
		{
			string shell = ServiceLocator.Instance.GetService<IMacroService>().ExpandProperties(shellExecutable);
			using ManagementClass userSetting = new ManagementClass(ShellLauncherNamespace, "WESL_UserSetting", null);
			return InvokeShellLauncher(userSetting, "SetDefaultShell", input =>
				{
					input["Shell"] = shell;
					input["DefaultAction"] = RestartShell;
				})
				&& InvokeShellLauncher(userSetting, "SetCustomShell", input =>
				{
					input["Sid"] = AdministratorsSid;
					input["Shell"] = shell;
					input["DefaultAction"] = RestartShell;
				})
				&& InvokeShellLauncher(userSetting, "SetEnabled", input => input["Enabled"] = true);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was rasied in ShellLauncherFunctions.ReplaceShellWith.", e);
		}
		return false;
	}

	private static bool InvokeShellLauncher(ManagementClass userSetting, string method, Action<ManagementBaseObject> setParameters)
	{
		using ManagementBaseObject input = userSetting.GetMethodParameters(method);
		setParameters(input);
		using ManagementBaseObject output = userSetting.InvokeMethod(method, input, null);
		// The WESL_UserSetting methods report failure by throwing; ReturnValue, when present, is an HRESULT.
		uint result = S_OK;
		if (output != null)
		{
			foreach (PropertyData property in output.Properties)
			{
				if (property.Name == "ReturnValue" && property.Value != null)
				{
					result = Convert.ToUInt32(property.Value);
				}
			}
		}
		LogHelper.Instance.Log($"ShellLauncherFunctions: WESL_UserSetting.{method} returned {result}.");
		return result == S_OK;
	}
}
