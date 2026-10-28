using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using Redbox.HAL.Core;

namespace Redbox.HAL.IPC.Framework;

internal abstract class BasePipeChannel : IDisposable
{
	internal bool Connected { get; set; }

	internal string ID { get; set; }

	protected bool Disposed { get; set; }

	protected SafeFileHandle PipeHandle { get; set; }

	protected uint MessageHeaderSize => (uint)Marshal.SizeOf(typeof(int));

	public void Dispose()
	{
		DisposeChannel(fromDispose: true);
		GC.SuppressFinalize(this);
	}

	internal abstract bool Connect(string pipeName);

	internal bool SendMessage(byte[] message)
	{
		return SendBytes(message);
	}

	internal byte[] ReceiveMessage()
	{
		byte[] array = ReadBytes(MessageHeaderSize);
		if (array == null)
		{
			return null;
		}
		uint count = (uint)BitConverter.ToInt32(array, 0);
		return ReadBytes(count);
	}

	protected abstract bool DisposeChannel(bool fromDispose);

	protected byte[] ReadBytes(uint count)
	{
		if (PipeHandle.IsInvalid)
		{
			return null;
		}
		uint lpNumberOfBytesRead = 0u;
		byte[] array = new byte[count];
		if (!ReadFile(PipeHandle, array, count, out lpNumberOfBytesRead, IntPtr.Zero))
		{
			int lastWin32Error = Marshal.GetLastWin32Error();
			LogHelper.Instance.Log($"PipeClientSession.ReadFile() returned false; error code = {lastWin32Error}", LogEntryType.Error);
			return null;
		}
		if (lpNumberOfBytesRead == count)
		{
			return array;
		}
		return null;
	}

	protected bool SendBytes(byte[] message)
	{
		if (PipeHandle.IsInvalid)
		{
			return false;
		}
		uint writtenBytes = 0u;
		byte[] bytes = BitConverter.GetBytes(message.Length);
		uint messageHeaderSize = MessageHeaderSize;
		if (!SafeWrite(bytes, messageHeaderSize, out writtenBytes))
		{
			return false;
		}
		if (writtenBytes != messageHeaderSize)
		{
			LogHelper.Instance.Log($"SendBytes: written bytes {writtenBytes} != header size {messageHeaderSize}", LogEntryType.Error);
			return false;
		}
		if (SafeWrite(message, (uint)message.Length, out writtenBytes))
		{
			return writtenBytes == message.Length;
		}
		return false;
	}

	protected BasePipeChannel(SafeFileHandle pipeHandle)
	{
		PipeHandle = pipeHandle;
	}

	~BasePipeChannel()
	{
		DisposeChannel(fromDispose: false);
	}

	protected BasePipeChannel()
	{
	}

	[DllImport("kernel32.dll", SetLastError = true)]
	protected static extern SafeFileHandle CreateFile(string pipeName, uint dwDesiredAccess, uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplate);

	[DllImport("kernel32.dll", SetLastError = true)]
	protected static extern bool ReadFile(SafeFileHandle hFile, byte[] lpBuffer, uint nNumberOfBytesToRead, out uint lpNumberOfBytesRead, IntPtr lpOverlapped);

	[DllImport("kernel32.dll", SetLastError = true)]
	protected static extern bool WriteFile(SafeFileHandle hFile, byte[] lpBuffer, uint nNumberOfBytesToWrite, out uint lpNumberOfBytesWritten, IntPtr lpOverlapped);

	[DllImport("kernel32.dll", SetLastError = true)]
	protected static extern bool FlushFileBuffers(SafeFileHandle handle);

	private bool SafeWrite(byte[] bytes, uint msgLen, out uint writtenBytes)
	{
		if (WriteFile(PipeHandle, bytes, msgLen, out writtenBytes, IntPtr.Zero))
		{
			return true;
		}
		int lastWin32Error = Marshal.GetLastWin32Error();
		LogHelper.Instance.Log($"PipeClientSession.SafeWrite() returned false; error code = {lastWin32Error}", LogEntryType.Error);
		return false;
	}
}
