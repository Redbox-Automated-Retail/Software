using System;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using Redbox.Core;
using Redbox.IPC.Framework.NamedPipes;
using Redbox.IPC.Framework.Remoting;
using Redbox.IPC.Framework.Sockets;

namespace Redbox.IPC.Framework;

public abstract class IPCServiceHost
{
	private bool _enableFilters;

	private List<string> _filters = new List<string>();

	private Action<string, string> _paramAction;

	private Action<ISession> _registerSessionAction;

	private static IPCServiceHost m_serviceHost;

	private readonly object m_syncObject = new object();

	private readonly IList<ISession> m_sessions = new List<ISession>();

	public bool Alive { get; set; }

	public string Name { get; protected set; }

	public string Version { get; protected set; }

	public string Copyright { get; protected set; }

	public IPCProtocol Protocol { get; protected set; }

	public WorkerPool WorkerPool { get; internal set; }

	public X509Certificate2 Certificate { get; set; }

	public SslProtocols EncryptionProtocol { get; set; }

	public IDictionary<string, object> Properties { get; private set; }

	public bool EnableFilters
	{
		get
		{
			return _enableFilters;
		}
		set
		{
			_enableFilters = value;
		}
	}

	public List<string> Filters => _filters;

	public Action<string, string> ParamAction
	{
		get
		{
			return _paramAction;
		}
		set
		{
			_paramAction = value;
		}
	}

	public Action<ISession> RegisterSessionAction
	{
		get
		{
			return _registerSessionAction;
		}
		set
		{
			_registerSessionAction = value;
		}
	}

	public static IPCServiceHost Create(IPCProtocol protocol, string product, string version, string copyright, int minThreads, int maxThreads, int bufferSize, int numberOfBuffers, int? backlogSize)
	{
		return Create(protocol, product, version, copyright, minThreads, maxThreads, bufferSize, numberOfBuffers, backlogSize, null);
	}

	public static IPCServiceHost Create(IPCProtocol protocol, string product, string version, string copyright, int minThreads, int maxThreads, int bufferSize, int numberOfBuffers, int? backlogSize, string poolName)
	{
		switch (protocol.Channel)
		{
		case ChannelType.NamedPipe:
			m_serviceHost = new NamedPipeServiceHost();
			break;
		case ChannelType.Socket:
			m_serviceHost = new TcpServiceHost(backlogSize);
			break;
		case ChannelType.Remoting:
			m_serviceHost = new RemotingServiceHost(maxThreads);
			break;
		default:
			LogHelper.Instance.Log($"Unsupported channel type '{protocol.Channel}' specified; try using sockets or pipes in your protocol uri.", LogEntryType.Error);
			return null;
		}
		BufferPool.Instance.Initialize(bufferSize, numberOfBuffers);
		if (poolName == null)
		{
			m_serviceHost.WorkerPool = new WorkerPool(minThreads, maxThreads);
		}
		else
		{
			m_serviceHost.WorkerPool = new WorkerPool(minThreads, maxThreads, poolName);
		}
		m_serviceHost.Name = product;
		m_serviceHost.Version = version;
		m_serviceHost.Protocol = protocol;
		m_serviceHost.Copyright = copyright;
		return m_serviceHost;
	}

	public abstract bool Start();

	public abstract void Stop();

	public virtual void UnregisterSession(ISession session)
	{
		UnregisterSession(session, cleanupAbandonedSessions: true);
	}

	public virtual void UnregisterSession(ISession session, bool cleanupAbandonedSessions)
	{
		LogHelper.Instance.Log("Unregister server session.", LogEntryType.Debug);
		int count;
		lock (m_syncObject)
		{
			if (!m_sessions.Remove(session))
			{
				return;
			}
			if (cleanupAbandonedSessions)
			{
				for (int i = 0; i < m_sessions.Count; i++)
				{
					if (m_sessions[i] == null || !m_sessions[i].IsConnected())
					{
						m_sessions.RemoveAt(i);
						LogHelper.Instance.Log("Unregister abandon server session.", LogEntryType.Debug);
					}
				}
			}
			count = m_sessions.Count;
		}
		LogHelper.Instance.Log($"There are {count} server sessions active.", LogEntryType.Debug);
	}

	public virtual void RegiserSession(ISession session)
	{
		int count;
		lock (m_syncObject)
		{
			foreach (KeyValuePair<string, object> property in Properties)
			{
				session.Properties.Add(property);
			}
			session.EnableFilters = EnableFilters;
			session.SetFilters(Filters);
			session.SetParamAction(_paramAction);
			m_sessions.Add(session);
			count = m_sessions.Count;
			if (_registerSessionAction != null)
			{
				_registerSessionAction(session);
			}
		}
		LogHelper.Instance.Log($"Register server session - there are {count} active server sessions.", LogEntryType.Debug);
	}

	protected IPCServiceHost()
	{
		EncryptionProtocol = SslProtocols.None;
		Properties = new Dictionary<string, object>();
	}
}
