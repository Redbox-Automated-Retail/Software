using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Redbox.HAL.Component.Model;
using Redbox.HAL.IPC.Framework.Server;

namespace Redbox.HAL.IPC.Framework.Sockets;

internal sealed class TcpServiceHost : AbstractIPCServiceHost
{
	private static int m_counter;

	private TcpListener m_listener;

	private int m_listenerPort;

	protected override void OnStart()
	{
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
				string id = $"TcpServer-{Interlocked.Increment(ref m_counter)}";
				TcpServerSession handler = new TcpServerSession(client, this, id);
				Register(handler);
				ThreadPool.QueueUserWorkItem(delegate
				{
					handler.Start();
				});
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

	protected override void OnStop()
	{
		m_listener.Stop();
	}

	internal TcpServiceHost(IIpcProtocol protocol, IHostInfo info)
		: base(protocol, info)
	{
	}
}
