using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32.SafeHandles;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.IPC.Framework.Pipes;

internal sealed class ServerPipeChannel : BasePipeChannel
{
	private static int m_id;

	public static ServerPipeChannel Make(SafeFileHandle handle)
	{
		return new ServerPipeChannel(handle, Interlocked.Increment(ref m_id).ToString());
	}

	protected override bool OnDisconnect()
	{
		LogHelper.Instance.Log(LogEntryType.Debug, "[ServerPipeChannel-{0}] OnDisconnect", ID);
		BasePipeChannel.FlushFileBuffers(PipeHandle);
		DisconnectNamedPipe(PipeHandle);
		PipeHandle.Close();
		return true;
	}

	protected override bool OnConnect()
	{
		LogHelper.Instance.Log("[ServerPipeChannel-{0}] Connect() improperly called.", ID);
		return false;
	}

	private ServerPipeChannel(SafeFileHandle pipeHandle, string id)
		: base(pipeHandle, id)
	{
		base.IsConnected = !pipeHandle.IsInvalid;
	}

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern bool DisconnectNamedPipe(SafeFileHandle hHandle);
}
