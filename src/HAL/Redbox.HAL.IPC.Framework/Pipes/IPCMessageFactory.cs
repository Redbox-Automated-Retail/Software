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
		byte[] array = channel.Read();
		int num = 4;
		int num2 = 0;
		int num3 = BitConverter.ToInt32(array, num2);
		num2 += num;
		byte[] array2 = new byte[num3];
		Array.Copy(array, num2, array2, 0, num3);
		Guid uID = new Guid(array2);
		num2 += num3;
		MessageType type = (MessageType)BitConverter.ToInt32(array, num2);
		num2 += num;
		MessageSeverity severity = (MessageSeverity)BitConverter.ToInt32(array, num2);
		num2 += num;
		int num4 = BitConverter.ToInt32(array, num2);
		num2 += num;
		string message = Encoding.ASCII.GetString(array, num2, num4);
		num2 += num4;
		DateTime timestamp = DateTime.FromBinary(BitConverter.ToInt64(array, num2));
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
			byte[] array = msg.UID.ToByteArray();
			list.AddRange(BitConverter.GetBytes(array.Length));
			list.AddRange(array);
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
