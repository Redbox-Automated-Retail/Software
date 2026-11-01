using System.Collections.Generic;
using System.Text;

namespace Redbox.HAL.DataMatrix.Framework.Cortex;

internal class ImageDescriptorPacket : ImagePacket
{
	internal int ImageSize { get; private set; }

	protected override void ParseData()
	{
		List<byte> nameBytes = new List<byte>();
		int index = 22;
		while (Packet[index] != 9)
		{
			nameBytes.Add(Packet[index++]);
		}
		m_imageName = Encoding.ASCII.GetString(nameBytes.ToArray());
		index += 2;
		StringBuilder stringBuilder = new StringBuilder();
		for (; Packet[index] != 41; index++)
		{
			stringBuilder.Append((byte)(Packet[index] - 48));
		}
		ImageSize = int.Parse(stringBuilder.ToString());
		stringBuilder = null;
	}

	internal ImageDescriptorPacket(CortexPacket packet)
		: base(packet)
	{
	}
}
