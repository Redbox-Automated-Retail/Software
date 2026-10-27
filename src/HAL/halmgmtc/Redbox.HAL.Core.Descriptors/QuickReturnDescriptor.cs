using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Core.Descriptors;

public sealed class QuickReturnDescriptor : IDeviceDescriptor, IDriverDescriptor
{
	public string Vendor => "0403";

	public string Product => "6001";

	public string Friendlyname => "Quick Return";

	public IDeviceSetupClass SetupClass { get; private set; }

	public Version DriverVersion { get; private set; }

	public string Provider => "FTDI";

	public bool ResetDriver()
	{
		throw new NotImplementedException();
	}

	public bool MatchDriver()
	{
		if (ServiceLocator.Instance.GetService<IUsbDeviceService>().MatchDriver(this, this))
		{
			LogHelper.Instance.Log("[{0}] driver matched.", GetType().Name);
			return true;
		}
		LogHelper.Instance.Log("[{0}]Could not match driver.", GetType().Name);
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

	public QuickReturnDescriptor()
	{
		IDeviceSetupClassFactory service = ServiceLocator.Instance.GetService<IDeviceSetupClassFactory>();
		SetupClass = service.Get(DeviceClass.Ports);
		DriverVersion = new Version("2.4.6.0");
	}
}
