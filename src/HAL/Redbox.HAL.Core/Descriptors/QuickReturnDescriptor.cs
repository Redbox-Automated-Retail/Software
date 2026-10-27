using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Core.Descriptors;

public sealed class QuickReturnDescriptor : AbstractDeviceDescriptor
{
	private readonly IDriverDescriptor Driver = new DriverDescriptor("2.4.6.0", "FTDI");

	protected override bool OnResetDriver()
	{
		throw new NotImplementedException();
	}

	protected override DeviceStatus OnGetStatus()
	{
		throw new NotImplementedException();
	}

	protected override bool OnMatchDriver()
	{
		return OnMatchDriver(this, Driver);
	}

	public QuickReturnDescriptor(IUsbDeviceService service)
		: base(service, DeviceClass.Ports)
	{
		base.Vendor = "0403";
		base.Product = "6001";
		base.Friendlyname = "Quick Return";
	}
}
