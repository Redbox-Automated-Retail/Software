using System;
using System.Threading;
using Redbox.HAL.Component.Model;
using Redbox.HAL.IPC.Framework.Pipes;
using Redbox.IPC.Framework;

namespace Redbox.HAL.IPC.Framework;

public static class ClientSessionFactory
{
	private static int m_session;

	public static IIpcClientSession GetClientSession(IPCProtocol protocol)
	{
		string identifier = $"Session-{Interlocked.Increment(ref m_session)}";
		if (protocol.Channel == ChannelType.NamedPipe)
		{
			return NamedPipeClientSession.MakeSession(protocol, identifier);
		}
		if (protocol.Channel == ChannelType.Socket)
		{
			if (int.TryParse(protocol.Port, out var result))
			{
				return new TcpClientSession(protocol, result);
			}
			LogHelper.Instance.Log($"TCP port specification {protocol.Port} is incorrect; check your URI and try again.", LogEntryType.Fatal);
			throw new Exception("Port specification in URI is incorrect.");
		}
		LogHelper.Instance.Log(LogEntryType.Error, "Unrecognized channel type {0}; please fix your protocol string.", protocol.Channel);
		return null;
	}
}
