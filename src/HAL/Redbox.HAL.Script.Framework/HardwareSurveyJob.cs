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
		IRuntimeService runtimeService = ServiceLocator.Instance.GetService<IRuntimeService>();
		base.Context.CreateInfoResult("KioskId", runtimeService.KioskId);
		IUsbDeviceService usbDeviceService = ServiceLocator.Instance.GetService<IUsbDeviceService>();
		base.Context.CreateInfoResult("AuxRelayBoard", "Not Configured");
		base.Context.CreateInfoResult("QuickReturnStatus", DeviceStatus.None.ToString());
		IAirExchangerService airExchangerService = ServiceLocator.Instance.GetService<IAirExchangerService>();
		DeviceStatus deviceStatus = DeviceStatus.None;
		if (airExchangerService.Configured)
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
		IDeviceDescriptor deviceDescriptor = usbDeviceService.FindActiveCamera(matchDriver: false);
		base.Context.CreateInfoResult("CameraInfo", (deviceDescriptor == null) ? "NONE" : deviceDescriptor.Friendlyname);
		DeviceStatus abeStatus = DeviceStatus.None;
		if (new AbeDeviceDescriptor(usbDeviceService).Locate())
		{
			abeStatus = DeviceStatus.Found;
		}
		base.Context.CreateInfoResult("AbeDeviceStatus", abeStatus.ToString());
		ITouchscreenDescriptor touchscreenDescriptor = usbDeviceService.FindTouchScreen();
		base.Context.CreateInfoResult("TouchscreenDevice", (touchscreenDescriptor == null) ? "UNKNOWN" : touchscreenDescriptor.Friendlyname);
		base.Context.CreateInfoResult("TouchscreenFirmware", (touchscreenDescriptor == null) ? "UNKNOWN" : touchscreenDescriptor.ReadFirmware());
		IControlSystem controlSystem = ServiceLocator.Instance.GetService<IControlSystem>();
		base.Context.CreateInfoResult("ControllerFirmware", controlSystem.GetBoardVersion(ControlBoards.Serial).Version);
		List<IDeviceDescriptor> monitors = usbDeviceService.FindDevices(DeviceClass.Monitor);
		string monitorInfo = "UNKNOWN";
		foreach (IDeviceDescriptor monitor in monitors)
		{
			string key = monitor.Product.ToLower();
			if (Monitors.ContainsKey(key))
			{
				monitorInfo = Monitors[key];
				break;
			}
		}
		base.Context.CreateInfoResult("MonitorInfo", monitorInfo);
		List<IDeviceDescriptor> batteries = usbDeviceService.FindDevices(DeviceClass.Battery);
		string upsInfo = "NONE";
		foreach (IDeviceDescriptor battery in batteries)
		{
			if (battery.Vendor.Equals("051d", StringComparison.CurrentCultureIgnoreCase))
			{
				upsInfo = "APC";
				break;
			}
			if (battery.Vendor.Equals("09ae", StringComparison.CurrentCultureIgnoreCase))
			{
				upsInfo = "Tripplite";
				break;
			}
		}
		base.Context.CreateInfoResult("UpsInfo", upsInfo);
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
