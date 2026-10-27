using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.IPC.Framework.Pipes;

public class ClientPipeChannel : BasePipeChannel
{
	private readonly string PipePath;

	public ClientPipeChannel(string pipeName, string id)
		: base(id)
	{
		PipePath = "\\\\.\\PIPE\\" + pipeName;
	}

	protected override bool OnDisconnect()
	{
		LogHelper.Instance.Log(LogEntryType.Debug, "[ClientPipeChannel-{0}] OnDisconnect", ID);
		PipeHandle.Close();
		return true;
	}

	protected override bool OnConnect()
	{
		if (base.IsConnected)
		{
			return true;
		}
		PipeHandle = Win32.CreateFile(PipePath, Win32.AccessFlags.GENERIC_READ_WRITE, Win32.ShareFlags.NONE, IntPtr.Zero, 3u, 0u, IntPtr.Zero);
		return base.IsConnected = !PipeHandle.IsInvalid;
	}
}
