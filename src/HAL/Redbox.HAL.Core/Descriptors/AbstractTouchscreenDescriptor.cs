using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Core.Descriptors;

internal abstract class AbstractTouchscreenDescriptor : AbstractDeviceDescriptor, ITouchscreenDescriptor, IDeviceDescriptor
{
	protected readonly IDriverDescriptor DriverDesc;

	public bool SoftReset()
	{
		return OnSoftReset();
	}

	public bool HardReset()
	{
		return OnHardReset();
	}

	public string ReadFirmware()
	{
		return OnReadFirmware();
	}

	protected virtual bool OnSoftReset()
	{
		throw new NotImplementedException("SoftReset");
	}

	protected virtual bool OnHardReset()
	{
		throw new NotImplementedException("HardReset");
	}

	protected virtual string OnReadFirmware()
	{
		return "UNKNOWN";
	}

	internal AbstractTouchscreenDescriptor(IUsbDeviceService s, DeviceClass clazz, IDriverDescriptor desc)
		: base(s, clazz)
	{
		DriverDesc = desc;
	}
}
