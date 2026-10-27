using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Core.Descriptors;

internal sealed class GenericTouchscreenDescriptor : AbstractTouchscreenDescriptor
{
	protected override bool OnResetDriver()
	{
		throw new NotImplementedException("ResetDriver");
	}

	protected override DeviceStatus OnGetStatus()
	{
		throw new NotImplementedException("GetStatus");
	}

	protected override bool OnMatchDriver()
	{
		return OnMatchDriver(this, DriverDesc);
	}

	internal GenericTouchscreenDescriptor(string v, string p, string f, IDriverDescriptor desc, IUsbDeviceService s, DeviceClass clazz)
		: base(s, clazz, desc)
	{
		base.Vendor = v;
		base.Product = p;
		base.Friendlyname = f;
	}
}
