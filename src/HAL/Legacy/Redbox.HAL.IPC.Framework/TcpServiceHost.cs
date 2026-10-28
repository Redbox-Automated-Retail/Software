using System;
using System.Net;
using System.Net.Sockets;
using Redbox.HAL.Core;

namespace Redbox.HAL.IPC.Framework;

public class TcpServiceHost : IPCServiceHost
{
	private TcpListener m_listener;

	private int m_listenerPort;

	public override void Start()
	{
		base.Alive = true;
		IPAddress iPAddress;
		if (IPAddress.TryParse(base.Protocol.Host, out var address))
		{
			iPAddress = address;
		}
		else
		{
			iPAddress = IPAddressHelper.GetAddressForHostName(base.Protocol.Host);
			if (iPAddress == null)
			{
				LogHelper.Instance.Log($"Unable to bind to host named '{base.Protocol.Host}' on port {base.Protocol.Port},  ensure there is a valid network interface associated to this host and that the designated port is available.", LogEntryType.Fatal);
				return;
			}
		}
		if (!int.TryParse(base.Protocol.Port, out m_listenerPort))
		{
			LogHelper.Instance.Log($"Unable to create a listener on port '{base.Protocol.Port}'; maybe your uri is wrong?", LogEntryType.Fatal);
			return;
		}
		m_listener = new TcpListener(new IPEndPoint(iPAddress, m_listenerPort));
		m_listener.Start();
		LogHelper.Instance.Log("TCP Server: start processing incoming requests on address {0}.", iPAddress);
		Statistics.Instance.ServerStartTime = DateTime.Now;
		while (base.Alive)
		{
			try
			{
				TcpClient client = m_listener.AcceptTcpClient();
				TcpServerSession tcpServerSession = new TcpServerSession(client, this);
				RegiserSession(tcpServerSession);
				tcpServerSession.Start();
			}
			catch (SocketException ex)
			{
				if (ex.SocketErrorCode != SocketError.Interrupted)
				{
					LogHelper.Instance.Log("An unhandled socket exception occurred", ex);
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised.", e);
			}
		}
		LogHelper.Instance.Log("End processing incoming requests.");
	}

	public override void Stop()
	{
		base.Alive = false;
		m_listener.Stop();
	}
}
