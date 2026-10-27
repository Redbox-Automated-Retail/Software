using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Core.Descriptors;

internal abstract class Abstract3MCommand
{
	protected readonly byte[] ErrorResponse = new byte[0];

	protected readonly byte[] Command;

	protected virtual int CommandSize => 8;

	protected abstract byte[] OnReadResponse(SafeFileHandle handle);

	protected abstract byte OnFillIndex(int idx);

	protected Abstract3MCommand()
	{
		Command = new byte[CommandSize];
		for (int i = 0; i < Command.Length; i++)
		{
			Command[i] = OnFillIndex(i);
		}
	}

	protected SafeFileHandle Connect()
	{
		SafeFileHandle safeFileHandle = Win32.CreateFile("\\\\.\\TwTouchDevice1", Win32.AccessFlags.GENERIC_READ_WRITE, Win32.ShareFlags.FILE_SHARE_READ_WRITE, IntPtr.Zero, 3u, 128u, IntPtr.Zero);
		if (safeFileHandle.IsInvalid)
		{
			LogHelper.Instance.Log("[MicrotouchDescriptor] CreateFile failed: Error = {0}", Marshal.GetLastWin32Error());
		}
		return safeFileHandle;
	}

	protected bool Write(SafeFileHandle handle, byte[] command)
	{
		bool result = true;
		if (!Win32.WriteFile(handle, command, command.Length, out var _, IntPtr.Zero))
		{
			LogHelper.Instance.Log("[{0}] WriteFile failed: Error = {1}", GetType().Name, Marshal.GetLastWin32Error());
			result = false;
		}
		return result;
	}

	protected byte[] Read(SafeFileHandle handle, int responseLength)
	{
		byte[] array = new byte[responseLength];
		byte[] array2 = array;
		if (!Win32.ReadFile(handle, array2, array2.Length, out var _, IntPtr.Zero))
		{
			array = ErrorResponse;
			LogHelper.Instance.Log("[{0}] ReadFile failed: Error = {1}", GetType().Name, Marshal.GetLastWin32Error());
		}
		return array;
	}

	internal byte[] SendReceive()
	{
		using SafeFileHandle safeFileHandle = Connect();
		if (safeFileHandle.IsInvalid)
		{
			return ErrorResponse;
		}
		return (!Write(safeFileHandle, Command)) ? ErrorResponse : OnReadResponse(safeFileHandle);
	}
}
