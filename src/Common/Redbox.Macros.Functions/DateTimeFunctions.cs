using System;

namespace Redbox.Macros.Functions;

[FunctionSet("datetime", "Date/Time")]
public class DateTimeFunctions : FunctionSetBase
{
	public DateTimeFunctions(PropertyDictionary properties)
		: base(properties)
	{
	}

	[Function("now")]
	public static DateTime Now()
	{
		return DateTime.Now;
	}

	[Function("get-year")]
	public static int GetYear(DateTime date)
	{
		return date.Year;
	}

	[Function("get-month")]
	public static int GetMonth(DateTime date)
	{
		return date.Month;
	}

	[Function("get-day")]
	public static int GetDay(DateTime date)
	{
		return date.Day;
	}

	[Function("get-hour")]
	public static int GetHour(DateTime date)
	{
		return date.Hour;
	}

	[Function("get-minute")]
	public static int GetMinute(DateTime date)
	{
		return date.Minute;
	}

	[Function("get-second")]
	public static int GetSecond(DateTime date)
	{
		return date.Second;
	}

	[Function("get-millisecond")]
	public static int GetMillisecond(DateTime date)
	{
		return date.Millisecond;
	}

	[Function("get-ticks")]
	public static long GetTicks(DateTime date)
	{
		return date.Ticks;
	}

	[Function("get-day-of-week")]
	public static int GetDayOfWeek(DateTime date)
	{
		return (int)date.DayOfWeek;
	}

	[Function("get-day-of-year")]
	public static int GetDayOfYear(DateTime date)
	{
		return date.DayOfYear;
	}

	[Function("get-days-in-month")]
	public static int GetDaysInMonth(int year, int month)
	{
		return DateTime.DaysInMonth(year, month);
	}

	[Function("is-leap-year")]
	public static bool IsLeapYear(int year)
	{
		return DateTime.IsLeapYear(year);
	}
}
