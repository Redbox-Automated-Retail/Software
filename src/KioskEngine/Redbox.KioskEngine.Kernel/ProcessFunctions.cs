using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Principal;
using System.Windows.Forms;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.IDE;
using Redbox.Lua;

namespace Redbox.KioskEngine.Kernel;

internal static class ProcessFunctions
{
	private static char[] AdminPassword = new char[10] { 'W', 'i', 'A', 'M', 's', 'o', 'c', 'a', 'P', '?' };

	[KernelFunction(Name = "kernel.startprocess")]
	internal static int LaunchProcess(string processName, string args, bool hiddenProcess)
	{
		string defaultAdmin = "rbadmin";
		try
		{
			if (new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
			{
				LogHelper.Instance.Log($"Launch {processName} {args} as user");
				return LaunchProcessInner(processName, args, null, useAdminPassword: false, hiddenProcess);
			}
			List<string> allAdministrators = GetAllAdministrators();
			if (allAdministrators.Count() < 1)
			{
				LogHelper.Instance.Log("No Admin Accounts Dectected - Launching with reduced privileges.");
				return LaunchProcessInner(processName, args, null, useAdminPassword: false, hiddenProcess);
			}
			if (allAdministrators.Find((string x) => x.ToLower() == defaultAdmin) != null)
			{
				LogHelper.Instance.Log($"Launch {processName} {args} as {defaultAdmin}");
				return LaunchProcessInner(processName, args, defaultAdmin, useAdminPassword: true, hiddenProcess);
			}
			LogHelper.Instance.Log($"Launch {processName} {args} as {allAdministrators[0]}");
			return LaunchProcessInner(processName, args, allAdministrators[0], useAdminPassword: false, hiddenProcess);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Error in Kernel Function LaunchProcess.", e);
		}
		return -1;
	}

	[KernelFunction(Name = "kernel.startprocesswaitforexit")]
	internal static int StartProcessWaitForExit(string process, string arguments, int timeout)
	{
		try
		{
			ProcessStartInfo startInfo = new ProcessStartInfo
			{
				FileName = process,
				Arguments = arguments,
				WindowStyle = ProcessWindowStyle.Hidden,
				CreateNoWindow = true
			};
			Process process2 = new Process
			{
				StartInfo = startInfo
			};
			LogHelper.Instance.Log("Launching {0}, args={1}, timeout={2}.", process, arguments, timeout);
			process2.Start();
			process2.WaitForExit(timeout);
			if (process2.HasExited)
			{
				LogHelper.Instance.Log("Launched successfully, ExitCode={0}.", process2.ExitCode);
				return process2.ExitCode;
			}
			LogHelper.Instance.Log("HasExited=false, possible timeout.");
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in ProcessFunctions.StartProcessWaitForExit.", e);
		}
		return -1;
	}

	[KernelFunction(Name = "kernel.killprocessbyid")]
	internal static void KillProcessById(int processId)
	{
		try
		{
			LaunchProcess("TaskKill.exe", $"/f /pid {processId}", hiddenProcess: true);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Error in Kernel Function KillProcessId.", e);
		}
	}

	[KernelFunction(Name = "kernel.starthiddenprocess")]
	internal static int LaunchHiddenProcess(string processName, string args)
	{
		try
		{
			LaunchProcess(processName, args, hiddenProcess: true);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Error in Kernel Function LaunchProcess.", e);
		}
		return -1;
	}

	[KernelFunction(Name = "kernel.listrunningprocesses")]
	internal static LuaTable ListRunningProcesses()
	{
		int num = 0;
		LuaTable luaTable = new LuaTable(KernelService.Instance.LuaRuntime);
		Process[] processes = Process.GetProcesses();
		foreach (Process process in processes)
		{
			LuaTable luaTable2 = new LuaTable(KernelService.Instance.LuaRuntime);
			luaTable2["id"] = process.Id;
			luaTable2["name"] = process.ProcessName;
			luaTable2["memory"] = $"{process.WorkingSet64 / 1000:#,###}K";
			luaTable[num] = luaTable2;
			num++;
		}
		return luaTable;
	}

	[KernelFunction(Name = "kernel.listpidrundll32s")]
	internal static LuaTable ListRunningRunDLL32Processes()
	{
		int num = 0;
		LuaTable luaTable = new LuaTable(KernelService.Instance.LuaRuntime);
		Process[] processes = Process.GetProcesses();
		foreach (Process process in processes)
		{
			if (string.CompareOrdinal(process.ProcessName.ToLower(), 0, "rundll32", 0, "rundll32".Length) == 0)
			{
				luaTable[num++] = process.Id;
			}
		}
		return luaTable;
	}

	[KernelFunction(Name = "kernel.showtaskmanager")]
	internal static void ShowTaskManager(LuaTable processNamesToIgnore, KillHandler killHandler)
	{
		TaskManagerForm form = new TaskManagerForm();
		if (processNamesToIgnore != null)
		{
			form.ProcessNamesToIgnore = processNamesToIgnore.ToList<string>();
		}
		form.Kill += delegate
		{
			LuaTable luaTable = new LuaTable(KernelService.Instance.LuaRuntime);
			int num = 1;
			foreach (Process selectedProcess in form.SelectedProcesses)
			{
				LuaTable value = new LuaTable(KernelService.Instance.LuaRuntime)
				{
					["pid"] = selectedProcess.Id,
					["window_title"] = selectedProcess.MainWindowTitle,
					["process_name"] = selectedProcess.ProcessName,
					["start_time"] = selectedProcess.StartTime.ToString(),
					["memory"] = $"{selectedProcess.WorkingSet64 / 1000:#,###}K"
				};
				luaTable[num++] = value;
			}
			if (killHandler != null && killHandler(luaTable))
			{
				form.RefreshList();
			}
		};
		form.Show();
	}

	[KernelFunction(Name = "kernel.showtaskrunner")]
	internal static void ShowTaskRunner(LuaFunction runProcessHandler)
	{
		TaskRunnerForm form = new TaskRunnerForm();
		form.Run += delegate
		{
			if (runProcessHandler != null)
			{
				runProcessHandler.Call(form.ProcessToRun);
			}
		};
		form.Show();
	}

	internal static void Reset()
	{
		Application.OpenForms["TaskManagerForm"]?.Close();
		Application.OpenForms["TaskRunnerForm"]?.Close();
	}

	private static int LaunchProcessInner(string programPath, string args, string userName, bool useAdminPassword, bool hiddenProcess)
	{
		try
		{
			ProcessStartInfo processStartInfo = new ProcessStartInfo
			{
				FileName = programPath,
				Arguments = args,
				WindowStyle = (hiddenProcess ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal),
				CreateNoWindow = hiddenProcess
			};
			if (useAdminPassword)
			{
				processStartInfo.UserName = userName;
				SecureString pass = new SecureString();
				AdminPassword.ForEach(delegate(char x)
				{
					pass.AppendChar(x);
				});
				processStartInfo.Password = pass;
				processStartInfo.UseShellExecute = false;
				if (string.IsNullOrEmpty(Path.GetExtension(processStartInfo.FileName)))
				{
					processStartInfo.FileName += ".exe";
				}
				if (!File.Exists(Path.Combine(processStartInfo.WorkingDirectory, processStartInfo.FileName)))
				{
					string text = Environment.GetEnvironmentVariable("SystemRoot");
					if (string.IsNullOrEmpty(text))
					{
						text = "c:\\windows";
					}
					string text2 = Path.Combine(text, processStartInfo.FileName);
					if (File.Exists(text2))
					{
						processStartInfo.FileName = text2;
					}
				}
			}
			Process process = Process.Start(processStartInfo);
			if (process != null)
			{
				return process.Id;
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Error in Kernel Function LaunchProcess.", e);
		}
		return -1;
	}

	private static List<string> GetAllAdministrators()
	{
		List<string> list = new List<string>();
		DirectoryEntry directoryEntry = new DirectoryEntry("WinNT://" + Environment.MachineName).Children.Find("administrators", "group");
		try
		{
			foreach (object item in (IEnumerable)directoryEntry.Invoke("members", null))
			{
				DirectoryEntry directoryEntry2 = new DirectoryEntry(item);
				if (directoryEntry2.SchemaClassName == "User")
				{
					list.Add(directoryEntry2.Name);
				}
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Failed to get list of administrators", e, LogEntryType.Error);
		}
		return list.Distinct().ToList();
	}
}
