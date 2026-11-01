namespace Redbox.HAL.DataMatrix.Framework.Cortex;

internal sealed class TakePictureCommand : AbstractCortexCommand
{
	private readonly byte[] m_rawCommand = new byte[4] { 36, 1, 7, 0 };

	protected override byte[] CoreCommand => m_rawCommand;

	protected override int ReadTimeout => 8000;

	internal ImagePacket ImagePacket
	{
		get
		{
			ImagePacket result = null;
			CortexPacket[] parsedPackets = base.ParsedPackets;
			foreach (CortexPacket cortexPacket in parsedPackets)
			{
				if (cortexPacket.RawPacketType == 'm')
				{
					result = new ImagePacket(cortexPacket);
				}
			}
			return result;
		}
	}

	protected override bool CoreValidate()
	{
		int responseCount = 0;
		foreach (CortexPacket packet in Packets)
		{
			if (packet.RawPacketType == 'm')
			{
				responseCount++;
			}
			else if (packet.RawPacketType == 'd')
			{
				responseCount++;
			}
		}
		return responseCount >= 2;
	}

	protected override bool CoreProcess()
	{
		bool doneSeen = false;
		bool pictureSeen = false;
		foreach (CortexPacket packet in Packets)
		{
			if (packet.RawPacketType == 'd')
			{
				doneSeen = true;
			}
			else if (packet.RawPacketType == 'm')
			{
				pictureSeen = true;
			}
			else if (packet.PacketType == PacketType.Incomplete)
			{
				doneSeen = (pictureSeen = false);
			}
		}
		return doneSeen & pictureSeen;
	}
}
