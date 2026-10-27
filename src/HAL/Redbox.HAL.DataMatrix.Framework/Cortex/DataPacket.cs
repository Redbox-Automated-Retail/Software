namespace Redbox.HAL.DataMatrix.Framework.Cortex;

internal sealed class DataPacket : CortexPacket
{
	internal override PacketType PacketType => PacketType.Data;

	protected override void ParseData()
	{
		for (int i = 0; i < base.DataSize; i++)
		{
			PacketData.Add(Packet[i + 15]);
		}
	}

	internal DataPacket(byte[] packet, ushort dataSize)
		: base(packet, dataSize)
	{
	}
}
