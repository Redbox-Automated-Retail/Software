using System.Collections.Generic;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.IPC.Framework;

public abstract class IPCServiceHost : IIpcServiceHost
{
	private const int StateRunning = 1;

	private const int StateStopped = 0;

	private int m_aliveState;

	private readonly object m_sessionLockObject = new object();

	private readonly IList<ISession> m_sessions = new List<ISession>();

	public bool Alive
	{
		get
		{
			return Thread.VolatileRead(ref m_aliveState) == 1;
		}
		set
		{
			Interlocked.Exchange(ref m_aliveState, value ? 1 : 0);
		}
	}

	public IIpcProtocol Protocol { get; protected set; }

	public IHostInfo HostInfo { get; private set; }

	public X509Certificate2 Certificate { get; set; }

	public SslProtocols EncryptionProtocol { get; set; }

	public bool LogDetailedMessages { get; set; }

	public void Start()
	{
		OnStart();
	}

	public void Stop()
	{
		OnStop();
	}

	public void Unregister(ISession session)
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

	public void Register(ISession session)
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

	protected abstract void OnStart();

	protected abstract void OnStop();

	protected IPCServiceHost(IIpcProtocol protocol, IHostInfo info)
	{
		EncryptionProtocol = SslProtocols.None;
		Protocol = protocol;
		HostInfo = info;
	}
}
