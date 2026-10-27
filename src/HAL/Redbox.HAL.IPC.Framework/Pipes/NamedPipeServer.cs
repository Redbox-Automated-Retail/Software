using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Interop;

namespace Redbox.HAL.IPC.Framework.Pipes;

public sealed class NamedPipeServer : IDisposable
{
	private bool Disposed;

	private readonly string PipeName;

	private readonly string PipePath;

	private readonly Win32.SecurityDescriptor m_securityDescriptor;

	private readonly Win32.SecurityAttributes m_securityAttributes;

	private readonly IntPtr m_securityDescriptorPointer = IntPtr.Zero;

	private readonly IntPtr m_securityAttributesPointer = IntPtr.Zero;

	private readonly int BufferSize = 4096;

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

	private static IntPtr INVALID_HANDLE_VALUE = (IntPtr)(-1);

	private const uint SECURITY_DESCRIPTOR_REVISION = 1u;

	public static NamedPipeServer Create(string pipeName)
	{
		return new NamedPipeServer(pipeName);
	}

	public BasePipeChannel WaitForClientConnect()
	{
		int num = 0;
		SafeFileHandle safeFileHandle = CreateNamedPipe(PipePath, 3u, 0u, 255u, (uint)BufferSize, (uint)BufferSize, uint.MaxValue, IntPtr.Zero);
		if (safeFileHandle.IsInvalid)
		{
			return null;
		}
		num = ((!ConnectNamedPipe(safeFileHandle, IntPtr.Zero)) ? Marshal.GetLastWin32Error() : 0);
		if (535 == num || num == 0)
		{
			return ServerPipeChannel.Make(safeFileHandle);
		}
		return null;
	}

	~NamedPipeServer()
	{
		OnDispose(disposing: false);
	}

	public void Dispose()
	{
		OnDispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	private void OnDispose(bool disposing)
	{
		if (!Disposed)
		{
			Marshal.FreeCoTaskMem(m_securityDescriptorPointer);
			Marshal.FreeCoTaskMem(m_securityAttributesPointer);
			Disposed = true;
			LogHelper.Instance.Log("[NamedPipeServer] OnDispose");
		}
	}

	private NamedPipeServer(string pipeName)
	{
		m_securityDescriptor = default(Win32.SecurityDescriptor);
		Win32.InitializeSecurityDescriptor(ref m_securityDescriptor, 1u);
		Win32.SetSecurityDescriptorDacl(ref m_securityDescriptor, daclPresent: true, IntPtr.Zero, daclDefaulted: false);
		m_securityDescriptorPointer = Marshal.AllocCoTaskMem(Marshal.SizeOf(m_securityDescriptor));
		Marshal.StructureToPtr(m_securityDescriptor, m_securityDescriptorPointer, fDeleteOld: false);
		m_securityAttributes = new Win32.SecurityAttributes
		{
			nLength = Marshal.SizeOf(m_securityDescriptor),
			lpSecurityDescriptor = m_securityDescriptorPointer,
			bInheritHandle = 1
		};
		m_securityAttributesPointer = Marshal.AllocCoTaskMem(Marshal.SizeOf(m_securityAttributes));
		Marshal.StructureToPtr(m_securityAttributes, m_securityAttributesPointer, fDeleteOld: false);
		PipeName = pipeName;
		PipePath = "\\\\.\\PIPE\\" + PipeName;
	}

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern SafeFileHandle CreateNamedPipe(string pipeName, uint dwOpenMode, uint dwPipeMode, uint nMaxInstances, uint nOutBufferSize, uint nInBufferSize, uint nDefaultTimeOut, IntPtr lpSecurityAttributes);

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern bool ConnectNamedPipe(SafeFileHandle hNamedPipe, IntPtr lpOverlapped);
}
