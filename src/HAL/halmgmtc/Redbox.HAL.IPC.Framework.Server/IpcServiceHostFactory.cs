using System;
using System.Reflection;
using Redbox.HAL.Component.Model;
using Redbox.HAL.IPC.Framework.Pipes;
using Redbox.HAL.IPC.Framework.Sockets;

namespace Redbox.HAL.IPC.Framework.Server;

public sealed class IpcServiceHostFactory : IIpcServiceHostFactory
{
	private readonly IpcHostVersion Version;

	public IIpcServiceHost Create(IIpcProtocol protocol)
	{
		throw new NotImplementedException();
	}

	public IIpcServiceHost Create(IIpcProtocol protocol, IHostInfo info)
	{
		IIpcServiceHost ipcServiceHost = null;
		if (protocol.Channel == ChannelType.NamedPipe)
		{
			return new NamedPipeServiceHost(protocol, info);
		}
		if (protocol.Channel == ChannelType.Socket)
		{
			return CreateTcpHost(protocol, info);
		}
		LogHelper.Instance.Log(LogEntryType.Error, "Unsupported channel type '{0}' specified; try using sockets or pipes in your protocol uri.", protocol.Channel);
		return null;
	}

	public IHostInfo Create(Assembly a)
	{
		return new HostInfo(a);
	}

	public IpcServiceHostFactory()
		: this(IpcHostVersion.Legacy)
	{
	}

	public IpcServiceHostFactory(IpcHostVersion version)
	{
		Version = version;
	}

	private IIpcServiceHost CreateTcpHost(IIpcProtocol protocol, IHostInfo info)
	{
		IIpcServiceHost ipcServiceHost = null;
		if (IpcHostVersion.Modern == Version)
		{
			return new Redbox.HAL.IPC.Framework.Sockets.TcpServiceHost(protocol, info);
		}
		return new TcpServiceHost(protocol, info);
	}
}
