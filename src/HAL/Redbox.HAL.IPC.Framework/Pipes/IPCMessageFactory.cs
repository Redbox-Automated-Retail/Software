using System;
using System.Collections.Generic;
using System.Text;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.IPC.Framework.Pipes;

public sealed class IPCMessageFactory : IIPCMessageFactory
{
	public IIPCMessage Create(MessageType p, MessageSeverity s, string message)
	{
		return new IPCMessage
		{
			Type = p,
			Severity = s,
			Message = message,
			UID = Guid.NewGuid(),
			Timestamp = DateTime.Now
		};
	}

	public IIPCMessage CreateAck(Guid guid)
	{
		return new IPCMessage
		{
			Type = MessageType.Ack,
			Severity = MessageSeverity.None,
			Message = string.Empty,
			UID = guid,
			Timestamp = DateTime.Now
		};
	}

	public IIPCMessage CreateNack(Guid guid)
	{
		return new IPCMessage
		{
			Type = MessageType.Nack,
			Severity = MessageSeverity.None,
			Message = string.Empty,
			UID = guid,
			Timestamp = DateTime.Now
		};
	}

	public IIPCMessage Read(IIPCChannel channel)
	{
		byte[] data = channel.Read()!;
		int intSize = 4;
		int offset = 0;
		int uidLength = BitConverter.ToInt32(data, offset);
		offset += intSize;
		byte[] buffer = new byte[uidLength];
		Array.Copy(data, offset, buffer, 0, uidLength);
		Guid uID = new Guid(buffer);
		offset += uidLength;
		MessageType type = (MessageType)BitConverter.ToInt32(data, offset);
		offset += intSize;
		MessageSeverity severity = (MessageSeverity)BitConverter.ToInt32(data, offset);
		offset += intSize;
		int messageLength = BitConverter.ToInt32(data, offset);
		offset += intSize;
		string message = Encoding.ASCII.GetString(data, offset, messageLength);
		offset += messageLength;
		DateTime timestamp = DateTime.FromBinary(BitConverter.ToInt64(data, offset));
		return new IPCMessage
		{
			Type = type,
			Severity = severity,
			Message = message,
			UID = uID,
			Timestamp = timestamp
		};
	}

	public bool Write(IIPCMessage msg, IIPCChannel channel)
	{
		List<byte> list = new List<byte>();
		try
		{
			byte[] uidBytes = msg.UID.ToByteArray();
			list.AddRange(BitConverter.GetBytes(uidBytes.Length));
			list.AddRange(uidBytes);
			list.AddRange(BitConverter.GetBytes((int)msg.Type));
			list.AddRange(BitConverter.GetBytes((int)msg.Severity));
			list.AddRange(BitConverter.GetBytes(msg.Message.Length));
			list.AddRange(Encoding.ASCII.GetBytes(msg.Message));
			list.AddRange(BitConverter.GetBytes(msg.Timestamp.Ticks));
			return channel.Write(list.ToArray());
		}
		finally
		{
			list.Clear();
		}
	}
}
