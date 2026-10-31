using System;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace Redbox.Installer.Framework;

internal static class NativeMethods
{
	public static class PROCESSOR_ARCHITECTURE
	{
		public const ushort INTEL = 0;

		public const ushort IA64 = 6;

		public const ushort AMD64 = 9;

		public const ushort UNKNOWN = ushort.MaxValue;
	}

	public struct SYSTEM_INFO
	{
		public ushort processorArchitecture;

		private ushort reserved;

		public uint pageSize;

		public IntPtr minimumApplicationAddress;

		public IntPtr maximumApplicationAddress;

		public IntPtr activeProcessorMask;

		public uint numberOfProcessors;

		public uint processorType;

		public uint allocationGranularity;

		public ushort processorLevel;

		public ushort processorRevision;
	}

	[Flags]
	public enum RegOption
	{
		NonVolatile = 0,
		Volatile = 1,
		CreateLink = 2,
		BackupRestore = 4,
		OpenLink = 8
	}

	[Flags]
	public enum RegSAM
	{
		QueryValue = 1,
		SetValue = 2,
		CreateSubKey = 4,
		EnumerateSubKeys = 8,
		Notify = 0x10,
		CreateLink = 0x20,
		WOW64_32Key = 0x200,
		WOW64_64Key = 0x100,
		WOW64_Res = WOW64_32Key | WOW64_64Key,
		Read = 0x20019,
		Write = 0x20006,
		Execute = Read,
		AllAccess = 0xF003F
	}

	public enum RegResult
	{
		CreatedNewKey = 1,
		OpenedExistingKey
	}

	[StructLayout(LayoutKind.Sequential)]
	public class SECURITY_ATTRIBUTES
	{
		public int nLength;

		public IntPtr lpSecurityDescriptor;

		public int bInheritHandle;
	}

	public const int ERROR_SUCCESS = 0;

	public const int ERROR_FILE_NOT_FOUND = 2;

	public const int ERROR_BADKEY = 1010;

	public static Architecture ToArchitecture(this SYSTEM_INFO si)
	{
		switch (si.processorArchitecture)
		{
		case 0:
			return Architecture.x86;
		case 6:
		case 9:
			return Architecture.x64;
		default:
			throw new ApplicationException("Unknown PROCESSOR_ARCHITECTURE value: '" + si.processorArchitecture + "' found during call to GetNativeSystemInfo!");
		}
	}

	[DllImport("kernel32.dll")]
	internal static extern void GetNativeSystemInfo(ref SYSTEM_INFO lpSystemInfo);

	[DllImport("advapi32.dll", SetLastError = true)]
	public static extern int RegCreateKeyEx(RegistryHive hKey, string lpSubKey, int Reserved, string lpClass, RegOption dwOptions, RegSAM samDesired, SECURITY_ATTRIBUTES lpSecurityAttributes, out UIntPtr phkResult, out RegResult lpdwDisposition);

	[DllImport("advapi32.dll", SetLastError = true)]
	public static extern int RegDeleteKeyEx(RegistryHive root, string subKey, RegSAM samDesired, uint Reserved);

	[DllImport("advapi32.dll", SetLastError = true)]
	public static extern int RegDeleteKey(RegistryHive hKey, string subkey);

	[DllImport("advapi32.dll", CharSet = CharSet.Unicode)]
	public static extern int RegOpenKeyEx(RegistryHive hKey, string subKey, uint options, RegSAM sam, out UIntPtr phkResult);

	[DllImport("advapi32.dll", SetLastError = true)]
	public static extern int RegCloseKey(UIntPtr hKey);

	[DllImport("advapi32.dll", SetLastError = true)]
	public static extern int RegSetValueEx(UIntPtr key, [MarshalAs(UnmanagedType.LPStr)] string lpValueName, int Reserved, RegistryValueKind dwType, byte[] lpData, int cbData);

	[DllImport("advapi32.dll", SetLastError = true)]
	public static extern int RegDeleteKeyValue(RegistryHive hKey, [MarshalAs(UnmanagedType.LPStr)] string subkey, [MarshalAs(UnmanagedType.LPStr)] string name);

	[DllImport("advapi32.dll", SetLastError = true)]
	public static extern int RegDeleteValue(UIntPtr key, [MarshalAs(UnmanagedType.LPStr)] string value);

	[DllImport("advapi32.dll", SetLastError = true)]
	public static extern int RegQueryInfoKey(UIntPtr key, out string lpClass, out uint lpcbClass, IntPtr lpReserved, out uint lpcSubKeys, out uint lpcbMaxSubKeyLen, out uint lpcbMaxClassLen, out uint lpcValues, out uint lpcbMaxValueNameLen, out uint lpcbMaxValueLen, out uint lpcbSecurityDescriptor, IntPtr lpftLastWriteTime);
}
