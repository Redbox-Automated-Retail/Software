using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using Redbox.IPC.Framework;

namespace Redbox.HAL.IPC.Framework;

public class TcpClientSession : ClientSession
{
	private Stream m_stream;

	private TcpClient m_transportClient;

	private int m_clientPort;

	protected override Stream Stream
	{
		get
		{
			if (m_stream == null)
			{
				if (!base.Protocol.IsSecure)
				{
					m_stream = TransportClient.GetStream();
					m_stream.ReadTimeout = base.Timeout;
				}
				else
				{
					SslStream sslStream = new SslStream(TransportClient.GetStream(), leaveInnerStreamOpen: false, (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true);
					sslStream.ReadTimeout = base.Timeout;
					sslStream.WriteTimeout = base.Timeout;
					SslStream sslStream2 = sslStream;
					sslStream2.AuthenticateAsClient(base.Protocol.Host);
					m_stream = sslStream2;
				}
			}
			return m_stream;
		}
	}

	protected internal TcpClientSessionPool OwningPool { get; set; }

	private TcpClient TransportClient
	{
		get
		{
			return m_transportClient;
		}
		set
		{
			m_transportClient = value;
			if (m_transportClient != null)
			{
				base.ReadBuffer = new byte[m_transportClient.ReceiveBufferSize];
			}
		}
	}

	public override bool Connect()
	{
		try
		{
			ConnectThrowOnError();
			return true;
		}
		catch (IOException)
		{
			return false;
		}
	}

	public override void Dispose()
	{
		if (OwningPool != null)
		{
			OwningPool.ReturnSession(this);
		}
		else
		{
			base.Dispose();
		}
	}

	public override void ConnectThrowOnError()
	{
		TransportClient = new TcpClient(base.Protocol.Host, m_clientPort);
		ConsumeMessages();
		base.IsConnected = true;
	}

	protected internal override bool IsConnectionAvailable()
	{
		if (TransportClient.Client.Poll(10, SelectMode.SelectRead))
		{
			return TransportClient.Client.Available != 0;
		}
		return true;
	}

	protected internal override void CustomClose()
	{
		try
		{
			Stream.Close();
			TransportClient.Close();
		}
		catch (Exception)
		{
		}
	}

	protected override bool CanRead()
	{
		if (Stream.CanRead)
		{
			return TransportClient.Client.Available > 0;
		}
		return false;
	}

	protected override int GetAvailableData()
	{
		return TransportClient.Client.Available;
	}

	internal TcpClientSession(IPCProtocol protocol, int port)
		: base(protocol)
	{
		m_clientPort = port;
	}
}
