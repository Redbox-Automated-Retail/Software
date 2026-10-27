using System;
using System.Collections.Generic;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;

namespace Redbox.HAL.Client.Executors;

public sealed class MachineConfiguration : JobExecutor
{
	public bool IsDenseMachine { get; private set; }

	public ScannerServices ConfiguredCamera { get; private set; }

	public CameraGeneration CurrentCameraGeneration { get; private set; }

	public BarcodeServices BarcodeDecoder { get; private set; }

	public bool LegacyCamera { get; private set; }

	public bool VMZConfigured { get; private set; }

	public bool DoorSensorsConfigured { get; private set; }

	public bool HasQuickReturn
	{
		get
		{
			throw new NotImplementedException("HasQuickReturn property deprecated.");
		}
	}

	public AirExchangerStatus AirExchangerStatus { get; private set; }

	public ExchangerFanStatus AirExchangerFanStatus { get; private set; }

	public bool AirExchangerConfigured { get; private set; }

	public bool HasFraudDevice { get; private set; }

	public string DoorSensorStatus { get; private set; }

	public bool HasAuxRelayBoard { get; private set; }

	public bool HasABEDevice { get; private set; }

	public DeviceStatus QuickReturnStatus { get; private set; }

	public bool HasRouterPowerRelay { get; private set; }

	public bool ArcusResetConfigured { get; private set; }

	public DateTime? IRHardwareInstall { get; private set; }

	public bool SupportsFraudScan { get; private set; }

	public bool HasIRHardware => IRHardwareInstall.HasValue;

	public IList<Location> ExcludedSlots { get; private set; }

	public int MaxDeck { get; private set; }

	public int MaxSlot { get; set; }

	public int BufferSlot { get; set; }

	public bool KFCDisableCheckDrivers { get; set; }

	public bool KFCDisableDecodeTest { get; set; }

	public bool KFCDisableVendDoorTest { get; set; }

	public bool KFCDisableInit { get; set; }

	public bool KFCDisableVerticalSlotTest { get; set; }

	public bool KFCDisableUnknownCount { get; set; }

	protected override string JobName => "kiosk-configuration-job";

	public MachineConfiguration(HardwareService service)
		: base(service)
	{
	}

	protected override void OnJobCompleted()
	{
		ExcludedSlots = new List<Location>();
		base.Results.ForEach(delegate(ProgramResult result)
		{
			string code = result.Code;
			if (code != null)
			{
				switch (code.Length)
				{
				case 13:
					switch (code[0])
					{
					case 'A':
						if (code == "AuxRelayBoard")
						{
							HasAuxRelayBoard = result.Message == "Configured";
						}
						break;
					case 'M':
						if (code == "MachineConfig")
						{
							IsDenseMachine = result.Message == "Dense";
						}
						break;
					}
					break;
				case 12:
					switch (code[0])
					{
					case 'C':
						if (code == "CameraConfig")
						{
							LegacyCamera = result.Message == "Legacy";
							ConfiguredCamera = Enum<ScannerServices>.ParseIgnoringCase(result.Message, ScannerServices.Legacy);
						}
						break;
					case 'E':
						if (code == "ExcludedSlot")
						{
							ExcludedSlots.Add(new Location
							{
								Deck = result.Deck,
								Slot = result.Slot
							});
						}
						break;
					}
					break;
				case 23:
					switch (code[0])
					{
					case 'C':
						if (code == "CurrentCameraGeneration")
						{
							CurrentCameraGeneration = Enum<CameraGeneration>.ParseIgnoringCase(result.Message, CameraGeneration.Unknown);
						}
						break;
					case 'I':
						if (code == "IRCameraHardwareInstall" && result.Message != "NONE")
						{
							try
							{
								IRHardwareInstall = DateTime.Parse(result.Message);
								break;
							}
							catch
							{
								IRHardwareInstall = null;
								break;
							}
						}
						break;
					}
					break;
				case 14:
					switch (code[0])
					{
					case 'B':
						if (code == "BarcodeDecoder")
						{
							BarcodeDecoder = Enum<BarcodeServices>.ParseIgnoringCase(result.Message, BarcodeServices.None);
						}
						break;
					case 'D':
						if (code == "DisableKFCInit")
						{
							bool result3 = false;
							KFCDisableInit = bool.TryParse(result.Message, out result3) && result3;
						}
						break;
					}
					break;
				case 11:
					switch (code[0])
					{
					case 'D':
						if (code == "DoorSensors")
						{
							DoorSensorsConfigured = result.Message == "On";
						}
						break;
					case 'M':
						if (code == "MaxDeckInfo")
						{
							MaxDeck = result.Deck;
							MaxSlot = result.Slot;
						}
						break;
					}
					break;
				case 16:
					switch (code[0])
					{
					case 'D':
						if (code == "DoorSensorStatus")
						{
							DoorSensorStatus = result.Message;
						}
						break;
					case 'R':
						if (code == "RouterPowerRelay")
						{
							HasRouterPowerRelay = result.Message == "Configured";
						}
						break;
					}
					break;
				case 22:
					switch (code[10])
					{
					case 'e':
						if (code == "AirExchangerConfigured")
						{
							AirExchangerConfigured = result.Message == "Configured";
						}
						break;
					case 'C':
						if (code == "DisableKFCCheckDrivers")
						{
							bool result8 = false;
							KFCDisableCheckDrivers = bool.TryParse(result.Message, out result8) && result8;
						}
						break;
					case 'T':
						if (code == "DisableKFCTestVendDoor")
						{
							bool result7 = false;
							KFCDisableVendDoorTest = bool.TryParse(result.Message, out result7) && result7;
						}
						break;
					case 'U':
						if (code == "DisableKFCUnknownCount")
						{
							bool result6 = false;
							KFCDisableUnknownCount = bool.TryParse(result.Message, out result6) && result6;
						}
						break;
					}
					break;
				case 18:
					switch (code[0])
					{
					case 'F':
						if (code == "FraudSensorEnabled")
						{
							HasFraudDevice = result.Message == "Enabled";
						}
						break;
					case 'A':
						if (code == "AirExchangerStatus")
						{
							AirExchangerStatus = Enum<AirExchangerStatus>.ParseIgnoringCase(result.Message, AirExchangerStatus.NotConfigured);
						}
						break;
					case 'S':
						if (code == "SupportsArcusReset")
						{
							ArcusResetConfigured = result.Message == "Configured";
						}
						break;
					}
					break;
				case 9:
					if (code == "MerchMode")
					{
						VMZConfigured = result.Message == "VMZ";
					}
					break;
				case 15:
					if (code == "AbeDeviceStatus")
					{
						HasABEDevice = result.Message.Equals("ATTACHED", StringComparison.CurrentCultureIgnoreCase);
					}
					break;
				case 21:
					if (code == "AirExchangerFanStatus")
					{
						AirExchangerFanStatus = Enum<ExchangerFanStatus>.ParseIgnoringCase(result.Message, ExchangerFanStatus.On);
					}
					break;
				case 17:
					if (code == "SupportsFraudScan")
					{
						SupportsFraudScan = result.Message == "Configured";
					}
					break;
				case 10:
					if (code == "BufferSlot")
					{
						int result5 = 1;
						BufferSlot = ((!int.TryParse(result.Message, out result5)) ? 1 : result5);
					}
					break;
				case 20:
					if (code == "DisableKFCDecodeTest")
					{
						bool result4 = false;
						KFCDisableDecodeTest = bool.TryParse(result.Message, out result4) && result4;
					}
					break;
				case 26:
					if (code == "DisableKFCVerticalSlotTest")
					{
						bool result2 = false;
						KFCDisableVerticalSlotTest = bool.TryParse(result.Message, out result2) && result2;
					}
					break;
				case 19:
				case 24:
				case 25:
					break;
				}
			}
		});
	}
}
