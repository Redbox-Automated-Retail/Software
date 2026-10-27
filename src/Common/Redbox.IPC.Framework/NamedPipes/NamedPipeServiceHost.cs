using System;
using System.IO.Pipes;
using System.Threading;
using Redbox.Core;

namespace Redbox.IPC.Framework.NamedPipes;

public class NamedPipeServiceHost : IPCServiceHost
{
	public override bool Start()
	{
		base.Alive = true;
		LogHelper.Instance.Log("Start Worker Pool threads.");
		base.WorkerPool.Start();
		LogHelper.Instance.Log($"Pipe Server on host {base.Protocol.Host} accepting connections on pipe {base.Protocol.Port}", LogEntryType.Info);
		try
		{
			while (base.Alive)
			{
				NamedPipeServerStream namedPipeServerStream = new NamedPipeServerStream(base.Protocol.Port, PipeDirection.InOut, -1, PipeTransmissionMode.Byte, PipeOptions.WriteThrough | PipeOptions.Asynchronous);
				namedPipeServerStream.WaitForConnection();
				ServerSession serverSession = new NamedPipeServerSession(namedPipeServerStream, this);
				RegiserSession(serverSession);
				serverSession.Start();
			}
		}
		catch (ThreadInterruptedException)
		{
			LogHelper.Instance.Log("Server thread interrupted", LogEntryType.Info);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Caught exception in NamedPipeServiceListener.Start", e);
		}
		return true;
	}

	public override void Stop()
	{
		LogHelper.Instance.Log($"Attempting to Stop NamedPipeServiceHost on host {base.Protocol.Host} with pipe {base.Protocol.Port}", LogEntryType.Info);
		base.Alive = false;
		base.WorkerPool.Shutdown();
		BufferPool.Instance.Shutdown();
	}
}
