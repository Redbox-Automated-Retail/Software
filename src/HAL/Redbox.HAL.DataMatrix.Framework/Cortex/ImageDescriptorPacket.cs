using System.Collections.Generic;
using System.Text;

namespace Redbox.HAL.DataMatrix.Framework.Cortex;

internal class ImageDescriptorPacket : ImagePacket
{
	internal int ImageSize { get; private set; }

	protected override void ParseData()
	{
		List<byte> list = new List<byte>();
		int num = 22;
		while (Packet[num] != 9)
		{
			list.Add(Packet[num++]);
		}
		m_imageName = Encoding.ASCII.GetString(list.ToArray());
		num += 2;
		StringBuilder stringBuilder = new StringBuilder();
		for (; Packet[num] != 41; num++)
		{
			stringBuilder.Append((byte)(Packet[num] - 48));
		}
		ImageSize = int.Parse(stringBuilder.ToString());
		stringBuilder = null;
	}

	internal ImageDescriptorPacket(CortexPacket packet)
		: base(packet)
	{
	}
}
