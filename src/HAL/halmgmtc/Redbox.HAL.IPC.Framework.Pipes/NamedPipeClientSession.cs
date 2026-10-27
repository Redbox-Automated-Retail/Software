using Redbox.HAL.Component.Model;
using Redbox.HAL.IPC.Framework.Client;

namespace Redbox.HAL.IPC.Framework.Pipes;

public sealed class NamedPipeClientSession : Redbox.HAL.IPC.Framework.Client.ClientSession
{
	private readonly BasePipeChannel m_channel;

	private readonly string ID;

	protected override IIPCChannel Channel => m_channel;

	public static NamedPipeClientSession MakeSession(IIpcProtocol protocol, string identifier)
	{
		return new NamedPipeClientSession(identifier, protocol);
	}

	protected override bool OnConnect()
	{
		return m_channel.Connect();
	}

	private NamedPipeClientSession(string id, IIpcProtocol protocol)
		: base(protocol, id)
	{
		ID = id;
		m_channel = new ClientPipeChannel(protocol.GetPipeName(), id);
	}
}
