using System;
using System.Collections.Generic;
using System.Threading;
using Redbox.IPC.Framework;

namespace Redbox.HAL.IPC.Framework;

public class TcpClientSessionPool : IDisposable
{
	private IPCProtocol m_protocol;

	private bool m_isDisposed;

	private Stack<TcpClientSession> m_sessions;

	private readonly object m_lock = new object();

	public int Port { get; internal set; }

	public string Host { get; internal set; }

	public bool Secure { get; internal set; }

	public int PoolSize { get; internal set; }

	public int? Timeout { get; internal set; }

	internal Stack<TcpClientSession> Sessions
	{
		get
		{
			if (m_sessions == null)
			{
				m_sessions = new Stack<TcpClientSession>();
			}
			return m_sessions;
		}
	}

	public TcpClientSessionPool(IPCProtocol protocol, int poolSize)
		: this(protocol, poolSize, null)
	{
	}

	public TcpClientSessionPool(IPCProtocol protocol, int poolSize, int? timeout)
	{
		m_protocol = protocol;
		Host = protocol.Host;
		if (!int.TryParse(protocol.Port, out var result))
		{
			throw new Exception("Port in protocol uri is invalid for sockets.");
		}
		Port = result;
		Secure = protocol.IsSecure;
		Timeout = timeout;
		PoolSize = poolSize;
		for (int i = 0; i < PoolSize; i++)
		{
			Sessions.Push(CreateSession());
		}
	}

	public void Dispose()
	{
		lock (m_lock)
		{
			if (m_isDisposed)
			{
				return;
			}
			m_isDisposed = true;
			while (Sessions.Count > 0)
			{
				TcpClientSession tcpClientSession = Sessions.Pop();
				try
				{
					if (tcpClientSession.IsConnected && tcpClientSession.IsConnectionAvailable())
					{
						tcpClientSession.Close();
					}
					else
					{
						tcpClientSession.CustomClose();
					}
				}
				catch
				{
				}
			}
		}
	}

	public bool ReturnSession(ClientSession session)
	{
		lock (m_lock)
		{
			if (m_isDisposed)
			{
				return false;
			}
			Sessions.Push(session as TcpClientSession);
			Monitor.PulseAll(m_lock);
		}
		return true;
	}

	public ClientSession GetSession(int timeout, int attempts)
	{
		TcpClientSession tcpClientSession;
		lock (m_lock)
		{
			if (m_isDisposed)
			{
				throw new ObjectDisposedException("Session pool is diposed.");
			}
			while (Sessions.Count <= 0)
			{
				if (!Monitor.Wait(m_lock, timeout) && --attempts == 0)
				{
					throw new TimeoutException("Unable to acquire TCP client session from pool.");
				}
			}
			tcpClientSession = Sessions.Pop();
			if (tcpClientSession.IsConnected && !tcpClientSession.IsConnectionAvailable())
			{
				tcpClientSession.CustomClose();
				tcpClientSession = CreateSession();
			}
		}
		return tcpClientSession;
	}

	internal TcpClientSession CreateSession()
	{
		TcpClientSession tcpClientSession = new TcpClientSession(m_protocol, Port);
		tcpClientSession.OwningPool = this;
		TcpClientSession tcpClientSession2 = tcpClientSession;
		if (Timeout.HasValue)
		{
			tcpClientSession2.Timeout = Timeout.Value;
		}
		return tcpClientSession2;
	}
}
