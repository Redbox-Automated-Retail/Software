using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Redbox.HAL.IPC.Framework;

public class NamedPipeServer
{
	private struct SecurityDescriptor
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

	private const int PIPE_FLAGS_OVERLAPPED = 1073741824;

	private const int PIPE_ACCESS_OUTBOUND = 2;

	private const int PIPE_ACCESS_DUPLEX = 3;

	private const int PIPE_ACCESS_INBOUND = 1;

	private const int PIPE_WAIT = 0;

	private const int PIPE_NOWAIT = 1;

	private const int PIPE_READMODE_BYTE = 0;

	private const int PIPE_READMODE_MESSAGE = 2;

	private const int PIPE_TYPE_BYTE = 0;

	private const int PIPE_TYPE_MESSAGE = 4;

	private const int PIPE_CLIENT_END = 0;

	private const int PIPE_SERVER_END = 1;

	private const int PIPE_UNLIMITED_INSTANCES = 255;

	private const uint NMPWAIT_WAIT_FOREVER = uint.MaxValue;

	private const uint NMPWAIT_NOWAIT = 1u;

	private const uint NMPWAIT_USE_DEFAULT_WAIT = 0u;

	private const uint GENERIC_READ = 2147483648u;

	private const uint GENERIC_WRITE = 1073741824u;

	private const uint GENERIC_EXECUTE = 536870912u;

	private const uint GENERIC_ALL = 268435456u;

	private const int CREATE_NEW = 1;

	private const int CREATE_ALWAYS = 2;

	private const int OPEN_EXISTING = 3;

	private const int OPEN_ALWAYS = 4;

	private const int TRUNCATE_EXISTING = 5;

	private const uint SECURITY_DESCRIPTOR_REVISION = 1u;

	private static NamedPipeServer m_instance = null;

	private IntPtr ServerOverlappedEvent = IntPtr.Zero;

	private SecurityDescriptor m_securityDescriptor;

	private SecurityAttributes m_securityAttributes;

	private IntPtr m_securityDescriptorPointer = IntPtr.Zero;

	private IntPtr m_securityAttributesPointer = IntPtr.Zero;

	private static IntPtr INVALID_HANDLE_VALUE = (IntPtr)(-1);

	internal string PipeName { get; private set; }

	private bool Disposed { get; set; }

	public static NamedPipeServer CreateServer(string pipeName)
	{
		if (m_instance == null)
		{
			m_instance = new NamedPipeServer(pipeName);
		}
		return m_instance;
	}

	public AbstractNamedPipeServerSession WaitForClientConnect(PipeSessionFactory factory)
	{
		SafeFileHandle safeFileHandle = ConnectPipeInner(out var lastError);
		if (safeFileHandle.IsInvalid)
		{
			return null;
		}
		int num = lastError;
		if (num != 0 && num != 535)
		{
			return null;
		}
		return factory.CreateSession(safeFileHandle);
	}

	~NamedPipeServer()
	{
		Dispose(disposing: false);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	private void Dispose(bool disposing)
	{
		if (!Disposed)
		{
			Marshal.FreeCoTaskMem(m_securityDescriptorPointer);
			Marshal.FreeCoTaskMem(m_securityAttributesPointer);
			Disposed = true;
		}
	}

	private SafeFileHandle ConnectPipeInner(out int lastError)
	{
		lastError = 0;
		SafeFileHandle safeFileHandle = CreateNamedPipe(PipeName, 1073741827u, 0u, 255u, 4096u, 4096u, uint.MaxValue, IntPtr.Zero);
		if (safeFileHandle.IsInvalid)
		{
			return safeFileHandle;
		}
		lastError = ((!ConnectNamedPipe(safeFileHandle, IntPtr.Zero)) ? Marshal.GetLastWin32Error() : 0);
		return safeFileHandle;
	}

	private NamedPipeServer(string pipeName)
	{
		m_securityDescriptor = default(SecurityDescriptor);
		InitializeSecurityDescriptor(ref m_securityDescriptor, 1u);
		SetSecurityDescriptorDacl(ref m_securityDescriptor, daclPresent: true, IntPtr.Zero, daclDefaulted: false);
		m_securityDescriptorPointer = Marshal.AllocCoTaskMem(Marshal.SizeOf(m_securityDescriptor));
		Marshal.StructureToPtr(m_securityDescriptor, m_securityDescriptorPointer, fDeleteOld: false);
		m_securityAttributes = new SecurityAttributes
		{
			nLength = Marshal.SizeOf(m_securityDescriptor),
			lpSecurityDescriptor = m_securityDescriptorPointer,
			bInheritHandle = 1
		};
		m_securityAttributesPointer = Marshal.AllocCoTaskMem(Marshal.SizeOf(m_securityAttributes));
		Marshal.StructureToPtr(m_securityAttributes, m_securityAttributesPointer, fDeleteOld: false);
		if (string.IsNullOrEmpty(pipeName))
		{
			pipeName = "HALService7001";
		}
		PipeName = "\\\\.\\PIPE\\" + pipeName;
	}

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern SafeFileHandle CreateNamedPipe(string pipeName, uint dwOpenMode, uint dwPipeMode, uint nMaxInstances, uint nOutBufferSize, uint nInBufferSize, uint nDefaultTimeOut, IntPtr lpSecurityAttributes);

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern bool ConnectNamedPipe(SafeFileHandle hNamedPipe, IntPtr lpOverlapped);

	[DllImport("advapi32.dll", SetLastError = true)]
	private static extern bool InitializeSecurityDescriptor(ref SecurityDescriptor sd, uint dwRevision);

	[DllImport("advapi32.dll", SetLastError = true)]
	private static extern bool SetSecurityDescriptorDacl(ref SecurityDescriptor sd, bool daclPresent, IntPtr dacl, bool daclDefaulted);
}
