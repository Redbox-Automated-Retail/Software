using System;
using TaskSchedulerInterop;

namespace TaskScheduler;

public abstract class StartableTrigger : Trigger
{
	public short StartHour
	{
		get
		{
			return (short)taskTrigger.StartHour;
		}
		set
		{
			if (value < 0 || value > 23)
			{
				throw new ArgumentOutOfRangeException("hour", value, "hour must be between 0 and 23");
			}
			taskTrigger.StartHour = (ushort)value;
			SyncTrigger();
		}
	}

	public short StartMinute
	{
		get
		{
			return (short)taskTrigger.StartMinute;
		}
		set
		{
			if (value < 0 || value > 59)
			{
				throw new ArgumentOutOfRangeException("minute", value, "minute must be between 0 and 59");
			}
			taskTrigger.StartMinute = (ushort)value;
			SyncTrigger();
		}
	}

	internal StartableTrigger()
	{
	}

	internal StartableTrigger(ITaskTrigger iTrigger)
		: base(iTrigger)
	{
	}

	protected void SetStartTime(ushort hour, ushort minute)
	{
		StartHour = (short)hour;
		StartMinute = (short)minute;
	}
}
