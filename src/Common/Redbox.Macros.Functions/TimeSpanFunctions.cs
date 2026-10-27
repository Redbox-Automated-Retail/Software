using System;

namespace Redbox.Macros.Functions;

[FunctionSet("timespan", "Date/Time")]
public class TimeSpanFunctions : FunctionSetBase
{
	public TimeSpanFunctions(PropertyDictionary properties)
		: base(properties)
	{
	}

	[Function("get-total-days")]
	public static double GetTotalDays(TimeSpan value)
	{
		return value.TotalDays;
	}

	[Function("get-total-hours")]
	public static double GetTotalHours(TimeSpan value)
	{
		return value.TotalHours;
	}

	[Function("get-total-minutes")]
	public static double GetTotalMinutes(TimeSpan value)
	{
		return value.TotalMinutes;
	}

	[Function("get-total-seconds")]
	public static double GetTotalSeconds(TimeSpan value)
	{
		return value.TotalSeconds;
	}

	[Function("get-total-milliseconds")]
	public static double GetTotalMilliseconds(TimeSpan value)
	{
		return value.TotalMilliseconds;
	}

	[Function("get-days")]
	public static int GetDays(TimeSpan value)
	{
		return value.Days;
	}

	[Function("get-hours")]
	public static int GetHours(TimeSpan value)
	{
		return value.Hours;
	}

	[Function("get-minutes")]
	public static int GetMinutes(TimeSpan value)
	{
		return value.Minutes;
	}

	[Function("get-seconds")]
	public static int GetSeconds(TimeSpan value)
	{
		return value.Seconds;
	}

	[Function("get-milliseconds")]
	public static int GetMilliseconds(TimeSpan value)
	{
		return value.Milliseconds;
	}

	[Function("get-ticks")]
	public static long GetTicks(TimeSpan value)
	{
		return value.Ticks;
	}

	[Function("from-days")]
	public static TimeSpan FromDays(double value)
	{
		return TimeSpan.FromDays(value);
	}

	[Function("from-hours")]
	public static TimeSpan FromHours(double value)
	{
		return TimeSpan.FromHours(value);
	}

	[Function("from-minutes")]
	public static TimeSpan FromMinutes(double value)
	{
		return TimeSpan.FromMinutes(value);
	}

	[Function("from-seconds")]
	public static TimeSpan FromSeconds(double value)
	{
		return TimeSpan.FromSeconds(value);
	}

	[Function("from-milliseconds")]
	public static TimeSpan FromMilliseconds(double value)
	{
		return TimeSpan.FromMilliseconds(value);
	}

	[Function("from-ticks")]
	public static TimeSpan FromTicks(long value)
	{
		return TimeSpan.FromTicks(value);
	}
}
