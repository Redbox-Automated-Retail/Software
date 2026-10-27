using System;
using System.Threading;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Environment;

internal class SoftTimer : ITimer
{
	public object Tag { get; set; }

	public int? Period { get; set; }

	public string Name { get; set; }

	public int? DueTime { get; set; }

	public bool Enabled { get; set; }

	internal DateTime? LastTime { get; set; }

	public event TimerCallback Fire;

	public void Stop()
	{
		Enabled = false;
		LastTime = null;
	}

	public void Start()
	{
		LastTime = null;
		Enabled = true;
	}

	public void ClearFire()
	{
		Fire = null;
	}

	public void RaiseFire()
	{
		if (Fire == null)
		{
			return;
		}
		try
		{
			Fire(Tag);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"An unhandled exception was raised in SoftTimer {Name}", e);
		}
	}

	internal bool UpdateTimer()
	{
		if (!Enabled)
		{
			return false;
		}
		if (!LastTime.HasValue)
		{
			LastTime = DateTime.UtcNow;
			return false;
		}
		DateTime utcNow = DateTime.UtcNow;
		TimeSpan timeSpan = utcNow - LastTime.Value;
		if (DueTime.HasValue)
		{
			if (timeSpan.TotalMilliseconds >= (double?)DueTime)
			{
				DueTime = null;
				LastTime = utcNow;
				RaiseFire();
				return true;
			}
			return false;
		}
		if (!Period.HasValue)
		{
			return false;
		}
		if (timeSpan.TotalMilliseconds >= (double?)Period)
		{
			LastTime = utcNow;
			RaiseFire();
			return true;
		}
		return false;
	}
}
