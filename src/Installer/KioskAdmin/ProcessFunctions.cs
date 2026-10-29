using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Permissions;
using System.Threading;

namespace KioskAdmin;

[PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
internal static class ProcessFunctions
{
	public static void KillProcessByName(string name)
	{
		try
		{
			GetProcessByName(name)?.Kill();
		}
		catch
		{
		}
	}

	public static bool IsProcessRunning(string name)
	{
		try
		{
			Process processByName = GetProcessByName(name);
			return processByName != null;
		}
		catch
		{
			return false;
		}
	}

	public static int Execute(string process, string arguments, string workingDirectory, object timeout)
	{
		try
		{
			ProcessStartInfo processStartInfo = new ProcessStartInfo();
			processStartInfo.CreateNoWindow = true;
			processStartInfo.UseShellExecute = false;
			processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			processStartInfo.FileName = process;
			processStartInfo.Arguments = arguments;
			processStartInfo.WorkingDirectory = workingDirectory;
			ProcessStartInfo startInfo = processStartInfo;
			Process process2 = new Process();
			process2.StartInfo = startInfo;
			Process process3 = process2;
			process3.Start();
			int? num = null;
			if (timeout != null)
			{
				try
				{
					num = Convert.ToInt32(timeout);
				}
				catch
				{
				}
			}
			if (num.HasValue)
			{
				process3.WaitForExit(num.Value);
			}
			else
			{
				process3.WaitForExit();
			}
			if (process3.HasExited)
			{
				return process3.ExitCode;
			}
			return -1;
		}
		catch
		{
			return -1;
		}
	}

	public static bool IsProcessRunning(int processId)
	{
		try
		{
			Process[] processes = Process.GetProcesses();
			if (processes.Where((Process item) => item.Id == processId).FirstOrDefault() == null)
			{
				return false;
			}
			return true;
		}
		catch
		{
			return false;
		}
	}

	public static void WaitForProcess(string name, TimeSpan timeout)
	{
		try
		{
			Process processByName = GetProcessByName(name);
			if (processByName != null)
			{
				DateTime now = DateTime.Now;
				while (IsProcessRunning(processByName.Id) && DateTime.Now - now < timeout)
				{
					Thread.Sleep(new TimeSpan(0, 0, 5));
				}
			}
		}
		catch
		{
		}
	}

	public static Process GetProcessByName(string name)
	{
		try
		{
			Process[] processes = Process.GetProcesses();
			return processes.FirstOrDefault((Process p) => string.Compare(p.ProcessName, name, ignoreCase: true) == 0);
		}
		catch
		{
			return null;
		}
	}

	public static void WaitForProcess(int processId, TimeSpan timeout)
	{
		try
		{
			DateTime now = DateTime.Now;
			while (IsProcessRunning(processId) && DateTime.Now - now < timeout)
			{
				Thread.Sleep(new TimeSpan(0, 0, 5));
			}
		}
		catch
		{
		}
	}
}
