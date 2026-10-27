using System;
using System.Collections.Generic;

namespace Redbox.HAL.DataMatrix.Framework.Cortex;

internal sealed class PacketFactory
{
	private readonly BigEndianConverter Converter = new BigEndianConverter();

	private static PacketFactory m_instance = new PacketFactory();

	private const ushort PacketOverhead = 17;

	internal static PacketFactory Instance => m_instance;

	internal CortexPacket ParseSingle(byte[] data)
	{
		CortexPacket result = new NilPacket(null);
		if (data == null || data.Length == 0)
		{
			return result;
		}
		return ParseInner(data, 0);
	}

	internal List<CortexPacket> Parse(byte[] data)
	{
		List<CortexPacket> list = new List<CortexPacket>();
		CortexPacket cortexPacket;
		for (int i = 0; i < data.Length; i += cortexPacket.Length)
		{
			int startIndex = i;
			cortexPacket = ParseInner(data, startIndex);
			list.Add(cortexPacket);
			if (cortexPacket.PacketType == PacketType.Incomplete || cortexPacket.PacketType == PacketType.Invalid)
			{
				break;
			}
		}
		return list;
	}

	private bool IsPacketStart(byte[] data, int startIndex)
	{
		if (startIndex > data.Length || startIndex + 3 > data.Length)
		{
			return false;
		}
		if (data[startIndex] == 1 && data[startIndex + 1] == 88)
		{
			return data[startIndex + 2] == 82;
		}
		return false;
	}

	private bool IsNonDataPacket(byte[] data, int start)
	{
		if (start > data.Length || start + 18 > data.Length)
		{
			return false;
		}
		if (data[start + 15] == 1 && data[start + 16] == 88)
		{
			return data[start + 17] == 30;
		}
		return false;
	}

	private CortexPacket ParseInner(byte[] data, int startIndex)
	{
		if (startIndex + 15 > data.Length)
		{
			return new IncompletePacket();
		}
		if (!IsPacketStart(data, startIndex))
		{
			return new NilPacket(data);
		}
		ushort num = Converter.ToUInt16(data, startIndex + 13);
		int num2 = num + 17;
		byte[] array = new byte[num2];
		if (startIndex + num2 > data.Length)
		{
			return new IncompletePacket();
		}
		Array.Copy(data, startIndex, array, 0, num2);
		if (IsNonDataPacket(data, startIndex))
		{
			return new NonDataPacket(array, num);
		}
		return new DataPacket(array, num);
	}

	private PacketFactory()
	{
	}
}
