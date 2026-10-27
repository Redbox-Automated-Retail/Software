using System.Text;

namespace Redbox.HAL.DataMatrix.Framework.Cortex;

internal sealed class InformationPacket : NonDataPacket
{
	internal override PacketType PacketType => PacketType.Information;

	internal string FirmwareVersion { get; private set; }

	internal string SerialNumber { get; private set; }

	internal string OEMIdentifier { get; private set; }

	internal string DecoderVersion { get; private set; }

	internal string State { get; private set; }

	protected override void ParseData()
	{
		base.RawPacketType = Encoding.ASCII.GetChars(Packet, 21, 1)[0];
		FirmwareVersion = Encoding.ASCII.GetString(Packet, 22, 4);
		SerialNumber = Encoding.ASCII.GetString(Packet, 34, 10);
		State = Encoding.ASCII.GetString(Packet, 44, 1);
		OEMIdentifier = Encoding.ASCII.GetString(Packet, 45, 2);
		if (Packet[70] == 9)
		{
			DecoderVersion = Encoding.ASCII.GetString(Packet, 71, Packet.Length - 4 - 70);
		}
	}

	internal InformationPacket(CortexPacket packet)
		: base(packet)
	{
	}
}
