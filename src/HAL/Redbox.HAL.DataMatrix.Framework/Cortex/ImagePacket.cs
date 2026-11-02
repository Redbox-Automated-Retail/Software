using System.Collections.Generic;
using System.Text;

namespace Redbox.HAL.DataMatrix.Framework.Cortex;

internal class ImagePacket : CortexPacket
{
	protected const int DataStart = 22;

	// Set by ParseData, which the CortexPacket constructor calls.
	protected string m_imageName = null!;

	internal string ImageName => m_imageName;

	protected override void ParseData()
	{
		List<byte> nameBytes = new List<byte>();
		int index = 22;
		while (Packet[index] != 4)
		{
			nameBytes.Add(Packet[index++]);
		}
		m_imageName = Encoding.ASCII.GetString(nameBytes.ToArray());
	}

	internal ImagePacket(CortexPacket packet)
		: base(packet)
	{
	}
}
