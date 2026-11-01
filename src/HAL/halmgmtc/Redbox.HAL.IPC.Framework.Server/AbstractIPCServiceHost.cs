using System.Collections.Generic;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.IPC.Framework.Server;

internal abstract class AbstractIPCServiceHost : IIpcServiceHost
{
	private readonly AtomicFlag AliveFlag = new AtomicFlag(initial: false);

	private readonly object m_sessionLockObject = new object();

	private readonly IList<ISession> m_sessions = new List<ISession>();

	public bool Alive
	{
		get
		{
			return AliveFlag.IsSet;
		}
		set
		{
			if (value)
			{
				AliveFlag.Set();
			}
			else
			{
				AliveFlag.Clear();
			}
		}
	}

	public IIpcProtocol Protocol { get; private set; }

	public IHostInfo HostInfo { get; private set; }

	public bool LogDetailedMessages { get; set; }

	public void Start()
	{
		Alive = true;
		OnStart();
	}

	public void Stop()
	{
		Alive = false;
		OnStop();
	}

	public void Unregister(ISession session)
	{
		int activeSessions = 0;
		lock (m_sessionLockObject)
		{
			m_sessions.Remove(session);
			activeSessions = m_sessions.Count;
		}
		if (LogDetailedMessages)
		{
			LogHelper.Instance.Log("[IPCServiceHost] Unregister - there are {0} active server sessions.", activeSessions);
		}
	}

	public void Register(ISession session)
	{
		int activeSessions = 0;
		lock (m_sessionLockObject)
		{
			m_sessions.Add(session);
			activeSessions = m_sessions.Count;
		}
		if (LogDetailedMessages)
		{
			LogHelper.Instance.Log("[IPCServiceHost] Register - there are {0} active server sessions.", activeSessions);
		}
	}

	protected abstract void OnStart();

	protected abstract void OnStop();

	protected AbstractIPCServiceHost(IIpcProtocol protocol, IHostInfo info)
	{
		Protocol = protocol;
		HostInfo = info;
	}
}
