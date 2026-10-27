using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Core;

internal sealed class GenericTouchscreenDescriptor : ITouchscreenDescriptor, IDeviceDescriptor
{
	private readonly IDriverDescriptor DriverDesc;

	public string Vendor { get; private set; }

	public string Product { get; private set; }

	public string Friendlyname { get; private set; }

	public IDeviceSetupClass SetupClass { get; private set; }

	public override string ToString()
	{
		return $"vid_{Vendor}&pid_{Product}";
	}

	public bool SoftReset()
	{
		throw new NotImplementedException("TouchscreenDescriptor::SoftReset");
	}

	public bool HardReset()
	{
		throw new NotImplementedException("TouchscreenDescriptor::HardReset");
	}

	public string ReadFirmware()
	{
		return "UNKNOWN";
	}

	public bool MatchDriver()
	{
		if (ServiceLocator.Instance.GetService<IUsbDeviceService>().MatchDriver(this, DriverDesc))
		{
			LogHelper.Instance.Log("Touch screen driver matched.");
			return true;
		}
		LogHelper.Instance.Log("Could not match TS driver.");
		return false;
	}

	public bool Locate()
	{
		return ServiceLocator.Instance.GetService<IUsbDeviceService>().FindDevice(this).Found;
	}

	public DeviceStatus GetStatus()
	{
		throw new NotImplementedException("GenericTouchScreenDescriptor::GetStatus is not implemented");
	}

	public bool ResetDriver()
	{
		throw new NotImplementedException("GenericTouchScreenDescriptor::ResetDriver is not implemented");
	}

	internal GenericTouchscreenDescriptor(string v, string p, string f, IDriverDescriptor desc, DeviceClass clazz)
	{
		Vendor = v;
		Product = p;
		Friendlyname = f;
		DriverDesc = desc;
		IDeviceSetupClassFactory service = ServiceLocator.Instance.GetService<IDeviceSetupClassFactory>();
		SetupClass = service.Get(clazz);
	}
}
