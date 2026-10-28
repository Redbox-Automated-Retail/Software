using System.Collections.Generic;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using Redbox.HAL.Core;
using Redbox.IPC.Framework;

namespace Redbox.HAL.IPC.Framework;

public abstract class IPCServiceHost
{
	private static IPCServiceHost m_serviceHost;

	private readonly object m_sessionLockObject = new object();

	private readonly IList<ISession> m_sessions = new List<ISession>();

	public bool Alive { get; set; }

	public IPCProtocol Protocol { get; protected set; }

	public string Name { get; protected set; }

	public string Version { get; protected set; }

	public string Copyright { get; protected set; }

	public X509Certificate2 Certificate { get; set; }

	public SslProtocols EncryptionProtocol { get; set; }

	protected bool LogDetailedMessages { get; private set; }

	public static IPCServiceHost Create(IPCProtocol protocol, string product, string version, string copyright, int minThreads, int maxThreads)
	{
		if (m_serviceHost != null)
		{
			return m_serviceHost;
		}
		if (protocol.Channel == ChannelType.NamedPipe)
		{
			m_serviceHost = new NamedPipeServiceHost();
		}
		else
		{
			if (protocol.Channel != ChannelType.Socket)
			{
				LogHelper.Instance.Log($"Unsupported channel type '{protocol.Channel}' specified; try using sockets or pipes in your protocol uri.", LogEntryType.Error);
				return null;
			}
			m_serviceHost = new TcpServiceHost();
		}
		m_serviceHost.Protocol = protocol;
		m_serviceHost.Name = product;
		m_serviceHost.Version = version;
		m_serviceHost.Copyright = copyright;
		return m_serviceHost;
	}

	public abstract void Start();

	public abstract void Stop();

	public virtual void UnregisterSession(ISession session)
	{
		int num = 0;
		lock (m_sessionLockObject)
		{
			m_sessions.Remove(session);
			num = m_sessions.Count;
		}
		if (LogDetailedMessages)
		{
			LogHelper.Instance.Log($"Unregister server session - there are {num} active server sessions.", LogEntryType.Debug);
		}
	}

	public virtual void RegiserSession(ISession session)
	{
		int num = 0;
		lock (m_sessionLockObject)
		{
			m_sessions.Add(session);
			num = m_sessions.Count;
		}
		if (LogDetailedMessages)
		{
			LogHelper.Instance.Log($"Register server session - there are {num} active server sessions.", LogEntryType.Debug);
		}
	}

	protected IPCServiceHost()
	{
		EncryptionProtocol = SslProtocols.None;
		bool? secretValue = RegistryOptions.Instance.GetSecretValue<bool?>("SOFTWARE\\Redbox\\HAL", "IPCDetailedMessages");
		if (secretValue.HasValue)
		{
			LogDetailedMessages = secretValue.Value;
		}
	}
}
