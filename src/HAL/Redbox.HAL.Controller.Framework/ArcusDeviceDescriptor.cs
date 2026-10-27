using System;
using System.Diagnostics;
using System.Threading;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core.Descriptors;

namespace Redbox.HAL.Controller.Framework;

internal sealed class ArcusDeviceDescriptor : AbstractDeviceDescriptor
{
	protected override bool OnResetDriver()
	{
		Thread.Sleep(3000);
		if (!UsbService.SetDeviceState(this, DeviceState.Disable))
		{
			return false;
		}
		if (!UsbService.SetDeviceState(this, DeviceState.Enable))
		{
			return false;
		}
		if (UsbService.SetDeviceState(this, DeviceState.Enable))
		{
			return true;
		}
		bool flag = false;
		try
		{
			Process process = Process.Start("c:\\Program Files\\Redbox\\hal-tools\\halutilities.exe", "-resetProteus");
			process.WaitForExit();
			return process.ExitCode == 0;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Unable to reset Proteus driver.", e);
			return false;
		}
	}

	protected override bool OnLocate()
	{
		return false;
	}

	protected override DeviceStatus OnGetStatus()
	{
		return DeviceStatus.Unknown;
	}

	protected override bool OnMatchDriver()
	{
		return false;
	}

	internal ArcusDeviceDescriptor(IUsbDeviceService usb)
		: base(usb, DeviceClass.None)
	{
		base.Vendor = "1589";
		base.Product = "a001";
		base.Friendlyname = "Proteus XES";
	}
}
