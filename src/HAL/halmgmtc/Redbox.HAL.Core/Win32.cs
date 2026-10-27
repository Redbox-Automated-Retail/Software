using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Redbox.HAL.Core;

public static class Win32
{
	public static class ErrorCodes
	{
		public const int ErrorSuccess = 0;

		public const int ErrorBrokenPipe = 109;

		public const int ErrorPipeBusy = 231;

		public const int ErrorNoData = 232;

		public const int ErrorPipeNotConnected = 233;

		public const int ErrorMoreData = 234;

		public const int ErrorPipeConnected = 535;

		public const int ErrorPipeListening = 536;
	}

	[Flags]
	public enum AccessFlags : uint
	{
		NONE = 0u,
		GENERIC_READ = 0x80000000u,
		GENERIC_WRITE = 0x40000000u,
		GENERIC_READ_WRITE = GENERIC_READ | GENERIC_WRITE,
		GENERIC_EXECUTE = 0x20000000u,
		GENERIC_ALL = 0x10000000u
	}

	[Flags]
	public enum ShareFlags : uint
	{
		NONE = 0u,
		FILE_SHARE_READ = 1u,
		FILE_SHARE_WRITE = 2u,
		FILE_SHARE_READ_WRITE = FILE_SHARE_READ | FILE_SHARE_WRITE,
		FILE_SHARE_DELETE = 4u
	}

	public struct SECURITY_ATTRIBUTES
	{
		public int nLength;

		public int lpSecurityDescriptor;

		public int bInheritHandle;
	}

	public struct SecurityDescriptor
	{
		public byte revision;

		public byte size;

		public short control;

		public IntPtr owner;

		public IntPtr group;

		public IntPtr sacl;

		public IntPtr dacl;
	}

	public struct SecurityAttributes
	{
		public int nLength;

		public IntPtr lpSecurityDescriptor;

		public int bInheritHandle;
	}

	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern SafeFileHandle CreateFile(string pipeName, AccessFlags dwDesiredAccess, ShareFlags dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplate);

	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool WriteFile(SafeFileHandle hFile, byte[] lpBuffer, int nNumberOfBytesToWrite, out int lpNumberOfBytesWritten, IntPtr lpOverlapped);

	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool ReadFile(SafeFileHandle hFile, byte[] lpBuffer, int nNumberOfBytesToRead, out int lpNumberOfBytesRead, IntPtr lpOverlapped);

	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool FlushFileBuffers(SafeFileHandle handle);

	[DllImport("advapi32.dll", SetLastError = true)]
	public static extern bool InitializeSecurityDescriptor(ref SecurityDescriptor sd, uint dwRevision);

	[DllImport("advapi32.dll", SetLastError = true)]
	public static extern bool SetSecurityDescriptorDacl(ref SecurityDescriptor sd, bool daclPresent, IntPtr dacl, bool daclDefaulted);
}
