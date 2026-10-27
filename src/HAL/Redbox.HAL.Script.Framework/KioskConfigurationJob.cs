using System.Collections.Generic;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "kiosk-configuration-job", Operand = "KIOSK-CONFIGURATION-JOB")]
internal sealed class KioskConfigurationJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		IScannerDevice configuredDevice = ServiceLocator.Instance.GetService<IScannerDeviceService>().GetConfiguredDevice();
		IDoorSensorService service = ServiceLocator.Instance.GetService<IDoorSensorService>();
		IDeck first = base.DecksService.First;
		base.Context.CreateInfoResult("MachineConfig", (first.NumberOfSlots == 90) ? "Dense" : "Sparse");
		base.Context.CreateInfoResult("DeckCount", base.DecksService.DeckCount.ToString());
		IDeck deck = base.DecksService.Last;
		if (deck != null)
		{
			if (deck.IsQlm)
			{
				deck = base.DecksService.GetByNumber(deck.Number - 1);
			}
			base.Context.CreateResult("MaxDeckInfo", "This contains the last deck and slot.", deck.Number, deck.NumberOfSlots, null, null, null);
		}
		base.Context.CreateInfoResult("CameraConfig", configuredDevice.Service.ToString());
		base.Context.CreateInfoResult("AuxRelayBoard", "Not Configured");
		base.Context.CreateInfoResult("MerchMode", ControllerConfiguration.Instance.IsVMZMachine ? "VMZ" : "QLM");
		base.Context.CreateInfoResult("DoorSensors", service.SensorsEnabled ? "On" : "Off");
		if (service.SensorsEnabled)
		{
			base.Context.CreateInfoResult("DoorSensorStatus", service.Query().ToString().ToUpper());
		}
		base.Context.CreateInfoResult("QuickReturn", "Not configured");
		IAirExchangerService service2 = ServiceLocator.Instance.GetService<IAirExchangerService>();
		base.Context.CreateInfoResult("AirExchangerConfigured", service2.Configured ? "Configured" : "Not configured");
		base.Context.CreateInfoResult("AirExchangerStatus", service2.CheckStatus().ToString());
		base.Context.CreateInfoResult("AirExchangerFanStatus", service2.FanStatus.ToString());
		IUsbDeviceService service3 = ServiceLocator.Instance.GetService<IUsbDeviceService>();
		base.Context.CreateInfoResult("FraudSensorEnabled", ControllerConfiguration.Instance.EnableSecureDiskValidator ? "Enabled" : "Not Enabled");
		AbeDeviceDescriptor abeDeviceDescriptor = new AbeDeviceDescriptor(service3, debug: true);
		base.Context.CreateInfoResult("AbeDeviceStatus", abeDeviceDescriptor.Locate() ? "ATTACHED" : "NOT ATTACHED");
		base.Context.CreateInfoResult("QuickReturnStatus", DeviceStatus.None.ToString());
		base.Context.CreateInfoResult("RouterPowerRelay", (ControllerConfiguration.Instance.RouterPowerCyclePause > 0) ? "Configured" : "Not Configured");
		base.Context.CreateInfoResult("SupportsArcusReset", (ControllerConfiguration.Instance.RestartControllerDuringUserJobs && ControllerConfiguration.Instance.TrackHardwareCorrections) ? "Configured" : "Not Configured");
		IFraudService service4 = ServiceLocator.Instance.GetService<IFraudService>();
		base.Context.CreateInfoResult("SupportsFraudScan", service4.IsConfigured ? "Configured" : "Not Configured");
		IScannerDeviceService service5 = ServiceLocator.Instance.GetService<IScannerDeviceService>();
		base.Context.CreateInfoResult("IRCameraHardwareInstall", service5.IRHardwareInstall.HasValue ? service5.IRHardwareInstall.ToString() : "NONE");
		base.Context.CreateInfoResult("CurrentCameraGeneration", service5.CurrentCameraGeneration.ToString());
		IBarcodeReader configuredReader = ServiceLocator.Instance.GetService<IBarcodeReaderFactory>().GetConfiguredReader();
		base.Context.CreateInfoResult("BarcodeDecoder", configuredReader.IsLicensed ? configuredReader.Service.ToString() : BarcodeServices.None.ToString());
		List<ILocation> excludedSlots = base.InventoryService.GetExcludedSlots();
		if (excludedSlots.Count > 0)
		{
			excludedSlots.ForEach(delegate(ILocation x)
			{
				base.Context.CreateResult("ExcludedSlot", "The location is excluded.", x.Deck, x.Slot, null, null, null);
			});
		}
		else
		{
			base.Context.CreateInfoResult("NoExcludedSlots", "There are no excluded slots.");
		}
		base.Context.CreateInfoResult("BufferSlot", ControllerConfiguration.Instance.ReturnSlotBuffer.ToString());
		base.Context.CreateInfoResult("DisableKFCCheckDrivers", ControllerConfiguration.Instance.DisableKFCCheckDrivers.ToString());
		base.Context.CreateInfoResult("DisableKFCDecodeTest", ControllerConfiguration.Instance.DisableKFCDecodeTest.ToString());
		base.Context.CreateInfoResult("DisableKFCTestVendDoor", ControllerConfiguration.Instance.DisableKFCTestVendDoor.ToString());
		base.Context.CreateInfoResult("DisableKFCInit", ControllerConfiguration.Instance.DisableKFCInit.ToString());
		base.Context.CreateInfoResult("DisableKFCVerticalSlotTest", ControllerConfiguration.Instance.DisableKFCVerticalSlotTest.ToString());
		base.Context.CreateInfoResult("DisableKFCUnknownCount", ControllerConfiguration.Instance.DisableKFCUnknownCount.ToString());
	}

	internal KioskConfigurationJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
