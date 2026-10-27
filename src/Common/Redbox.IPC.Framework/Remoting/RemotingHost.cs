using System;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using Redbox.Core;

namespace Redbox.IPC.Framework.Remoting;

public class RemotingHost : MarshalByRefObject, IRemotingHost
{
	private static long m_connectionCount;

	public string ExecuteCommand(string commandString)
	{
		RemotingServiceHost remotingServiceHost = null;
		using ExecutionTimer timer = new ExecutionTimer();
		try
		{
			Interlocked.Add(ref Statistics.Instance.NumberOfBytesReceived, commandString.Length);
			remotingServiceHost = (RemotingServiceHost)CallContext.GetData("RemotingHost");
			if (Interlocked.Read(ref m_connectionCount) >= remotingServiceHost.MaxThreads)
			{
				return CommandService.Instance.GetServerBusyResult(timer, remotingServiceHost.MaxThreads);
			}
			Interlocked.Increment(ref m_connectionCount);
			LogHelper.Instance.Log($"There are {Interlocked.Read(ref m_connectionCount)} connections active", LogEntryType.Debug);
			string text = new RemotingServerSession().ProcessRequest(commandString);
			Interlocked.Add(ref Statistics.Instance.NumberOfBytesSent, text.Length);
			Interlocked.Decrement(ref m_connectionCount);
			return text;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("RemotingHost execution error", e);
			return null;
		}
	}
}
