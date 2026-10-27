using System.Runtime.InteropServices;

namespace UpdateClientService.API.Services.Kernel;

public class TokenPrivilegesAccess
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct LUID
	{
		internal uint LowPart;

		internal uint HighPart;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct LUID_AND_ATTRIBUTES
	{
		internal LUID Luid;

		internal uint Attributes;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct TOKEN_PRIVILEGE
	{
		internal uint PrivilegeCount;

		internal LUID_AND_ATTRIBUTES Privilege;
	}

	public const int TOKEN_ASSIGN_PRIMARY = 1;

	public const int TOKEN_DUPLICATE = 2;

	public const int TOKEN_IMPERSONATE = 4;

	public const int TOKEN_QUERY = 8;

	public const int TOKEN_QUERY_SOURCE = 16;

	public const int TOKEN_ADJUST_PRIVILEGES = 32;

	public const int TOKEN_ADJUST_GROUPS = 64;

	public const int TOKEN_ADJUST_DEFAULT = 128;

	public const uint SE_PRIVILEGE_ENABLED_BY_DEFAULT = 1u;

	public const uint SE_PRIVILEGE_ENABLED = 2u;

	public const uint SE_PRIVILEGE_REMOVED = 4u;

	public const uint SE_PRIVILEGE_USED_FOR_ACCESS = 2147483648u;

	[DllImport("advapi32.dll", CharSet = CharSet.Auto)]
	public static extern int OpenProcessToken(int ProcessHandle, int DesiredAccess, ref int tokenhandle);

	[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
	public static extern int GetCurrentProcess();

	[DllImport("advapi32.dll", CharSet = CharSet.Auto)]
	public static extern int LookupPrivilegeValue(string lpsystemname, string lpname, [MarshalAs(UnmanagedType.Struct)] ref LUID lpLuid);

	[DllImport("advapi32.dll", CharSet = CharSet.Auto)]
	public static extern int AdjustTokenPrivileges(int tokenhandle, int disableprivs, [MarshalAs(UnmanagedType.Struct)] ref TOKEN_PRIVILEGE Newstate, int bufferlength, int PreivousState, int Returnlength);

	public static void EnablePrivilege(string privilege)
	{
		int tokenhandle = 0;
		int num = 0;
		TOKEN_PRIVILEGE Newstate = default(TOKEN_PRIVILEGE);
		LUID lpLuid = default(LUID);
		num = OpenProcessToken(GetCurrentProcess(), 40, ref tokenhandle);
		num = LookupPrivilegeValue(null, privilege, ref lpLuid);
		Newstate.PrivilegeCount = 1u;
		Newstate.Privilege = new LUID_AND_ATTRIBUTES
		{
			Attributes = 2u,
			Luid = lpLuid
		};
		num = AdjustTokenPrivileges(tokenhandle, 0, ref Newstate, 1024, 0, 0);
	}

	public static void DisablePrivilege(string privilege)
	{
		int tokenhandle = 0;
		int num = 0;
		TOKEN_PRIVILEGE Newstate = default(TOKEN_PRIVILEGE);
		LUID lpLuid = default(LUID);
		num = OpenProcessToken(GetCurrentProcess(), 40, ref tokenhandle);
		num = LookupPrivilegeValue(null, privilege, ref lpLuid);
		Newstate.PrivilegeCount = 1u;
		Newstate.Privilege = new LUID_AND_ATTRIBUTES
		{
			Luid = lpLuid
		};
		num = AdjustTokenPrivileges(tokenhandle, 0, ref Newstate, 1024, 0, 0);
	}
}
