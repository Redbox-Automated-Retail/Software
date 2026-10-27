using System.Net.Sockets;

namespace Redbox.HAL.IPC.Framework;

public class TcpServerSession : ServerSession
{
	protected override int ReadBufferSize => 1024;

	private TcpClient TransportClient { get; set; }

	private NetworkStream TransportStream { get; set; }

	public TcpServerSession(TcpClient client, IPCServiceHost serviceHost)
		: base(serviceHost, client.ReceiveBufferSize)
	{
		TransportClient = client;
		TransportStream = client.GetStream();
		NegotiateSecurityLayer(TransportStream);
	}

	protected override bool CloseClientsInternal()
	{
		TransportClient.Close();
		return true;
	}

	protected override bool CloseStreamsInternal()
	{
		base.Stream.Dispose();
		return true;
	}
}
