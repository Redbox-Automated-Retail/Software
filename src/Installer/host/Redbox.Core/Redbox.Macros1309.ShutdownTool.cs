using System;
using System.Runtime.InteropServices;

namespace Redbox.Core;

internal static class Redbox_002EMacros1309_002EShutdownTool
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	private struct TokPriv1Luid
	{
		public int Count;

		public long Luid;

		public int Attr;
	}

	private const int SE_PRIVILEGE_ENABLED = 2;

	private const int TOKEN_QUERY = 8;

	private const int TOKEN_ADJUST_PRIVILEGES = 32;

	private const string SE_SHUTDOWN_NAME = "SeShutdownPrivilege";

	public static void Shutdown(Redbox_002EMacros1309_002EShutdownFlags flags, Redbox_002EMacros1309_002EShutdownReason reason)
	{
		IntPtr currentProcess = GetCurrentProcess();
		IntPtr phtok = IntPtr.Zero;
		OpenProcessToken(currentProcess, 40, ref phtok);
		TokPriv1Luid newst = default(TokPriv1Luid);
		newst.Count = 1;
		newst.Luid = 0L;
		newst.Attr = 2;
		LookupPrivilegeValue(null, "SeShutdownPrivilege", ref newst.Luid);
		AdjustTokenPrivileges(phtok, disall: false, ref newst, 0, IntPtr.Zero, IntPtr.Zero);
		ExitWindowsEx(flags, reason);
	}

	[DllImport("user32.dll")]
	private static extern bool ExitWindowsEx(Redbox_002EMacros1309_002EShutdownFlags uFlags, Redbox_002EMacros1309_002EShutdownReason dwReason);

	[DllImport("advapi32.dll", SetLastError = true)]
	private static extern bool LookupPrivilegeValue(string host, string name, ref long pluid);

	[DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
	private static extern bool AdjustTokenPrivileges(IntPtr htok, bool disall, ref TokPriv1Luid newst, int len, IntPtr prev, IntPtr relen);

	[DllImport("kernel32.dll", ExactSpelling = true)]
	private static extern IntPtr GetCurrentProcess();

	[DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
	private static extern bool OpenProcessToken(IntPtr h, int acc, ref IntPtr phtok);
}
