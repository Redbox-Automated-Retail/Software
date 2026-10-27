using System;

namespace Redbox.IPC.Framework;

[Obsolete("Use ClientSessionPool instead")]
public class TcpClientSessionPool : ClientSessionPool
{
	public TcpClientSessionPool(IPCProtocol protocol, int poolsize)
		: base(protocol, poolsize)
	{
	}

	public TcpClientSessionPool(IPCProtocol protocol, int poolsize, int? timeout)
		: base(protocol, poolsize, timeout)
	{
	}
}
