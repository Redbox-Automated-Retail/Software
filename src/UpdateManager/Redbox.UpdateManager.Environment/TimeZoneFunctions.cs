using System;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using Redbox.Core;

namespace Redbox.UpdateManager.Environment;

public static class TimeZoneFunctions
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	private struct TimeZoneInformation
	{
		[MarshalAs(UnmanagedType.I4)]
		public int Bias;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
		public string StandardName;

		public SystemTime StandardDate;

		[MarshalAs(UnmanagedType.I4)]
		public int StandardBias;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
		public string DaylightName;

		public SystemTime DaylightDate;

		[MarshalAs(UnmanagedType.I4)]
		public int DaylightBias;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	private struct DynamicTimeZoneInformation
	{
		[MarshalAs(UnmanagedType.I4)]
		public int Bias;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
		public string StandardName;

		public SystemTime StandardDate;

		[MarshalAs(UnmanagedType.I4)]
		public int StandardBias;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
		public string DaylightName;

		public SystemTime DaylightDate;

		[MarshalAs(UnmanagedType.I4)]
		public int DaylightBias;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
		public string TimeZoneKeyName;

		public bool DynamicDaylightTimeDisabled;
	}

	private struct SystemTime
	{
		public ushort Year;

		public ushort Month;

		public ushort DayOfWeek;

		public ushort Day;

		public ushort Hour;

		public ushort Minute;

		public ushort Second;

		public ushort Millisecond;
	}

	private static readonly bool IsOsVistaOrGreater = System.Environment.OSVersion.Version.Major >= 6;

	public static bool SetTime(DateTime dateTime)
	{
		if (dateTime.AddMinutes(-5.0) < DateTime.UtcNow && DateTime.UtcNow < dateTime.AddMinutes(5.0))
		{
			LogHelper.Instance.Log("Not changing time because the set time and the current time are within 5 minutes.", LogEntryType.Debug);
			return true;
		}
		try
		{
			SystemTime sysTime = new SystemTime
			{
				Year = (ushort)dateTime.Year,
				Month = (ushort)dateTime.Month,
				Day = (ushort)dateTime.Day,
				Hour = (ushort)dateTime.Hour,
				Minute = (ushort)dateTime.Minute,
				Second = (ushort)dateTime.Second
			};
			LogHelper.Instance.Log("Attempting to change date and time (UTC) from " + DateTime.UtcNow.ToLongDateString() + " " + DateTime.UtcNow.ToLongTimeString() + " to " + dateTime.ToLongDateString() + " " + dateTime.ToLongTimeString(), LogEntryType.Debug);
			bool result = Win32SetSystemTime(ref sysTime);
			LogHelper.Instance.Log("Timechange success: " + result + " - current date and time (UTC) is: " + DateTime.UtcNow.ToLongDateString() + " " + DateTime.UtcNow.ToLongTimeString(), LogEntryType.Debug);
			return result;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Unhandled exception in TimeZoneFunctions.SetTime", e);
			return false;
		}
	}

	public static bool SetTimeZone(TimeZoneInfo timeZoneInfo)
	{
		if (TimeZoneInfo.Local.Equals(timeZoneInfo))
		{
			return true;
		}
		try
		{
			TimeZoneInfo.AdjustmentRule currentAdjustmentRule = timeZoneInfo.GetAdjustmentRules().FirstOrDefault((TimeZoneInfo.AdjustmentRule ar) => ar.DateStart < DateTime.UtcNow && ar.DateEnd > DateTime.UtcNow);
			bool flag = false;
			LogHelper.Instance.Log("Attempting to change timezone from " + TimeZoneInfo.Local.DisplayName + " to " + timeZoneInfo.DisplayName, LogEntryType.Debug);
			if (IsOsVistaOrGreater)
			{
				DynamicTimeZoneInformation info = new DynamicTimeZoneInformation
				{
					StandardName = timeZoneInfo.StandardName,
					DaylightName = timeZoneInfo.DaylightName,
					StandardBias = 0,
					Bias = (int)timeZoneInfo.BaseUtcOffset.Negate().TotalMinutes,
					TimeZoneKeyName = timeZoneInfo.StandardName,
					DynamicDaylightTimeDisabled = false
				};
				SetAdjustmentRules(ref info, currentAdjustmentRule);
				TokenPrivilegesAccess.EnablePrivilege("SeTimeZonePrivilege");
				flag = SetDynamicTimeZoneInformation(ref info);
				TokenPrivilegesAccess.DisablePrivilege("SeTimeZonePrivilege");
			}
			else
			{
				TimeZoneInformation info2 = new TimeZoneInformation
				{
					StandardName = timeZoneInfo.StandardName,
					DaylightName = timeZoneInfo.DaylightName,
					StandardBias = 0,
					Bias = (int)timeZoneInfo.BaseUtcOffset.Negate().TotalMinutes
				};
				SetAdjustmentRules(ref info2, currentAdjustmentRule);
				flag = SetTimeZoneInformation(ref info2);
				DeleteDisableAutoDaylightTimeSet();
			}
			LogHelper.Instance.Log("TimeZone change success: " + flag, LogEntryType.Debug);
			return flag;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Unhandled exception in SetTimeZone", e, LogEntryType.Error);
			return false;
		}
	}

	private static void SetAdjustmentRules(ref TimeZoneInformation info, TimeZoneInfo.AdjustmentRule currentAdjustmentRule)
	{
		if (currentAdjustmentRule == null)
		{
			info.DaylightDate = (info.StandardDate = new SystemTime
			{
				Year = 0,
				Month = 0,
				DayOfWeek = 0,
				Day = 0,
				Hour = 0,
				Minute = 0,
				Second = 0,
				Millisecond = 0
			});
			info.DaylightBias = 0;
		}
		else
		{
			info.StandardDate = currentAdjustmentRule.DaylightTransitionEnd.ToSystemTime();
			info.DaylightDate = currentAdjustmentRule.DaylightTransitionStart.ToSystemTime();
			info.DaylightBias = (int)currentAdjustmentRule.DaylightDelta.Negate().TotalMinutes;
		}
	}

	private static void SetAdjustmentRules(ref DynamicTimeZoneInformation info, TimeZoneInfo.AdjustmentRule currentAdjustmentRule)
	{
		if (currentAdjustmentRule == null)
		{
			info.DaylightDate = (info.StandardDate = new SystemTime
			{
				Year = 0,
				Month = 0,
				DayOfWeek = 0,
				Day = 0,
				Hour = 0,
				Minute = 0,
				Second = 0,
				Millisecond = 0
			});
			info.DaylightBias = 0;
		}
		else
		{
			info.StandardDate = currentAdjustmentRule.DaylightTransitionEnd.ToSystemTime();
			info.DaylightDate = currentAdjustmentRule.DaylightTransitionStart.ToSystemTime();
			info.DaylightBias = (int)currentAdjustmentRule.DaylightDelta.Negate().TotalMinutes;
		}
	}

	private static SystemTime ToSystemTime(this TimeZoneInfo.TransitionTime input)
	{
		try
		{
			SystemTime result = new SystemTime
			{
				Year = 0,
				Month = (ushort)input.Month,
				Day = (input.IsFixedDateRule ? ((ushort)input.Day) : ((ushort)input.Week)),
				Hour = (ushort)input.TimeOfDay.Hour,
				Second = (ushort)input.TimeOfDay.Second,
				Minute = (ushort)input.TimeOfDay.Minute,
				Millisecond = (ushort)input.TimeOfDay.Millisecond
			};
			switch (input.DayOfWeek)
			{
			case DayOfWeek.Sunday:
				result.DayOfWeek = 0;
				break;
			case DayOfWeek.Monday:
				result.DayOfWeek = 1;
				break;
			case DayOfWeek.Tuesday:
				result.DayOfWeek = 2;
				break;
			case DayOfWeek.Wednesday:
				result.DayOfWeek = 3;
				break;
			case DayOfWeek.Thursday:
				result.DayOfWeek = 4;
				break;
			case DayOfWeek.Friday:
				result.DayOfWeek = 5;
				break;
			case DayOfWeek.Saturday:
				result.DayOfWeek = 6;
				break;
			}
			return result;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Unhandles exception in TimeZoneInfo.TransitionTime.ToSystemTime.", e);
			return default(SystemTime);
		}
	}

	private static void DeleteDisableAutoDaylightTimeSet()
	{
		try
		{
			RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\TimeZoneInformation", writable: true);
			if (registryKey != null)
			{
				registryKey.DeleteValue("DisableAutoDaylightTimeSet", throwOnMissingValue: false);
				registryKey.Close();
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("There was an error in deleting the DisableAutoDaylightTimeSet registry value", e, LogEntryType.Error);
		}
	}

	[DllImport("Kernel32.dll", EntryPoint = "SetSystemTime", SetLastError = true)]
	private static extern bool Win32SetSystemTime(ref SystemTime sysTime);

	[DllImport("Kernel32.dll", CharSet = CharSet.Auto)]
	private static extern bool SetTimeZoneInformation(ref TimeZoneInformation lpTimeZoneInformation);

	[DllImport("Kernel32.dll", CharSet = CharSet.Auto)]
	private static extern bool SetDynamicTimeZoneInformation(ref DynamicTimeZoneInformation lpTimeZoneInformation);
}
