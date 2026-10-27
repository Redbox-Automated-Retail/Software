using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;

namespace Redbox.UpdateManager.KioskUtil;

[PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
public class ShellHelper
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

	public static int StartProcessAsShellUser(string processName)
	{
		return StartProcessAsShellUser(processName, null);
	}

	public static int StartProcessAsShellUser(string processName, string args)
	{
		Process[] processesByName = Process.GetProcessesByName("KioskShell");
		if (processesByName.Length == 0 || processesByName.Length > 1)
		{
			return -1;
		}
		IntPtr tokenHandle = IntPtr.Zero;
		Process process = processesByName[0];
		if (OpenProcessToken(process.Handle, 14, ref tokenHandle) != 0)
		{
			try
			{
				IntPtr intPtr = DuplicateToken(tokenHandle);
				if (intPtr == IntPtr.Zero)
				{
					Console.WriteLine("Unable to to duplicate process token with DuplicateToken." + new Win32Exception(Marshal.GetLastWin32Error()).ToString());
					return -1;
				}
				try
				{
					string directoryName = Path.GetDirectoryName(processName);
					string fileName = Path.GetFileName(processName);
					string text = (string.IsNullOrEmpty(args) ? null : $"{processName} {args}");
					Console.WriteLine($"starting from path: '{processName}' with file: '{fileName}' in directory: '{directoryName}' with command line: '{text}'");
					STARTUPINFO lpStartupInfo = default(STARTUPINFO);
					lpStartupInfo.cb = Marshal.SizeOf(lpStartupInfo);
					lpStartupInfo.lpDesktop = "winsta0\\default";
					if (CreateProcessAsUser(intPtr, processName, text, IntPtr.Zero, IntPtr.Zero, bInheritHandles: false, 16u, IntPtr.Zero, directoryName, ref lpStartupInfo, out var lpProcessInformation))
					{
						return lpProcessInformation.dwProcessId;
					}
					Console.WriteLine("Error Calling CreateProcessWithTokenW" + new Win32Exception(Marshal.GetLastWin32Error()).ToString());
				}
				catch (Exception ex)
				{
					Console.WriteLine("Error in Function StartProcessAsShellUser." + ex.ToString());
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
			Console.WriteLine("Unable to acquire process token via OpenProcessToken." + new Win32Exception(Marshal.GetLastWin32Error()).ToString());
		}
		return -1;
	}

	[DllImport("advapi32", SetLastError = true)]
	[SuppressUnmanagedCodeSecurity]
	private static extern int OpenProcessToken(IntPtr processHandle, int desiredAccess, ref IntPtr tokenHandle);

	[DllImport("kernel32", SetLastError = true)]
	[SuppressUnmanagedCodeSecurity]
	private static extern bool CloseHandle(IntPtr handle);

	[DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern bool CreateProcessAsUser(IntPtr hToken, string lpApplicationName, string lpCommandLine, IntPtr lpProcessAttributes, IntPtr lpThreadAttributes, bool bInheritHandles, uint dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory, ref STARTUPINFO lpStartupInfo, out PROCESS_INFORMATION lpProcessInformation);

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
