using System;
using Redbox.HAL.Core;

namespace Redbox.HAL.IPC.Framework;

internal class ClientPipeChannel : BasePipeChannel
{
	internal override bool Connect(string pipeName)
	{
		if (base.Connected)
		{
			return false;
		}
		pipeName = "\\\\.\\PIPE\\" + pipeName;
		base.PipeHandle = BasePipeChannel.CreateFile(pipeName, 3221225472u, 0u, IntPtr.Zero, 3u, 0u, IntPtr.Zero);
		if (base.PipeHandle.IsInvalid)
		{
			return false;
		}
		return base.Connected = true;
	}

	internal ClientPipeChannel(string pipeName)
	{
	}

	protected override bool DisposeChannel(bool fromDispose)
	{
		if (base.Disposed)
		{
			return true;
		}
		if (LogHelper.Instance.IsLevelEnabled(LogEntryType.Debug))
		{
			LogHelper.Instance.Log("Client channel: dispose()");
		}
		base.PipeHandle.Close();
		base.Connected = false;
		return base.Disposed = true;
	}
}
