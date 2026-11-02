using System;
using System.Collections.Generic;

namespace Redbox.HAL.DataMatrix.Framework.Cortex;

internal abstract class CortexPacket
{
	// Null only for NilPacket (the parameterless constructor), which never reads it; Length checks for it.
	protected readonly byte[] Packet = null!;

	protected readonly List<byte> PacketData = new List<byte>();

	protected readonly BigEndianConverter Converter = new BigEndianConverter();

	internal virtual PacketType PacketType => PacketType.None;

	internal int ProtocolVersion { get; private set; }

	internal int ID { get; private set; }

	internal byte PacketNumber { get; private set; }

	internal int Timestamp { get; private set; }

	internal ushort DataSize { get; private set; }

	internal char RawPacketType { get; set; }

	internal ushort CRC { get; set; }

	internal bool ChecksumOk { get; private set; }

	internal byte[] PayloadData => PacketData.ToArray();

	internal int Length
	{
		get
		{
			if (Packet != null)
			{
				return Packet.Length;
			}
			return 0;
		}
	}

	protected virtual void ParseData()
	{
	}

	protected CortexPacket()
	{
	}

	protected CortexPacket(CortexPacket p)
	{
		Packet = new byte[p.Packet.Length];
		Array.Copy(p.Packet, 0, Packet, 0, p.Packet.Length);
		DataSize = p.DataSize;
		ProtocolVersion = p.ProtocolVersion;
		ID = Converter.ToInt32(Packet, 4);
		PacketNumber = p.PacketNumber;
		Timestamp = p.Timestamp;
		CRC = p.CRC;
		ChecksumOk = p.ChecksumOk;
		RawPacketType = p.RawPacketType;
		ParseData();
	}

	protected CortexPacket(byte[] data)
	{
		Packet = data;
	}

	protected CortexPacket(byte[] packet, ushort dataSize)
	{
		Packet = new byte[packet.Length];
		Array.Copy(packet, 0, Packet, 0, packet.Length);
		ProtocolVersion = Packet[3];
		ID = Converter.ToInt32(Packet, 4);
		PacketNumber = Packet[8];
		Timestamp = Converter.ToInt32(Packet, 9);
		DataSize = Converter.ToUInt16(Packet, 13);
		if (dataSize != DataSize)
		{
			DataSize = dataSize;
		}
		ParseData();
		byte crcLow = Packet[Packet.Length - 1];
		ushort receivedCrc = Packet[Packet.Length - 2];
		receivedCrc = (ushort)((receivedCrc << 8) | crcLow);
		ushort computedCrc = 0;
		for (int i = 0; i < Packet.Length - 2; i++)
		{
			computedCrc = CRC16.CRC(computedCrc, Packet[i]);
		}
		CRC = computedCrc;
		ChecksumOk = CRC == receivedCrc;
	}
}
