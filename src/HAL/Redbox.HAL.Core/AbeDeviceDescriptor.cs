using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core.Descriptors;

namespace Redbox.HAL.Core;

public sealed class AbeDeviceDescriptor : AbstractDeviceDescriptor
{
	private readonly bool Debug;

	public AbeDeviceDescriptor(IUsbDeviceService service)
		: this(service, debug: false)
	{
	}

	public AbeDeviceDescriptor(IUsbDeviceService s, bool debug)
		: base(s, DeviceClass.HIDClass)
	{
		Debug = debug;
		base.Vendor = "2047";
		base.Product = "0902";
		base.Friendlyname = "ABE Device";
	}

	protected override bool OnResetDriver()
	{
		throw new NotImplementedException("AbeDeviceDescriptor::ResetDriver is not implemented.");
	}

	protected override DeviceStatus OnGetStatus()
	{
		throw new NotImplementedException("AbeDeviceDescriptor::GetStatus is not implemented.");
	}
}
