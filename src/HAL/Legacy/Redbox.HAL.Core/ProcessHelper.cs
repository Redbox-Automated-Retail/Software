using System;
using System.Diagnostics;

namespace Redbox.HAL.Core;

public static class ProcessHelper
{
	public const string HalServiceProcess = "halsvc-win32.exe";

	public const string TesterProcess = "MSHALTester.exe";

	public const string ConsoleProcess = "halsvc-console.exe";

	public static ProcessStartInfo SetupDefaultProcessInfo(string processName)
	{
		ProcessStartInfo processStartInfo = new ProcessStartInfo(processName);
		processStartInfo.UserName = null;
		processStartInfo.Password = null;
		return processStartInfo;
	}

	public static bool KillProcess(string name)
	{
		Process[] processesByName = Process.GetProcessesByName(name);
		if (processesByName.Length == 0)
		{
			return false;
		}
		Process[] array = processesByName;
		foreach (Process process in array)
		{
			Console.WriteLine("process = {0}", process.ProcessName);
			try
			{
				process.Kill();
			}
			catch
			{
			}
		}
		return true;
	}

	public static bool HALProcessRunning()
	{
		return ProcessIsRunningInner("halsvc-win32.exe");
	}

	public static bool ProcessIsRunning(string name)
	{
		return ProcessIsRunningInner(name);
	}

	public static void DumpProcessMemoryMetrics()
	{
		Process currentProcess = Process.GetCurrentProcess();
		LogHelper.Instance.Log(" --- Memory dump --- ");
		LogHelper.Instance.Log("    Working set: {0} bytes", currentProcess.WorkingSet64);
		LogHelper.Instance.Log("    VM size: {0} bytes", currentProcess.VirtualMemorySize64);
		LogHelper.Instance.Log("    Private memory size: {0} bytes", currentProcess.PrivateMemorySize64);
		LogHelper.Instance.Log("    Paged memory size: {0} bytes", currentProcess.PagedMemorySize64);
	}

	private static string ProcessName(string name)
	{
		int num = name.LastIndexOf(".exe");
		if (-1 != num)
		{
			return name.Substring(0, num);
		}
		return name;
	}

	private static bool ProcessIsRunningInner(string name)
	{
		name = ProcessName(name);
		Process[] processesByName = Process.GetProcessesByName(name);
		if (processesByName.Length != 0)
		{
			return true;
		}
		return false;
	}
}
