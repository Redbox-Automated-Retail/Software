using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Interop;

namespace Redbox.HAL.IPC.Framework.Pipes;

public abstract class BasePipeChannel : IIPCChannel, IDisposable
{
	protected readonly string ID;

	// Set by the constructor that takes a handle, or by the client channel's OnConnect.
	protected SafeFileHandle PipeHandle = null!;

	private bool Disposed;

	private readonly int MessageHeaderSize = Marshal.SizeOf(typeof(int));

	private readonly bool FlushOnWrite;

	public bool IsConnected { get; protected set; }

	public void Dispose()
	{
		DisposeChannel(fromDispose: true);
		GC.SuppressFinalize(this);
	}

	public bool Disconnect()
	{
		if (IsConnected)
		{
			IsConnected = false;
			return OnDisconnect();
		}
		return false;
	}

	public bool Connect()
	{
		return OnConnect();
	}

	public bool Write(byte[] message)
	{
		if (PipeHandle.IsInvalid)
		{
			return false;
		}
		int lpNumberOfBytesWritten = 0;
		byte[] bytes = BitConverter.GetBytes(message.Length);
		byte[] buffer = new byte[bytes.Length + message.Length];
		Array.Copy(bytes, buffer, bytes.Length);
		Array.Copy(message, 0, buffer, bytes.Length, message.Length);
		if (!Win32.WriteFile(PipeHandle, buffer, buffer.Length, out lpNumberOfBytesWritten, IntPtr.Zero))
		{
			int lastWin32Error = Marshal.GetLastWin32Error();
			LogHelper.Instance.Log($"[BasePipeChannel] Write failed: GetLastError() = {lastWin32Error}", LogEntryType.Error);
			return false;
		}
		if (FlushOnWrite)
		{
			FlushFileBuffers(PipeHandle);
		}
		return true;
	}

	public byte[]? Read()
	{
		byte[]? header = ReadBytes(MessageHeaderSize);
		if (header == null || header.Length != MessageHeaderSize)
		{
			return null;
		}
		return ReadBytes(BitConverter.ToInt32(header, 0));
	}

	public byte[]? Read(int timeout)
	{
		return Read();
	}

	public void Read(IIPCResponse response)
	{
		Read(response, 30000);
	}

	public void Read(IIPCResponse response, int timeout)
	{
		byte[]? message = Read();
		if (message != null)
		{
			response.Accumulate(message);
		}
	}

	protected abstract bool OnDisconnect();

	protected abstract bool OnConnect();

	protected byte[]? ReadBytes(int count)
	{
		if (PipeHandle.IsInvalid)
		{
			return null;
		}
		int lpNumberOfBytesRead = 0;
		byte[] buffer = new byte[count];
		if (!Win32.ReadFile(PipeHandle, buffer, count, out lpNumberOfBytesRead, IntPtr.Zero))
		{
			int lastWin32Error = Marshal.GetLastWin32Error();
			LogHelper.Instance.Log($"PipeClientSession.ReadFile() returned false; error code = {lastWin32Error}", LogEntryType.Error);
			return null;
		}
		if (lpNumberOfBytesRead == count)
		{
			return buffer;
		}
		return null;
	}

	protected BasePipeChannel(string id)
	{
		FlushOnWrite = false;
		ID = id;
	}

	protected BasePipeChannel(SafeFileHandle handle, bool flushOnWrite, string id)
	{
		PipeHandle = handle;
		FlushOnWrite = flushOnWrite;
		ID = id;
	}

	protected BasePipeChannel(SafeFileHandle pipeHandle, string id)
		: this(pipeHandle, flushOnWrite: false, id)
	{
	}

	~BasePipeChannel()
	{
		DisposeChannel(fromDispose: false);
	}

	[DllImport("kernel32.dll", SetLastError = true)]
	protected static extern bool FlushFileBuffers(SafeFileHandle handle);

	private void DisposeChannel(bool fromDispose)
	{
		if (!Disposed)
		{
			Disposed = true;
			Disconnect();
			LogHelper.Instance.Log(LogEntryType.Debug, "[{0}-{1}] OnDispose", GetType().Name, ID);
		}
	}
}
