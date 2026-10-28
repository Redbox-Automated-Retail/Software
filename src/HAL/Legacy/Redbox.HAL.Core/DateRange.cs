using System;

namespace Redbox.HAL.Core;

public class DateRange : IRange<DateTime>
{
	private string m_formatted;

	public DateTime Start { get; private set; }

	public DateTime End { get; private set; }

	public static DateRange FromHourSpan(int start, int end)
	{
		DateTime now = DateTime.Now;
		DateTime dateTime = DateTime.Parse(end + ":00");
		DateTime start2 = DateTime.Parse(start + ":00");
		if (now.Hour <= start)
		{
			if (end < start)
			{
				dateTime = dateTime.AddDays(1.0);
			}
			return new DateRange(start2, dateTime);
		}
		if (end < start)
		{
			dateTime = dateTime.AddDays(1.0);
		}
		if (now >= dateTime)
		{
			start2 = start2.AddDays(1.0);
			dateTime = dateTime.AddDays(1.0);
		}
		return new DateRange(start2, dateTime);
	}

	public static bool NowIsBetweenHours(int start, int end)
	{
		return new DateRange(start, end).Includes(DateTime.Now);
	}

	public bool Includes(DateTime value)
	{
		if (Start <= value)
		{
			return value <= End;
		}
		return false;
	}

	public bool Includes(IRange<DateTime> range)
	{
		if (Start <= range.Start)
		{
			return range.End <= End;
		}
		return false;
	}

	public override string ToString()
	{
		if (m_formatted == null)
		{
			m_formatted = $"Start {Start.ToShortDateString()} {Start.ToShortTimeString()} -> End {End.ToShortDateString()} {End.ToShortTimeString()}";
		}
		return m_formatted;
	}

	public bool PriorToStart(DateTime time)
	{
		return time <= Start;
	}

	public void ShiftOneDay()
	{
		Start = Start.AddDays(1.0);
		End = End.AddDays(1.0);
		m_formatted = null;
	}

	internal DateRange(DateTime start, DateTime end)
	{
		Start = start;
		End = end;
	}

	private DateRange(int start, int end)
	{
		DateTime now = DateTime.Now;
		Start = new DateTime(now.Year, now.Month, now.Day, start, 0, 0);
		End = new DateTime(now.Year, now.Month, now.Day, end, 0, 0);
	}
}
