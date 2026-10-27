using Redbox.HAL.Component.Model;

namespace Redbox.HAL.IPC.Framework.Pipes;

internal sealed class NamedPipeServerSession : AbstractServerSession
{
	private readonly IIPCChannel m_channel;

	protected override IIPCChannel Channel => m_channel;

	protected override bool OnSessionEnd()
	{
		Channel.Dispose();
		return true;
	}

	internal NamedPipeServerSession(BasePipeChannel channel, IIpcServiceHost host, string id)
		: base(host, id)
	{
		m_channel = channel;
	}
}
