using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.Kernel;

[PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
public static class ProcessFunctions
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	private struct STARTUPINFO
	{
		public int cb;

		public string lpReserved;

		public string lpDesktop;

		public string lpTitle;

		public int dwX;

		public int dwY;

		public int dwXSize;

		public int dwYSize;

		public int dwXCountChars;

		public int dwYCountChars;

		public int dwFillAttribute;

		public int dwFlags;

		public short wShowWindow;

		public short cbReserved2;

		public IntPtr lpReserved2;

		public IntPtr hStdInput;

		public IntPtr hStdOutput;

		public IntPtr hStdError;
	}

	private enum SECURITY_IMPERSONATION_LEVEL
	{
		Anonymous,
		Identifcation,
		Impersonation,
		Delegation
	}

	private enum TOKEN_TYPE
	{
		Primary = 1,
		Impersonation
	}

	private struct PROCESS_INFORMATION
	{
		public IntPtr hProcess;

		public IntPtr hThread;

		public int dwProcessId;

		public int dwThreadId;
	}

	private const int TOKEN_DUPLICATE = 2;

	private const int TOKEN_QUERY = 8;

	private const int TOKEN_IMPERSONATE = 4;

	private const uint GRANTED_ALL = 268435456u;

	private const uint CREATE_NEW_CONSOLE = 16u;

	[KernelFunction(Name = "kernel.startprocessasshelluser")]
	internal static int LaunchProcessAsShellUser(string processName, string args)
	{
		return StartProcessAsShellUser(processName, args);
	}

	[KernelFunction(Name = "kernel.processtrimworkingset")]
	internal static void TrimWorkingSet(string name)
	{
		Process process = Process.GetProcesses().FirstOrDefault((Process p) => string.Compare(p.ProcessName, name, ignoreCase: true) == 0);
		if (process != null)
		{
			SetProcessWorkingSetSize(process.Handle, -1, -1);
		}
	}

	[KernelFunction(Name = "kernel.killprocessbyname")]
	public static void KillProcessByName(string name)
	{
		Process.GetProcesses().FirstOrDefault((Process p) => string.Compare(p.ProcessName, name, ignoreCase: true) == 0)?.Kill();
	}

	[KernelFunction(Name = "kernel.isprocessrunning")]
	internal static bool IsProcessRunning(string name)
	{
		return Process.GetProcessesByName(name).Length != 0;
	}

	[KernelFunction(Name = "kernel.executestring")]
	internal static int Execute(string process, string arguments, string workingDirectory, object timeout)
	{
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		ProcessStartInfo startInfo = new ProcessStartInfo
		{
			CreateNoWindow = true,
			UseShellExecute = false,
			WindowStyle = ProcessWindowStyle.Hidden,
			FileName = service.ExpandProperties(process),
			Arguments = service.ExpandProperties(arguments),
			WorkingDirectory = service.ExpandProperties(workingDirectory)
		};
		Process process2 = new Process
		{
			StartInfo = startInfo
		};
		process2.Start();
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
			LogHelper.Instance.Log("Entering bounded WaitForExit in ProcessFunctions.Execute with a timeout of {0}.", num.Value);
			process2.WaitForExit(num.Value);
		}
		else
		{
			LogHelper.Instance.Log("Entering unbounded WaitForExit in ProcessFunctions.Execute.");
			process2.WaitForExit();
		}
		if (process2.HasExited)
		{
			return process2.ExitCode;
		}
		return -1;
	}

	[DllImport("Kernel32.dll")]
	private static extern bool SetProcessWorkingSetSize(IntPtr hProcess, int dwMinimumWorkingSetSize, int dwMaximumWorkingSetSize);

	public static int StartProcessAsShellUser(string processName, string args)
	{
		Process[] processesByName = Process.GetProcessesByName("KioskShell");
		if (processesByName.Length == 0 || processesByName.Length > 1)
		{
			return -1;
		}
		IntPtr tokenHandle = IntPtr.Zero;
		if (OpenProcessToken(processesByName[0].Handle, 14, ref tokenHandle) != 0)
		{
			try
			{
				IntPtr intPtr = DuplicateToken(tokenHandle);
				if (intPtr == IntPtr.Zero)
				{
					LogHelper.Instance.Log("Unable to to duplicate process token with DuplicateToken.", new Win32Exception(Marshal.GetLastWin32Error()));
					return -1;
				}
				try
				{
					string directoryName = Path.GetDirectoryName(processName);
					string fileName = Path.GetFileName(processName);
					string text = (string.IsNullOrEmpty(args) ? null : $"{processName} {args}");
					LogHelper.Instance.Log("starting from path: '{0}' with file: '{1}' in directory: '{2}' with command line: '{3}'", processName, fileName, directoryName, text);
					STARTUPINFO lpStartupInfo = default(STARTUPINFO);
					lpStartupInfo.cb = Marshal.SizeOf(lpStartupInfo);
					lpStartupInfo.lpDesktop = "winsta0\\default";
					if (CreateProcessWithTokenW(intPtr, 0u, processName, text, 16u, IntPtr.Zero, directoryName, ref lpStartupInfo, out var lpProcessInformation))
					{
						return lpProcessInformation.dwProcessId;
					}
					LogHelper.Instance.Log("Error Calling CreateProcessWithTokenW", new Win32Exception(Marshal.GetLastWin32Error()));
				}
				catch (Exception e)
				{
					LogHelper.Instance.Log("Error in Function StartProcessAsShellUser.", e);
				}
				finally
				{
					CloseHandle(intPtr);
				}
			}
			finally
			{
				CloseHandle(tokenHandle);
			}
		}
		else
		{
			LogHelper.Instance.Log("Unable to acquire process token via OpenProcessToken.", new Win32Exception(Marshal.GetLastWin32Error()));
		}
		return -1;
	}

	[DllImport("advapi32", SetLastError = true)]
	[SuppressUnmanagedCodeSecurity]
	private static extern int OpenProcessToken(IntPtr processHandle, int desiredAccess, ref IntPtr tokenHandle);

	[DllImport("kernel32", SetLastError = true)]
	[SuppressUnmanagedCodeSecurity]
	private static extern bool CloseHandle(IntPtr handle);

	[DllImport("advapi32", CharSet = CharSet.Unicode, SetLastError = true)]
	private static extern bool CreateProcessWithTokenW(IntPtr hToken, uint dwLogonFlags, string lpApplicationName, string lpCommandLine, uint dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory, [In] ref STARTUPINFO lpStartupInfo, out PROCESS_INFORMATION lpProcessInformation);

	private static IntPtr DuplicateToken(IntPtr sourceToken)
	{
		if (!DuplicateTokenEx(sourceToken, 268435456u, IntPtr.Zero, SECURITY_IMPERSONATION_LEVEL.Impersonation, TOKEN_TYPE.Primary, out var phNewToken))
		{
			return IntPtr.Zero;
		}
		return phNewToken;
	}

	[DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern bool DuplicateTokenEx(IntPtr hExistingToken, uint dwDesiredAccess, IntPtr lpTokenAttributes, SECURITY_IMPERSONATION_LEVEL ImpersonationLevel, TOKEN_TYPE TokenType, out IntPtr phNewToken);
}
