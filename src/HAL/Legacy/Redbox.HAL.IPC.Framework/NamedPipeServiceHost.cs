using System;
using System.Collections.Generic;
using System.Threading;
using Redbox.HAL.Core;

namespace Redbox.HAL.IPC.Framework;

public class NamedPipeServiceHost : IPCServiceHost
{
	private List<ISession> m_sessions = new List<ISession>();

	private object m_sessionsSyncObject = new object();

	public override void Start()
	{
		base.Alive = true;
		LogHelper.Instance.Log(string.Format("Pipe Server accepting connections on pipe {0}", "HALService7001"));
		NamedPipeServer namedPipeServer = NamedPipeServer.CreateServer("HALService7001");
		try
		{
			while (base.Alive)
			{
				AbstractNamedPipeServerSession session = namedPipeServer.WaitForClientConnect(PipeServerSessionFactory.Instance);
				if (session != null && session is NamedPipeServerSession namedPipeServerSession)
				{
					namedPipeServerSession.ServiceHost = this;
					RegiserSession(namedPipeServerSession);
					ThreadPool.QueueUserWorkItem(delegate
					{
						session.Start();
					});
				}
			}
		}
		catch (ThreadInterruptedException)
		{
			LogHelper.Instance.Log("Server thread interrupted");
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Caught exception in NamedPipeServiceListener.Start", e);
		}
	}

	public override void Stop()
	{
		LogHelper.Instance.Log(string.Format("Attempting to Stop NamedPipeServiceHost on host {0} with pipe {1}", base.Protocol.Host, "HALService7001"));
		base.Alive = false;
	}

	public override void UnregisterSession(ISession session)
	{
		if (session != null)
		{
			UnregisterSessionInner(session);
		}
	}

	public override void RegiserSession(ISession session)
	{
		if (session != null)
		{
			int num = 0;
			lock (m_sessionsSyncObject)
			{
				m_sessions.Add(session);
				num = m_sessions.Count;
			}
			if (LogHelper.Instance.IsLevelEnabled(LogEntryType.Debug))
			{
				LogHelper.Instance.Log($"There are currently {num} active sessions.");
			}
		}
	}

	private void UnregisterSessionInner(ISession session)
	{
		lock (m_sessionsSyncObject)
		{
			m_sessions.Remove(session);
		}
		LogHelper.Instance.Log("Unregister server session.", LogEntryType.Debug);
	}
}
