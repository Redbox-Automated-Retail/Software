using Microsoft.Win32;
using Redbox.Core;

namespace Redbox.UpdateManager.BITS;

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
		using RegistryKey key = Registry.LocalMachine.CreateSubKey("SOFTWARE\\Policies\\Microsoft\\Windows\\BITS");
		object registryValue = key.GetValue(keyName);
		if (registryValue == null)
		{
			return default(T);
		}
		return (T)ConversionHelper.ChangeType(registryValue, typeof(T));
	}

	private static void Set(object value, string keyName)
	{
		using RegistryKey key = Registry.LocalMachine.CreateSubKey("SOFTWARE\\Policies\\Microsoft\\Windows\\BITS");
		key.SetValue(keyName, value);
	}
}
