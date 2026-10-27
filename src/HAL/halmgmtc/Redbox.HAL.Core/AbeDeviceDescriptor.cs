using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Core;

public sealed class AbeDeviceDescriptor : IDeviceDescriptor
{
	private readonly bool Debug;

	public string Vendor => "2047";

	public string Product => "0902";

	public string Friendlyname => "ABE Device";

	public IDeviceSetupClass SetupClass { get; private set; }

	public override string ToString()
	{
		return $"vid_{Vendor}&pid_{Product}";
	}

	public bool MatchDriver()
	{
		throw new NotImplementedException();
	}

	public bool Locate()
	{
		return ServiceLocator.Instance.GetService<IUsbDeviceService>().FindDevice(this).Found;
	}

	public DeviceStatus GetStatus()
	{
		throw new NotImplementedException("AbeDeviceDescriptor::GetStatus is not implemented.");
	}

	public bool ResetDriver()
	{
		throw new NotImplementedException("AbeDeviceDescriptor::ResetDriver is not implemented.");
	}

	public AbeDeviceDescriptor(bool debug)
	{
		Debug = debug;
		IDeviceSetupClassFactory service = ServiceLocator.Instance.GetService<IDeviceSetupClassFactory>();
		SetupClass = service.Get(DeviceClass.HIDClass);
	}

	public AbeDeviceDescriptor()
		: this(debug: false)
	{
	}
}
