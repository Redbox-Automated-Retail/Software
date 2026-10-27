using System.Text;

namespace Redbox.HAL.DataMatrix.Framework.Cortex;

internal class NonDataPacket : CortexPacket
{
	internal override PacketType PacketType => PacketType.Response;

	protected override void ParseData()
	{
		base.RawPacketType = Encoding.ASCII.GetChars(Packet, 21, 1)[0];
		if (base.DataSize != 0)
		{
			for (int i = 22; Packet[i] != 4; i++)
			{
				PacketData.Add(Packet[i]);
			}
		}
	}

	internal NonDataPacket(CortexPacket packet)
		: base(packet)
	{
	}

	internal NonDataPacket(byte[] packet, ushort dataSize)
		: base(packet, dataSize)
	{
	}
}
