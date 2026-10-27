using System;
using System.Threading;
using Redbox.HAL.Component.Model;
using Redbox.HAL.IPC.Framework.Server;

namespace Redbox.HAL.IPC.Framework.Pipes;

internal sealed class NamedPipeServiceHost : AbstractIPCServiceHost
{
	private int m_identifier;

	protected override void OnStart()
	{
		LogHelper.Instance.Log("[NamedPipeServiceHost] Accepting connections on pipe {0}", base.Protocol.GetPipeName());
		NamedPipeServer namedPipeServer = NamedPipeServer.Create(base.Protocol.GetPipeName());
		try
		{
			while (base.Alive)
			{
				BasePipeChannel basePipeChannel = namedPipeServer.WaitForClientConnect();
				if (basePipeChannel != null)
				{
					string id = Interlocked.Increment(ref m_identifier).ToString();
					NamedPipeServerSession session = new NamedPipeServerSession(basePipeChannel, this, id);
					Register(session);
					ThreadPool.QueueUserWorkItem(delegate
					{
						session.Start();
					});
				}
			}
			LogHelper.Instance.Log("[NamedPipeServiceHost] Exiting.");
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("[NamedPipeServiceHost] Caught an exception", e);
		}
	}

	protected override void OnStop()
	{
		LogHelper.Instance.Log("[NamedPipeServiceHost] Stop on pipe {0}", base.Protocol.GetPipeName());
		using NamedPipeClientSession namedPipeClientSession = NamedPipeClientSession.MakeSession(base.Protocol, "Shutdown");
		namedPipeClientSession.ConnectThrowOnError();
	}

	internal NamedPipeServiceHost(IIpcProtocol protocol, IHostInfo info)
		: base(protocol, info)
	{
	}
}
