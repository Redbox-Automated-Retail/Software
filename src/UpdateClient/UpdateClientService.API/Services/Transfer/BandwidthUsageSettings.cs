using System;
using Microsoft.Win32;

namespace UpdateClientService.API.Services.Transfer;

public static class BandwidthUsageSettings
{
	private const string m_registryPath = "SOFTWARE\\Policies\\Microsoft\\Windows\\BITS";

	public static int MaxBandwidthWhileWithInSchedule
	{
		get
		{
			return Get<int>("MaxTransferRateOnSchedule");
		}
		set
		{
			Set(value, "MaxTransferRateOnSchedule");
		}
	}

	public static int MaxBandwidthWhileOutsideOfSchedule
	{
		get
		{
			return Get<int>("MaxTransferRateOffSchedule");
		}
		set
		{
			Set(value, "MaxTransferRateOffSchedule");
		}
	}

	public static byte StartOfScheduleInHoursFromMidnight
	{
		get
		{
			return (byte)Get<int>("MaxBandwidthValidFrom");
		}
		set
		{
			Set((int)value, "MaxBandwidthValidFrom");
		}
	}

	public static byte EndOfScheduleInHoursFromMidnight
	{
		get
		{
			return (byte)Get<int>("MaxBandwidthValidTo");
		}
		set
		{
			Set((int)value, "MaxBandwidthValidTo");
		}
	}

	public static bool UseSystemMaxOutsideOfSchedule
	{
		get
		{
			return Get<bool>("UseSystemMaximum");
		}
		set
		{
			Set(value, "UseSystemMaximum");
		}
	}

	public static bool EnableMaximumBandwitdthThrottle
	{
		get
		{
			return Get<bool>("EnableBITSMaxBandwidth");
		}
		set
		{
			Set(value, "EnableBITSMaxBandwidth");
		}
	}

	static BandwidthUsageSettings()
	{
		EnableMaximumBandwitdthThrottle = true;
		UseSystemMaxOutsideOfSchedule = false;
	}

	private static T Get<T>(string keyName)
	{
		using RegistryKey registryKey = Registry.LocalMachine.CreateSubKey("SOFTWARE\\Policies\\Microsoft\\Windows\\BITS");
		object value = registryKey.GetValue(keyName);
		if (value == null)
		{
			return default(T);
		}
		return (T)Convert.ChangeType(value, typeof(T));
	}

	private static void Set(object value, string keyName)
	{
		using RegistryKey registryKey = Registry.LocalMachine.CreateSubKey("SOFTWARE\\Policies\\Microsoft\\Windows\\BITS");
		registryKey.SetValue(keyName, value);
	}
}
