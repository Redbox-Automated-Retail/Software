using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.Kernel;

[PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
public static class ShellLauncherFunctions
{
	private static readonly uint S_OK = 0u;

	private static readonly string DefaultShellCommandLine = "/namespace:\\\\root\\StandardCimv2\\Embedded CLASS WESL_UserSetting CALL SetDefaultShell Shell=\"{0}\" DefaultAction=0";

	private static readonly string AdminShellCommandLine = "/namespace:\\\\root\\StandardCimv2\\Embedded CLASS WESL_UserSetting CALL SetCustomShell Sid=\"S-1-5-32-544\" Shell=\"{0}\" DefaultAction=0";

	private static readonly string ShellEnabledCommandLine = "/namespace:\\\\root\\StandardCimv2\\Embedded CLASS WESL_UserSetting CALL SetEnabled Enabled=true";

	[DllImport("slc.dll", CharSet = CharSet.Unicode)]
	private static extern uint SLGetWindowsInformationDWORD([MarshalAs(UnmanagedType.LPWStr)] string valueName, out int value);

	[KernelFunction(Name = "kernel.shelllauncherfeatureinstalled")]
	internal static bool ShellLauncherWindowsFeatureInstalled()
	{
		try
		{
			uint num = SLGetWindowsInformationDWORD("EmbeddedFeature-ShellLauncher-Enabled", out var value);
			LogHelper.Instance.Log("ShellLauncherWindowsFeatureInstalled() - SLGetWindowsInformationDWORD returned: " + num);
			if (num == S_OK)
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
		try
		{
			string arg = ServiceLocator.Instance.GetService<IMacroService>().ExpandProperties(shellExecutable);
			string arguments = string.Format(DefaultShellCommandLine, arg);
			Process process = new Process();
			process.StartInfo = new ProcessStartInfo("wmic.exe", arguments)
			{
				CreateNoWindow = true,
				WindowStyle = ProcessWindowStyle.Hidden,
				Verb = "runas"
			};
			process.Start();
			process.WaitForExit(120000);
			if (process.ExitCode == 0)
			{
				arguments = string.Format(AdminShellCommandLine, arg);
				Process process2 = new Process();
				process2.StartInfo = new ProcessStartInfo("wmic.exe", arguments)
				{
					CreateNoWindow = true,
					WindowStyle = ProcessWindowStyle.Hidden,
					Verb = "runas"
				};
				process2.Start();
				process2.WaitForExit(120000);
				if (process2.ExitCode == 0)
				{
					Process process3 = new Process();
					process3.StartInfo = new ProcessStartInfo("wmic.exe", ShellEnabledCommandLine)
					{
						CreateNoWindow = true,
						WindowStyle = ProcessWindowStyle.Hidden,
						Verb = "runas"
					};
					process3.Start();
					process3.WaitForExit(120000);
					if (process3.ExitCode == 0)
					{
						return true;
					}
				}
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was rasied in ShellLauncherFunctions.ReplaceShellWith.", e);
		}
		return false;
	}
}
