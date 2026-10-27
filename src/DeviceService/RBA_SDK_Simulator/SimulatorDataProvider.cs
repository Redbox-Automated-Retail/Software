using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;

namespace RBA_SDK_Simulator;

public class SimulatorDataProvider : ISimulatorDataProvider
{
	private readonly ConcurrentDictionary<string, string> _configuration = new ConcurrentDictionary<string, string>();

	private readonly ConcurrentDictionary<string, string> _variables = new ConcurrentDictionary<string, string>();

	private readonly ConcurrentDictionary<string, string> _files = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

	private int _rebootCount;

	private int _swipeCount;

	public IDictionary<string, string> UnitData { get; } = new ConcurrentDictionary<string, string>
	{
		["P07_RES_MANUFACTURE"] = "INGENICO",
		["P07_RES_DEVICE"] = "iUC285",
		["P07_RES_UNIT_SERIAL_NUMBER"] = "SIM00000001",
		["P07_RES_RAM_SIZE"] = "512",
		["P07_RES_FLASH_SIZE"] = "512",
		["P07_RES_DIGITIZER_VERSION"] = "0",
		["P07_RES_SECURITY_MODULE_VERSION"] = "SIM",
		["P07_RES_OS_VERSION"] = "SIM-OS",
		["P07_RES_APPLICATION_VERSION"] = "23020",
		["P07_RES_EFTL_VERSION"] = "0",
		["P07_RES_EFTP_VERSION"] = "0",
		["P07_RES_MANUFACTURING_SERIAL_NUMBER"] = "SIM0000000001",
		["P07_RES_EMV_DC_KERNEL_TYPE"] = "1",
		["P07_RES_EMV_ENGINE_KERNEL_TYPE"] = "1",
		["P07_RES_CLESS_DISCOVER_KERNEL_TYPE"] = "1",
		["P07_RES_CLESS_EXPRESSPAY_V3_KERNEL_TYPE"] = "1",
		["P07_RES_CLESS_EXPRESSPAY_V2_KERNEL_TYPE"] = "1",
		["P07_RES_CLESS_PAYPASS_V3_KERNEL_TYPE"] = "1",
		["P07_RES_CLESS_PAYPASS_V3_APP_TYPE"] = "1",
		["P07_RES_CLESS_VISA_PAYWAVE_KERNEL_TYPE"] = "1",
		["P07_RES_CLESS_INTERAC_KERNEL_TYPE"] = "0",
		["P07_RES_CLESS_INTERFACE_IS_SUPPORTED"] = "1"
	};

	public IDictionary<string, string> HealthData { get; } = new ConcurrentDictionary<string, string>
	{
		["P08_RES_COUNT_MSR_SWIPES"] = "0",
		["P08_RES_COUNT_REBOOT"] = "0",
		["P08_RES_COUNT_BAD_TRACK1_READS"] = "0",
		["P08_RES_COUNT_BAD_TRACK2_READS"] = "0",
		["P08_RES_COUNT_BAD_TRACK3_READS"] = "0",
		["P08_RES_DEVICE_NAME"] = "iUC285",
		["P08_RES_SERIAL_NUMBER"] = "SIM00000001",
		["P08_RES_OS_VERSION"] = "SIM-OS",
		["P08_RES_APP_VERSION"] = "23020",
		["P08_RES_SECURITY_LIB_VERSION"] = "SIM",
		["P08_RES_TDA_VERSION"] = "SIM",
		["P08_RES_EFTL_VERSION"] = "0",
		["P08_RES_EFTP_VERSION"] = "0",
		["P08_RES_RAM_SIZE"] = "512",
		["P08_RES_FLASH_SIZE"] = "512",
		["P08_RES_MANUFACTURE_DATE"] = "010120",
		["P08_RES_CPEM_TYPE"] = "0",
		["P08_RES_APP_NAME"] = "RBA",
		["P08_RES_MANUFACTURE_ID"] = "INGENICO",
		["P08_RES_DIGITIZER_VERSION"] = "0",
		["P08_RES_MANUFACTURING_SERIAL_NUMBER"] = "SIM0000000001"
	};

	public bool Tampered { get; set; }

	public bool DeviceAttached { get; set; } = true;

	public SimulatedCard DefaultCard { get; set; } = SimulatedCard.Preset("Visa");

	public int ResponseDelay { get; set; } = 100;

	public IDictionary<string, string> Configuration => _configuration;

	public IDictionary<string, string> Variables => _variables;

	public IDictionary<string, string> Files => _files;

	public SimulatorDataProvider()
	{
		// 14.1 is the EMV (TGZ) parameter version; DeviceService only enables EMV when it is non-zero.
		WriteConfig("14", "1", "1.0.0");
	}

	private static string Key(string group, string index)
	{
		return $"{Normalize(group)}.{Normalize(index)}";
	}

	private static string Normalize(string number)
	{
		if (!int.TryParse(number, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result))
		{
			return number;
		}
		return result.ToString(CultureInfo.InvariantCulture);
	}

	public string ReadConfig(string group, string index)
	{
		if (!_configuration.TryGetValue(Key(group, index), out string value))
		{
			return null;
		}
		return value;
	}

	public void WriteConfig(string group, string index, string value)
	{
		_configuration[Key(group, index)] = value ?? string.Empty;
	}

	public string GetVariable(string id)
	{
		if (!_variables.TryGetValue(Normalize(id), out string value))
		{
			return null;
		}
		return value;
	}

	public void SetVariable(string id, string value)
	{
		_variables[Normalize(id)] = value ?? string.Empty;
	}

	public void CountReboot()
	{
		System.Threading.Interlocked.Increment(ref _rebootCount);
		HealthData["P08_RES_COUNT_REBOOT"] = _rebootCount.ToString(CultureInfo.InvariantCulture);
	}

	public void CountSwipe()
	{
		System.Threading.Interlocked.Increment(ref _swipeCount);
		HealthData["P08_RES_COUNT_MSR_SWIPES"] = _swipeCount.ToString(CultureInfo.InvariantCulture);
	}
}
