using System.Collections.Generic;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.DataMatrix.Framework.Cortex;

internal sealed class GetFileCommand : TextCommand
{
	private readonly string ImageName;

	private readonly List<byte> ImageData = new List<byte>();

	private readonly List<int> ProcessedPackets = new List<int>();

	protected override string Prefix => "^";

	protected override string Data => ImageName;

	protected override int ReadTimeout => 10000;

	internal byte[] ImageBytes => ImageData.ToArray();

	internal ImageDescriptorPacket Descriptor { get; private set; }

	protected override bool CoreValidate()
	{
		if (Descriptor != null)
		{
			return ImageBytes.Length == Descriptor.ImageSize;
		}
		return false;
	}

	protected override bool CoreProcess()
	{
		if (LogDetails)
		{
			LogHelper.Instance.Log("[GetFileCommand] CoreProcess");
		}
		try
		{
			foreach (CortexPacket packet in Packets)
			{
				if (packet.PacketType == PacketType.Data)
				{
					if (-1 == ProcessedPackets.FindIndex((int each) => each == packet.PacketNumber))
					{
						if (LogDetails)
						{
							LogHelper.Instance.Log(" [GetFileCommand] Process data packet ( data length = {0} )", packet.PayloadData.Length);
						}
						ImageData.AddRange(packet.PayloadData);
						ProcessedPackets.Add(packet.PacketNumber);
					}
					continue;
				}
				if (packet.PacketType == PacketType.Incomplete)
				{
					return false;
				}
				if (packet.RawPacketType == 'g')
				{
					if (Descriptor == null)
					{
						Descriptor = new ImageDescriptorPacket(packet);
					}
					if (LogDetails)
					{
						LogHelper.Instance.Log(" [GetFileCommand] Process 'g' packet ( ImageSize = {0} ) packet size = {1}.", Descriptor.ImageSize, packet.Length);
					}
				}
			}
			if (Descriptor == null)
			{
				return false;
			}
			return ImageData.Count == Descriptor.ImageSize;
		}
		finally
		{
			if (LogDetails)
			{
				LogHelper.Instance.Log("[GetFileCommand] End.");
			}
		}
	}

	internal GetFileCommand(ImagePacket packet)
	{
		ImageName = packet.ImageName;
	}
}
