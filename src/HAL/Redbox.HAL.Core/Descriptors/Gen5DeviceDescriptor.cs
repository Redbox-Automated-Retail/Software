using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Core.Descriptors;

internal sealed class Gen5DeviceDescriptor : AbstractDeviceDescriptor
{
	private readonly IDriverDescriptor DriverInfo = new DriverDescriptor("2.0.0.2", "Scanner Manufacturer");

	protected override bool OnResetDriver()
	{
		return false;
	}

	protected override bool OnLocate()
	{
		DeviceStatus status = GetStatus();
		if ((status & DeviceStatus.Found) != DeviceStatus.None)
		{
			return (status & DeviceStatus.Enabled) != 0;
		}
		return false;
	}

	protected override bool OnMatchDriver()
	{
		return OnMatchDriver(this, DriverInfo);
	}

	internal Gen5DeviceDescriptor(IUsbDeviceService s)
		: base(s, DeviceClass.USB)
	{
		base.Vendor = "11FA";
		base.Product = "0204";
		base.Friendlyname = "Cortex Scanner";
	}
}
