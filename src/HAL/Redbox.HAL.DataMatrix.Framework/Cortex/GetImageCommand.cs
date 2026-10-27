using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Timers;

namespace Redbox.HAL.DataMatrix.Framework.Cortex;

internal sealed class GetImageCommand : TextCommand
{
	private readonly ImagePacket ImagePacket;

	private readonly string ImageName;

	private byte[] ImageBytes;

	protected override string Prefix => "^";

	protected override string Data => ImageName;

	protected override int ReadTimeout => 10000;

	internal ImageDescriptorPacket Descriptor { get; private set; }

	protected override bool OnValidateResponse()
	{
		if (Descriptor != null)
		{
			return ImageBytes != null;
		}
		return false;
	}

	protected override bool OnTest(byte[] array)
	{
		if (Descriptor == null)
		{
			CortexPacket cortexPacket = PacketFactory.Instance.Parse(array).Find((CortexPacket each) => each.RawPacketType == 'g');
			if (cortexPacket != null)
			{
				Descriptor = new ImageDescriptorPacket(cortexPacket);
				if (LogDetails)
				{
					LogHelper.Instance.Log(" [GetImageCommand] Process 'g' packet ( ImageSize = {0} ) packet size = {1}.", Descriptor.ImageSize, cortexPacket.Length);
				}
			}
			return false;
		}
		if (array.Length < Descriptor.ImageSize + 37 + Descriptor.Length)
		{
			return false;
		}
		List<CortexPacket> list = PacketFactory.Instance.Parse(array);
		if (list.Count == 0)
		{
			return false;
		}
		if (list[0].RawPacketType != 'g' || list[list.Count - 1].RawPacketType != 'd')
		{
			return false;
		}
		List<CortexPacket> list2 = list.FindAll((CortexPacket each) => each.PacketType == PacketType.Data);
		ImageBytes = new byte[Descriptor.ImageSize];
		int offset = 0;
		list2.ForEach(delegate(CortexPacket each)
		{
			Array.Copy(each.PayloadData, 0, ImageBytes, offset, each.PayloadData.Length);
			offset += each.PayloadData.Length;
		});
		DumpPackets(list);
		return true;
	}

	protected override void OnDispose(bool disposing)
	{
		ImageBytes = null;
	}

	internal bool CreateImage(string fileName)
	{
		if (ImageBytes == null || Descriptor == null || ImageBytes.Length != Descriptor.ImageSize)
		{
			LogHelper.Instance.Log("[GetImageCommand] Unable to construct image. Imag bytes = {0} ( expected {1} )", ImageBytes.Length, Descriptor.ImageSize);
			return false;
		}
		try
		{
			using (ExecutionTimer executionTimer = new ExecutionTimer())
			{
				using (Image image = Image.FromStream(new MemoryStream(ImageBytes)))
				{
					image.Save(fileName, ImageFormat.Jpeg);
				}
				executionTimer.Stop();
				LogHelper.Instance.Log("[GetImageCommand] image construction time = {0}ms", executionTimer.ElapsedMilliseconds);
			}
			return true;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"[GetImageCommand] Exception: unable to construct image '{fileName}'. Image bytes = {ImageBytes.Length} ( expected {Descriptor.ImageSize} )", e);
			return false;
		}
	}

	internal GetImageCommand(ImagePacket packet)
	{
		ImagePacket = packet;
		ImageName = packet.ImageName;
	}
}
