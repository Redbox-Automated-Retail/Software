using System;
using System.Net;
using System.Net.Sockets;
using Redbox.Core;

namespace Redbox.IPC.Framework.Sockets;

public class TcpServiceHost : IPCServiceHost
{
	private Socket m_serverSocket;

	private readonly int? m_backlogSize;

	private IPEndPoint m_serverSocketEndPoint;

	public TcpServiceHost(int? backlogSize)
	{
		m_backlogSize = backlogSize;
	}

	public override bool Start()
	{
		LogHelper.Instance.Log("Find IP address of interface.", LogEntryType.Info);
		base.Alive = true;
		if (!int.TryParse(base.Protocol.Port, out var result))
		{
			LogHelper.Instance.Log($"Unable to create a listener on port '{base.Protocol.Port}'; maybe your uri is wrong?", LogEntryType.Fatal);
			return false;
		}
		IPAddress iPAddress = (IPAddress.TryParse(base.Protocol.Host, out var address) ? address : ((string.Compare("localhost", base.Protocol.Host, ignoreCase: true) != 0) ? IPAddressHelper.GetAddressForHostName(base.Protocol.Host) : IPAddress.Parse("127.0.0.1")));
		if (iPAddress == null)
		{
			LogHelper.Instance.Log($"Unable to bind to host named '{base.Protocol.Host}' on port {base.Protocol.Port},  ensure there is a valid network interface associated to this host and that the designated port is available.", LogEntryType.Fatal);
			return false;
		}
		LogHelper.Instance.Log("Start Worker Pool threads.");
		base.WorkerPool.Start();
		LogHelper.Instance.Log($"Resolved interface to IP address '{iPAddress}'.", LogEntryType.Info);
		LogHelper.Instance.Log("Bind to interface.", LogEntryType.Info);
		CreateServerSocket(iPAddress, result);
		LogHelper.Instance.Log("Begin processing incoming requests.", LogEntryType.Info);
		m_serverSocket.Bind(m_serverSocketEndPoint);
		m_serverSocket.Listen(m_backlogSize ?? 20);
		Statistics.Instance.ServerStartTime = DateTime.Now;
		BeginAccept();
		return true;
	}

	public override void Stop()
	{
		if (base.Alive)
		{
			base.Alive = false;
			try
			{
				m_serverSocket.Close();
				base.WorkerPool.Shutdown();
				BufferPool.Instance.Shutdown();
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was rasied in TcpServiceHost.Stop", e);
			}
			LogHelper.Instance.Log("End processing incoming requests.", LogEntryType.Info);
		}
	}

	private void BeginAccept()
	{
		try
		{
			if (base.Alive)
			{
				m_serverSocket.BeginAccept(EndAccept, this);
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in TcpServiceListener, m_listener.BeginAcceptTcpClient.", e);
			Stop();
		}
	}

	private void EndAccept(IAsyncResult result)
	{
		try
		{
			if (base.Alive)
			{
				Socket socket = m_serverSocket.EndAccept(result);
				base.WorkerPool.QueueWorkItem(delegate
				{
					TcpServerSession tcpServerSession = new TcpServerSession(socket, result.AsyncState as IPCServiceHost);
					RegiserSession(tcpServerSession);
					tcpServerSession.Start();
				});
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in TcpServiceListener, m_listener.EndAcceptTcpClientCallback.", e);
		}
		finally
		{
			BeginAccept();
		}
	}

	private void CreateServerSocket(IPAddress bindToAddress, int port)
	{
		m_serverSocketEndPoint = new IPEndPoint(bindToAddress, port);
		m_serverSocket = new Socket(m_serverSocketEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
		m_serverSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, optionValue: true);
	}
}
