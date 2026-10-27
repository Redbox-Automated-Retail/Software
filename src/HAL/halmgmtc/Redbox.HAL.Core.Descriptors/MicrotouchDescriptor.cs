using System;
using System.Threading;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Core.Descriptors;

internal sealed class MicrotouchDescriptor : ITouchscreenDescriptor, IDeviceDescriptor, IDriverDescriptor
{
	public string Vendor => "0596";

	public string Product => "0001";

	public string Friendlyname => "3M";

	public IDeviceSetupClass SetupClass { get; private set; }

	public Version DriverVersion { get; private set; }

	public string Provider => "3M";

	public override string ToString()
	{
		return $"vid_{Vendor}&pid_{Product}";
	}

	public bool SoftReset()
	{
		return new _3mSoftResetCommand().Reset();
	}

	public bool HardReset()
	{
		IUsbDeviceService service = ServiceLocator.Instance.GetService<IUsbDeviceService>();
		bool result = false;
		if (service.SetDeviceState(this, DeviceState.Disable))
		{
			Thread.Sleep(2000);
			result = service.SetDeviceState(this, DeviceState.Enable);
			Thread.Sleep(2000);
		}
		return result;
	}

	public string ReadFirmware()
	{
		return new _3mFirmwareCommand().GetFirmwareRevision();
	}

	public bool ResetDriver()
	{
		throw new NotImplementedException();
	}

	public bool MatchDriver()
	{
		if (ServiceLocator.Instance.GetService<IUsbDeviceService>().MatchDriver(this, this))
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
		throw new NotImplementedException();
	}

	internal MicrotouchDescriptor(DeviceClass clazz)
	{
		DriverVersion = new Version("7.13.14.0");
		IDeviceSetupClassFactory service = ServiceLocator.Instance.GetService<IDeviceSetupClassFactory>();
		SetupClass = service.Get(clazz);
	}
}
