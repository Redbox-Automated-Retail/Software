using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.Client.Executors;

public sealed class HardwareSurveyExecutor : JobExecutor
{
	public string Kiosk { get; private set; }

	public DateTime Timestamp { get; private set; }

	public string CameraVersion { get; private set; }

	public DeviceStatus QuickReturn { get; private set; }

	public DeviceStatus AirExchanger { get; private set; }

	public DeviceStatus FraudDevice { get; private set; }

	public bool HasAuxRelayBoard { get; private set; }

	public DeviceStatus ABEDevice { get; private set; }

	public long Memory { get; private set; }

	public long FreeDiskSpace { get; private set; }

	public string PcModel { get; private set; }

	public string PcManufacturer { get; private set; }

	public string Touchscreen { get; private set; }

	public string TouchscreenFirmware { get; private set; }

	public string UpsModel { get; private set; }

	public string SerialControllerVersion { get; private set; }

	public string Monitor { get; private set; }

	protected override string JobName => "hardware-survey-job";

	public HardwareSurveyExecutor(HardwareService service)
		: base(service)
	{
	}

	protected override void OnJobCompleted()
	{
		Timestamp = DateTime.Now;
		foreach (ProgramResult result in base.Results)
		{
			switch (result.Code)
			{
			case "KioskId":
				Kiosk = result.Message;
				break;
			case "AuxRelayBoard":
				HasAuxRelayBoard = result.Message == "Configured";
				break;
			case "QuickReturnStatus":
				QuickReturn = Enum<DeviceStatus>.ParseIgnoringCase(result.Message, DeviceStatus.Unknown);
				break;
			case "AirExchanger":
				AirExchanger = Enum<DeviceStatus>.ParseIgnoringCase(result.Message, DeviceStatus.Unknown);
				break;
			case "FraudSensorEnabled":
				FraudDevice = Enum<DeviceStatus>.ParseIgnoringCase(result.Message, DeviceStatus.Unknown);
				break;
			case "AbeDeviceStatus":
				ABEDevice = Enum<DeviceStatus>.ParseIgnoringCase(result.Message, DeviceStatus.Unknown);
				break;
			case "PCManufacturer":
				PcManufacturer = result.Message;
				break;
			case "PCModel":
				PcModel = result.Message;
				break;
			case "InstalledMemory":
				Memory = long.Parse(result.Message);
				break;
			case "FreeDiskSpace":
				FreeDiskSpace = long.Parse(result.Message);
				break;
			case "CameraInfo":
				CameraVersion = result.Message;
				break;
			case "TouchscreenDevice":
				Touchscreen = result.Message;
				break;
			case "TouchscreenFirmware":
				TouchscreenFirmware = result.Message;
				break;
			case "ControllerFirmware":
				SerialControllerVersion = result.Message;
				break;
			case "MonitorInfo":
				Monitor = result.Message;
				break;
			case "UpsInfo":
				UpsModel = result.Message;
				break;
			}
		}
	}
}
