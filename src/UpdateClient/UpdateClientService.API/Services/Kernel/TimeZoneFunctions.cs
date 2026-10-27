using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace UpdateClientService.API.Services.Kernel;

public static class TimeZoneFunctions
{
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

	public enum SetTimeResult
	{
		NotSet,
		InRange,
		Changed,
		Errored
	}

	public enum SetTimeZoneResult
	{
		NotSet,
		Same,
		Changed,
		Errored
	}

	[DllImport("kernel32.dll", EntryPoint = "SetSystemTime", SetLastError = true)]
	private static extern bool Win32SetSystemTime(ref SystemTime sysTime);

	[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
	private static extern bool SetDynamicTimeZoneInformation(ref DynamicTimeZoneInformation lpTimeZoneInformation);

	private static SystemTime ToSystemTime(this TimeZoneInfo.TransitionTime input)
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

	public static SetTimeResult SetTime(DateTime dateTime)
	{
		SetTimeResult setTimeResult = SetTimeResult.NotSet;
		DateTime utcNow = DateTime.UtcNow;
		if (dateTime.AddMinutes(-5.0) < utcNow && utcNow < dateTime.AddMinutes(5.0))
		{
			return SetTimeResult.InRange;
		}
		SystemTime sysTime = new SystemTime
		{
			Year = (ushort)dateTime.Year,
			Month = (ushort)dateTime.Month,
			Day = (ushort)dateTime.Day,
			Hour = (ushort)dateTime.Hour,
			Minute = (ushort)dateTime.Minute,
			Second = (ushort)dateTime.Second
		};
		if (Win32SetSystemTime(ref sysTime))
		{
			return SetTimeResult.Changed;
		}
		return SetTimeResult.Errored;
	}

	public static SetTimeZoneResult SetTimeZone(TimeZoneInfo timeZoneInfo)
	{
		SetTimeZoneResult setTimeZoneResult = SetTimeZoneResult.NotSet;
		if (TimeZoneInfo.Local.Equals(timeZoneInfo))
		{
			setTimeZoneResult = SetTimeZoneResult.Same;
		}
		else
		{
			TimeZoneInfo.AdjustmentRule[] adjustmentRules = timeZoneInfo.GetAdjustmentRules();
			DateTime utcNow = DateTime.UtcNow;
			DynamicTimeZoneInformation info = new DynamicTimeZoneInformation
			{
				StandardName = timeZoneInfo.StandardName,
				DaylightName = timeZoneInfo.DaylightName,
				StandardBias = 0,
				Bias = (int)timeZoneInfo.BaseUtcOffset.Negate().TotalMinutes,
				TimeZoneKeyName = timeZoneInfo.StandardName,
				DynamicDaylightTimeDisabled = false
			};
			TimeZoneInfo.AdjustmentRule currentAdjustmentRule = adjustmentRules.FirstOrDefault((TimeZoneInfo.AdjustmentRule ar) => ar.DateStart < utcNow && ar.DateEnd > utcNow);
			SetAdjustmentRules(ref info, currentAdjustmentRule);
			TokenPrivilegesAccess.EnablePrivilege("SeTimeZonePrivilege");
			setTimeZoneResult = ((!SetDynamicTimeZoneInformation(ref info)) ? SetTimeZoneResult.Errored : SetTimeZoneResult.Changed);
			TokenPrivilegesAccess.DisablePrivilege("SeTimeZonePrivilege");
		}
		return setTimeZoneResult;
	}
}
