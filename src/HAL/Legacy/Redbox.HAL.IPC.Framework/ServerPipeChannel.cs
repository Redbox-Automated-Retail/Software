using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using Redbox.HAL.Core;

namespace Redbox.HAL.IPC.Framework;

internal class ServerPipeChannel : BasePipeChannel
{
	internal override bool Connect(string pipeName)
	{
		LogHelper.Instance.Log($"ServerChannel ID = {base.ID}: Connect() improperly called.");
		return false;
	}

	internal ServerPipeChannel(SafeFileHandle pipeHandle)
		: base(pipeHandle)
	{
	}

	protected override bool DisposeChannel(bool fromDispose)
	{
		if (base.Disposed)
		{
			return true;
		}
		BasePipeChannel.FlushFileBuffers(base.PipeHandle);
		DisconnectNamedPipe(base.PipeHandle);
		base.PipeHandle.Close();
		base.Connected = false;
		return base.Disposed = true;
	}

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern bool DisconnectNamedPipe(SafeFileHandle hHandle);
}
