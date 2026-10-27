using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Core.Descriptors;

public abstract class AbstractDeviceDescriptor : IDeviceDescriptor
{
	protected readonly IUsbDeviceService UsbService;

	protected readonly IRuntimeService RuntimeService;

	public string Vendor { get; protected set; }

	public string Product { get; protected set; }

	public string Friendlyname { get; protected set; }

	public IDeviceSetupClass SetupClass { get; private set; }

	public override string ToString()
	{
		return $"vid_{Vendor}&pid_{Product}";
	}

	public bool ResetDriver()
	{
		return OnResetDriver();
	}

	public bool MatchDriver()
	{
		return OnMatchDriver();
	}

	public bool Locate()
	{
		return OnLocate();
	}

	public DeviceStatus GetStatus()
	{
		return OnGetStatus();
	}

	protected virtual bool OnResetDriver()
	{
		return OnResetDriver(1500);
	}

	protected virtual bool OnLocate()
	{
		return UsbService.FindDevice(this).Found;
	}

	protected virtual DeviceStatus OnGetStatus()
	{
		return UsbService.FindDeviceStatus(this);
	}

	protected virtual bool OnMatchDriver()
	{
		throw new NotImplementedException();
	}

	protected bool OnResetDriver(int pause)
	{
		if (!UsbService.ChangeByHWID(this, DeviceState.Disable))
		{
			return false;
		}
		RuntimeService.Wait(pause);
		bool result = UsbService.ChangeByHWID(this, DeviceState.Enable);
		RuntimeService.SpinWait(pause);
		return result;
	}

	protected bool OnMatchDriver(IDeviceDescriptor desc, IDriverDescriptor driverDesc)
	{
		bool flag = UsbService.MatchDriver(desc, driverDesc);
		if (!flag)
		{
			LogHelper.Instance.Log("[{0}] Could not match driver.", GetType().Name);
		}
		return flag;
	}

	protected AbstractDeviceDescriptor(IUsbDeviceService service, DeviceClass clazz)
	{
		UsbService = service;
		RuntimeService = ServiceLocator.Instance.GetService<IRuntimeService>();
		IDeviceSetupClassFactory service2 = ServiceLocator.Instance.GetService<IDeviceSetupClassFactory>();
		SetupClass = service2.Get(clazz);
	}
}
