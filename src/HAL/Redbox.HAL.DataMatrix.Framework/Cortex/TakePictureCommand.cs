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
		int num = 0;
		foreach (CortexPacket packet in Packets)
		{
			if (packet.RawPacketType == 'm')
			{
				num++;
			}
			else if (packet.RawPacketType == 'd')
			{
				num++;
			}
		}
		return num >= 2;
	}

	protected override bool CoreProcess()
	{
		bool flag = false;
		bool flag2 = false;
		foreach (CortexPacket packet in Packets)
		{
			if (packet.RawPacketType == 'd')
			{
				flag = true;
			}
			else if (packet.RawPacketType == 'm')
			{
				flag2 = true;
			}
			else if (packet.PacketType == PacketType.Incomplete)
			{
				flag = (flag2 = false);
			}
		}
		return flag & flag2;
	}
}
