using System;
using System.Globalization;
using System.Text;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;

namespace Redbox.KioskEngine.Kernel;

public static class DateFunctions
{
	[KernelFunction(Name = "kernel.dateadd")]
	internal static LuaTable DateAdd(LuaTable date, string interval)
	{
		DateTime? workingDate = date.ToDateTime();
		string s = string.Empty;
		int num = 0;
		int num2 = 0;
		if (!workingDate.HasValue)
		{
			return null;
		}
		string[] array = interval.Split(new char[1] { '.' }, StringSplitOptions.RemoveEmptyEntries);
		switch (array.Length)
		{
		case 1:
			s = array[0];
			break;
		case 2:
			s = $"{array[0]}.{array[1]}";
			break;
		case 3:
			s = $"{array[1]}.{array[2]}";
			num = int.Parse(array[0]);
			break;
		case 4:
			s = $"{array[2]}.{array[3]}";
			num = int.Parse(array[1]);
			num2 = int.Parse(array[0]);
			break;
		}
		if (!TimeSpan.TryParse(s, out var result))
		{
			return null;
		}
		if (num2 > 0)
		{
			workingDate = workingDate.Value.AddYears(num2);
		}
		if (num > 0)
		{
			workingDate = workingDate.Value.AddMonths(num);
		}
		date.Dispose();
		workingDate = workingDate.Value.Add(result);
		return ToLuaTable(workingDate);
	}

	private static LuaTable ToLuaTable(DateTime? workingDate)
	{
		return new LuaTable(KernelService.Instance.LuaRuntime)
		{
			["month"] = workingDate.Value.Month,
			["day"] = workingDate.Value.Day,
			["year"] = workingDate.Value.Year,
			["hour"] = workingDate.Value.Hour,
			["min"] = workingDate.Value.Minute,
			["sec"] = workingDate.Value.Second,
			["wday"] = (int)(workingDate.Value.DayOfWeek + 1)
		};
	}

	[KernelFunction(Name = "kernel.datecompare")]
	internal static int DateCompare(LuaTable left, LuaTable right)
	{
		DateTime? dateTime = left.ToDateTime();
		DateTime? dateTime2 = right.ToDateTime();
		int result;
		try
		{
			if (!dateTime.HasValue || !dateTime.HasValue)
			{
				throw new ArgumentNullException("left");
			}
			if (!dateTime2.HasValue || !dateTime2.HasValue)
			{
				throw new ArgumentNullException("right");
			}
		}
		finally
		{
			result = dateTime.Value.CompareTo(dateTime2.Value);
			left.Dispose();
			right.Dispose();
		}
		return result;
	}

	[KernelFunction(Name = "kernel.stringdatecompare")]
	internal static int StringDateCompare(string left, string right)
	{
		if (DateTime.TryParse(left, out var result) && DateTime.TryParse(right, out var result2))
		{
			return result.CompareTo(result2);
		}
		return 1;
	}

	[KernelFunction(Name = "kernel.getelapseddays")]
	public static object GetElapsedDays(string startDateString, string endDateString)
	{
		string[] formats = new string[3] { "yyyyMMdd", "yyyyMMddHHmm", "yyyyMMddHHmmss" };
		if (DateTime.TryParseExact(startDateString, formats, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out var result) && DateTime.TryParseExact(endDateString, formats, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out var result2))
		{
			return (result2 - result).TotalDays;
		}
		return null;
	}

	public static bool ParseDateString(string dateString, out DateTime date)
	{
		string[] formats = new string[3] { "yyyyMMdd", "yyyyMMddHHmm", "yyyyMMddHHmmss" };
		bool result = DateTime.TryParseExact(dateString, formats, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out var result2);
		date = result2;
		return result;
	}

	[KernelFunction(Name = "kernel.getelapsedhours")]
	internal static object GetElapsedHours(string startDateString, string endDateString)
	{
		string[] formats = new string[3] { "yyyyMMdd", "yyyyMMddHHmm", "yyyyMMddHHmmss" };
		if (DateTime.TryParseExact(startDateString, formats, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out var result) && DateTime.TryParseExact(endDateString, formats, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out var result2))
		{
			return (result2 - result).TotalHours;
		}
		return null;
	}

	[KernelFunction(Name = "kernel.datetimeparsefromstringdate")]
	internal static LuaTable ParseFromStringDateTime(string datetime)
	{
		string[] formats = new string[3] { "yyyyMMdd", "yyyyMMddHHmm", "yyyyMMddHHmmss" };
		if (DateTime.TryParseExact(datetime, formats, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out var result))
		{
			return ToLuaTable(result);
		}
		return null;
	}

	[KernelFunction(Name = "kernel.datetimeparsefromstringtime")]
	internal static LuaTable ParseFromStringTime(string time)
	{
		DateTime value;
		try
		{
			value = DateTime.Parse(time, new CultureInfo("en-US"));
		}
		catch
		{
			return null;
		}
		return ToLuaTable(value);
	}

	[KernelFunction(Name = "kernel.gettimezone")]
	internal static string GetTimezone()
	{
		string obj = (TimeZone.CurrentTimeZone.IsDaylightSavingTime(DateTime.Now) ? TimeZone.CurrentTimeZone.DaylightName : TimeZone.CurrentTimeZone.StandardName);
		StringBuilder stringBuilder = new StringBuilder();
		string[] array = obj.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
		foreach (string text in array)
		{
			stringBuilder.Append(text[0]);
		}
		return stringBuilder.ToString();
	}

	[KernelFunction(Name = "kernel.tolocaltime")]
	internal static string ToLocalTime(string datetime, string format)
	{
		string[] formats = new string[3] { "yyyyMMdd", "yyyyMMddHHmm", "yyyyMMddHHmmss" };
		if (DateTime.TryParseExact(datetime, formats, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AssumeUniversal, out var result))
		{
			return result.ToLocalTime().ToString(format);
		}
		return null;
	}
}
