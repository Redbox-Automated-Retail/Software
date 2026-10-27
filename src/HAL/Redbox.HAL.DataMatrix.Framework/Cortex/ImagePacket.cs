using System.Collections.Generic;
using System.Text;

namespace Redbox.HAL.DataMatrix.Framework.Cortex;

internal class ImagePacket : CortexPacket
{
	protected const int DataStart = 22;

	protected string m_imageName;

	internal string ImageName => m_imageName;

	protected override void ParseData()
	{
		List<byte> list = new List<byte>();
		int num = 22;
		while (Packet[num] != 4)
		{
			list.Add(Packet[num++]);
		}
		m_imageName = Encoding.ASCII.GetString(list.ToArray());
	}

	internal ImagePacket(CortexPacket packet)
		: base(packet)
	{
	}
}
