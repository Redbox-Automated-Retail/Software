using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Core;

internal sealed class Gen5DeviceDescriptor : IDeviceDescriptor
{
	private readonly IDriverDescriptor DriverInfo = new DriverDescriptor(new Version("2.0.0.2"), "Scanner Manufacturer");

	public string Vendor => "CCVIRTPORT";

	public string Product => "CCVIRTPORT";

	public string Friendlyname => "Cortex Scanner";

	public IDeviceSetupClass SetupClass { get; private set; }

	public override string ToString()
	{
		return Vendor;
	}

	public bool ResetDriver()
	{
		return false;
	}

	public bool MatchDriver()
	{
		return ServiceLocator.Instance.GetService<IUsbDeviceService>().MatchDriverByVendor(this, DriverInfo);
	}

	public bool Locate()
	{
		return ServiceLocator.Instance.GetService<IUsbDeviceService>().FindDeviceByVendor(this).Found;
	}

	public DeviceStatus GetStatus()
	{
		return DeviceStatus.Enabled;
	}

	internal Gen5DeviceDescriptor()
	{
		IDeviceSetupClassFactory service = ServiceLocator.Instance.GetService<IDeviceSetupClassFactory>();
		SetupClass = service.Get(DeviceClass.Ports);
	}
}
