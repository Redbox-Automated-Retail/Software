using System;
using System.Timers;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Core;

public sealed class RedboxTimer
{
	private readonly Timer Timer;

	public bool Started => Timer.Enabled;

	public string Name { get; private set; }

	public DateTime? NextFireTime { get; private set; }

	public void ScheduleAtNext(int hour, int minute)
	{
		TimeSpan span = new TimeSpan(hour, minute, 0);
		ScheduleAtNextInner(span);
	}

	public void ScheduleAtNext(TimeSpan span)
	{
		ScheduleAtNextInner(span);
	}

	public void FireOn(DateTime time)
	{
		DateTime now = DateTime.Now;
		StartTimer((time - now).TotalMilliseconds);
		LogHelper.Instance.Log("Timer should fire on {0} at {1}", time.ToShortDateString(), time.ToShortTimeString());
	}

	public void Disable()
	{
		Timer.Enabled = false;
		NextFireTime = null;
	}

	public RedboxTimer(string name, ElapsedEventHandler handler)
	{
		Name = (string.IsNullOrEmpty(name) ? "Not Set" : name);
		Timer = new Timer();
		Timer.Elapsed += handler;
	}

	private void ScheduleAtNextInner(TimeSpan span)
	{
		DateTime now = DateTime.Now;
		if (now.TimeOfDay < span)
		{
			double totalMilliseconds = span.Subtract(now.TimeOfDay).TotalMilliseconds;
			NextFireTime = new DateTime(now.Year, now.Month, now.Day, span.Hours, span.Minutes, 0);
			LogHelper.Instance.Log("Timer {0} should fire {1} {2} ( {3}ms from now ).", Name, NextFireTime.Value.ToShortDateString(), NextFireTime.Value.ToShortTimeString(), totalMilliseconds);
			StartTimer(totalMilliseconds);
		}
		else
		{
			DateTime value = new DateTime(now.Year, now.Month, now.Day, span.Hours, span.Minutes, 0).AddDays(1.0);
			NextFireTime = value;
			LogHelper.Instance.Log("Timer {0} should fire on {1} {2}", Name, value.ToShortDateString(), value.ToShortTimeString());
			StartTimer(value.Subtract(now).TotalMilliseconds);
		}
	}

	private void StartTimer(double delta)
	{
		if (!Timer.Enabled)
		{
			Timer.Interval = delta;
			Timer.Start();
		}
	}
}
