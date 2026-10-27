using System.IO;
using System.Net.Sockets;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.IPC.Framework.Sockets;

public sealed class TcpServerSession : AbstractServerSession
{
	private readonly IIPCChannel m_channel;

	private readonly TcpClient TransportClient;

	protected override IIPCChannel Channel => m_channel;

	public TcpServerSession(TcpClient client, IIpcServiceHost serviceHost, string id)
		: base(serviceHost, id)
	{
		TransportClient = client;
		Stream stream = client.GetStream();
		m_channel = new AsyncChannel(stream, client.ReceiveBufferSize, id, logDetails: true);
	}

	protected override bool OnSessionEnd()
	{
		if (base.LogDetailedMessages)
		{
			LogHelper.Instance.Log("[TcpServerSession-{0}] Session end.", Identifier);
		}
		TransportClient.Close();
		Channel.Dispose();
		return true;
	}
}
