using System.IO;
using Redbox.HAL.Component.Model;

namespace HALUtilities;

internal sealed class _3MTouchscreen
{
	private readonly Operation_3m Op;

	internal void Execute(TextWriter writer)
	{
		ITouchscreenDescriptor touchscreenDescriptor = ServiceLocator.Instance.GetService<IUsbDeviceService>().FindTouchScreen(matchDriver: false);
		if (touchscreenDescriptor.Friendlyname != "3M")
		{
			writer.WriteLine("Could not locate 3m descriptor; found {0}", touchscreenDescriptor.Friendlyname);
			return;
		}
		switch (Op)
		{
		case Operation_3m.ReadFirmware:
			writer.WriteLine("Firmware: {0}", touchscreenDescriptor.ReadFirmware());
			break;
		case Operation_3m.SoftReset:
			writer.WriteLine("3M soft reset returned {0}", touchscreenDescriptor.SoftReset());
			break;
		case Operation_3m.HardReset:
			writer.WriteLine("3M hard reset returned {0}", touchscreenDescriptor.HardReset());
			break;
		}
	}

	internal _3MTouchscreen(Operation_3m op)
	{
		Op = op;
	}
}
