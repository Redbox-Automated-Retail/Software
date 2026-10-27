using System;
using System.Collections.Generic;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "hardware-survey-job")]
internal sealed class HardwareSurveyJob : NativeJobAdapter
{
	private readonly Dictionary<string, string> Monitors;

	protected override void ExecuteInner()
	{
		IRuntimeService service = ServiceLocator.Instance.GetService<IRuntimeService>();
		base.Context.CreateInfoResult("KioskId", service.KioskId);
		IUsbDeviceService service2 = ServiceLocator.Instance.GetService<IUsbDeviceService>();
		base.Context.CreateInfoResult("AuxRelayBoard", "Not Configured");
		base.Context.CreateInfoResult("QuickReturnStatus", DeviceStatus.None.ToString());
		IAirExchangerService service3 = ServiceLocator.Instance.GetService<IAirExchangerService>();
		DeviceStatus deviceStatus = DeviceStatus.None;
		if (service3.Configured)
		{
			deviceStatus |= DeviceStatus.Configured;
		}
		base.Context.CreateInfoResult("AirExchanger", deviceStatus.ToString());
		base.Context.CreateInfoResult("FraudSensorEnabled", DeviceStatus.None.ToString());
		Computerinfo computerinfo = new Computerinfo();
		base.Context.CreateInfoResult("PCManufacturer", computerinfo.Manufacturer);
		base.Context.CreateInfoResult("PCModel", computerinfo.Model);
		base.Context.CreateInfoResult("InstalledMemory", computerinfo.InstalledMemory.ToString());
		base.Context.CreateInfoResult("FreeDiskSpace", computerinfo.DiskFreeSpace.ToString());
		IDeviceDescriptor deviceDescriptor = service2.FindActiveCamera(matchDriver: false);
		base.Context.CreateInfoResult("CameraInfo", (deviceDescriptor == null) ? "NONE" : deviceDescriptor.Friendlyname);
		DeviceStatus deviceStatus2 = DeviceStatus.None;
		if (new AbeDeviceDescriptor(service2).Locate())
		{
			deviceStatus2 = DeviceStatus.Found;
		}
		base.Context.CreateInfoResult("AbeDeviceStatus", deviceStatus2.ToString());
		ITouchscreenDescriptor touchscreenDescriptor = service2.FindTouchScreen();
		base.Context.CreateInfoResult("TouchscreenDevice", (touchscreenDescriptor == null) ? "UNKNOWN" : touchscreenDescriptor.Friendlyname);
		base.Context.CreateInfoResult("TouchscreenFirmware", (touchscreenDescriptor == null) ? "UNKNOWN" : touchscreenDescriptor.ReadFirmware());
		IControlSystem service4 = ServiceLocator.Instance.GetService<IControlSystem>();
		base.Context.CreateInfoResult("ControllerFirmware", service4.GetBoardVersion(ControlBoards.Serial).Version);
		List<IDeviceDescriptor> list = service2.FindDevices(DeviceClass.Monitor);
		string message = "UNKNOWN";
		foreach (IDeviceDescriptor item in list)
		{
			string key = item.Product.ToLower();
			if (Monitors.ContainsKey(key))
			{
				message = Monitors[key];
				break;
			}
		}
		base.Context.CreateInfoResult("MonitorInfo", message);
		List<IDeviceDescriptor> list2 = service2.FindDevices(DeviceClass.Battery);
		string message2 = "NONE";
		foreach (IDeviceDescriptor item2 in list2)
		{
			if (item2.Vendor.Equals("051d", StringComparison.CurrentCultureIgnoreCase))
			{
				message2 = "APC";
				break;
			}
			if (item2.Vendor.Equals("09ae", StringComparison.CurrentCultureIgnoreCase))
			{
				message2 = "Tripplite";
				break;
			}
		}
		base.Context.CreateInfoResult("UpsInfo", message2);
	}

	internal HardwareSurveyJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
		Monitors = new Dictionary<string, string>();
		Monitors["PMT025D".ToLower()] = "Promate 17 inch";
		Monitors["KST0011".ToLower()] = "Kristel 17 inch";
		Monitors["KST0015".ToLower()] = "Kristel 15 inch";
		Monitors["MST1500".ToLower()] = "Promate 15 inch";
	}
}
