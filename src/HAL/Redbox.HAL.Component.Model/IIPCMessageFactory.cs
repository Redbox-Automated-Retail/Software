using System;

namespace Redbox.HAL.Component.Model;

public interface IIPCMessageFactory
{
	IIPCMessage Create(MessageType t, MessageSeverity s, string message);

	IIPCMessage CreateAck(Guid guid);

	IIPCMessage CreateNack(Guid guid);

	IIPCMessage Read(IIPCChannel channel);

	bool Write(IIPCMessage msg, IIPCChannel channel);
}
