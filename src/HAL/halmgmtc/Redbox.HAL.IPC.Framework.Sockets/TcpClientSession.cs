using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using Redbox.HAL.Component.Model;
using Redbox.HAL.IPC.Framework.Client;

namespace Redbox.HAL.IPC.Framework.Sockets;

internal sealed class TcpClientSession : Redbox.HAL.IPC.Framework.Client.ClientSession
{
	private IIPCChannel m_channel;

	private TcpClient TransportClient;

	private Stream Stream;

	private readonly int m_clientPort;

	protected override IIPCChannel Channel => m_channel;

	protected override void OnDispose()
	{
		try
		{
			TransportClient.Close();
		}
		catch (Exception)
		{
		}
	}

	protected override bool OnConnect()
	{
		TransportClient = new TcpClient(base.Protocol.Host, m_clientPort);
		Stream stream = TransportClient.GetStream();
		if (!base.Protocol.IsSecure)
		{
			stream.ReadTimeout = base.Timeout;
		}
		else
		{
			SslStream sslStream = new SslStream(stream, leaveInnerStreamOpen: false, (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true);
			sslStream.ReadTimeout = base.Timeout;
			sslStream.WriteTimeout = base.Timeout;
			sslStream.AuthenticateAsClient(base.Protocol.Host);
			stream = sslStream;
		}
		Stream = stream;
		m_channel = new AsyncChannel(stream, TransportClient.ReceiveBufferSize, base.Identifier);
		return m_channel.Connect();
	}

	internal TcpClientSession(IIpcProtocol protocol, int port, string identifier)
		: base(protocol, identifier)
	{
		m_clientPort = port;
	}
}
