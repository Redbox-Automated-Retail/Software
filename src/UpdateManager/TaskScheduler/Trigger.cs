using System;
using System.Runtime.InteropServices;
using TaskSchedulerInterop;

namespace TaskScheduler;

public abstract class Trigger : ICloneable
{
	[Flags]
	private enum TaskTriggerFlags
	{
		HasEndDate = 1,
		KillAtDurationEnd = 2,
		Disabled = 4
	}

	private ITaskTrigger iTaskTrigger;

	internal TaskTrigger taskTrigger;

	internal bool Bound => iTaskTrigger != null;

	public DateTime BeginDate
	{
		get
		{
			return new DateTime(taskTrigger.BeginYear, taskTrigger.BeginMonth, taskTrigger.BeginDay);
		}
		set
		{
			taskTrigger.BeginYear = (ushort)value.Year;
			taskTrigger.BeginMonth = (ushort)value.Month;
			taskTrigger.BeginDay = (ushort)value.Day;
			SyncTrigger();
		}
	}

	public bool HasEndDate
	{
		get
		{
			return (taskTrigger.Flags & 1) == 1;
		}
		set
		{
			if (value)
			{
				throw new ArgumentException("HasEndDate can only be set false");
			}
			taskTrigger.Flags &= 4294967294u;
			SyncTrigger();
		}
	}

	public DateTime EndDate
	{
		get
		{
			if (taskTrigger.EndYear == 0)
			{
				return DateTime.MinValue;
			}
			return new DateTime(taskTrigger.EndYear, taskTrigger.EndMonth, taskTrigger.EndDay);
		}
		set
		{
			taskTrigger.Flags |= 1u;
			taskTrigger.EndYear = (ushort)value.Year;
			taskTrigger.EndMonth = (ushort)value.Month;
			taskTrigger.EndDay = (ushort)value.Day;
			SyncTrigger();
		}
	}

	public int DurationMinutes
	{
		get
		{
			return (int)taskTrigger.MinutesDuration;
		}
		set
		{
			if (value < taskTrigger.MinutesInterval)
			{
				throw new ArgumentOutOfRangeException("DurationMinutes", value, "DurationMinutes must be greater than or equal the IntervalMinutes value");
			}
			taskTrigger.MinutesDuration = (uint)value;
			SyncTrigger();
		}
	}

	public int IntervalMinutes
	{
		get
		{
			return (int)taskTrigger.MinutesInterval;
		}
		set
		{
			if (value > taskTrigger.MinutesDuration)
			{
				throw new ArgumentOutOfRangeException("IntervalMinutes", value, "IntervalMinutes must be less than or equal the DurationMinutes value");
			}
			taskTrigger.MinutesInterval = (uint)value;
			SyncTrigger();
		}
	}

	public bool KillAtDurationEnd
	{
		get
		{
			return (taskTrigger.Flags & 2) == 2;
		}
		set
		{
			if (value)
			{
				taskTrigger.Flags |= 2u;
			}
			else
			{
				taskTrigger.Flags &= 4294967293u;
			}
			SyncTrigger();
		}
	}

	public bool Disabled
	{
		get
		{
			return (taskTrigger.Flags & 4) == 4;
		}
		set
		{
			if (value)
			{
				taskTrigger.Flags |= 4u;
			}
			else
			{
				taskTrigger.Flags &= 4294967291u;
			}
			SyncTrigger();
		}
	}

	internal Trigger()
	{
		iTaskTrigger = null;
		taskTrigger = default(TaskTrigger);
		taskTrigger.TriggerSize = (ushort)Marshal.SizeOf((object)taskTrigger);
		taskTrigger.BeginYear = (ushort)DateTime.Today.Year;
		taskTrigger.BeginMonth = (ushort)DateTime.Today.Month;
		taskTrigger.BeginDay = (ushort)DateTime.Today.Day;
	}

	internal Trigger(ITaskTrigger iTrigger)
	{
		if (iTrigger == null)
		{
			throw new ArgumentNullException("iTrigger", "ITaskTrigger instance cannot be null");
		}
		taskTrigger = default(TaskTrigger);
		taskTrigger.TriggerSize = (ushort)Marshal.SizeOf((object)taskTrigger);
		iTrigger.GetTrigger(ref taskTrigger);
		iTaskTrigger = iTrigger;
	}

	public object Clone()
	{
		Trigger trigger = (Trigger)MemberwiseClone();
		trigger.iTaskTrigger = null;
		return trigger;
	}

	internal static Trigger CreateTrigger(ITaskTrigger iTaskTrigger)
	{
		if (iTaskTrigger == null)
		{
			throw new ArgumentNullException("iTaskTrigger", "Instance of ITaskTrigger cannot be null");
		}
		TaskTrigger Trigger = default(TaskTrigger);
		Trigger.TriggerSize = (ushort)Marshal.SizeOf((object)Trigger);
		iTaskTrigger.GetTrigger(ref Trigger);
		return (TriggerType)Trigger.Type switch
		{
			TriggerType.RunOnce => new RunOnceTrigger(iTaskTrigger), 
			TriggerType.RunDaily => new DailyTrigger(iTaskTrigger), 
			TriggerType.RunWeekly => new WeeklyTrigger(iTaskTrigger), 
			TriggerType.RunMonthlyDOW => new MonthlyDOWTrigger(iTaskTrigger), 
			TriggerType.RunMonthly => new MonthlyTrigger(iTaskTrigger), 
			TriggerType.OnIdle => new OnIdleTrigger(iTaskTrigger), 
			TriggerType.OnSystemStart => new OnSystemStartTrigger(iTaskTrigger), 
			TriggerType.OnLogon => new OnLogonTrigger(iTaskTrigger), 
			_ => throw new ArgumentException("Unable to recognize type of trigger referenced in iTaskTrigger", "iTaskTrigger"), 
		};
	}

	protected void SyncTrigger()
	{
		if (iTaskTrigger != null)
		{
			iTaskTrigger.SetTrigger(ref taskTrigger);
		}
	}

	internal void Bind(ITaskTrigger iTaskTrigger)
	{
		if (this.iTaskTrigger != null)
		{
			throw new ArgumentException("Attempt to bind an already bound trigger");
		}
		this.iTaskTrigger = iTaskTrigger;
		iTaskTrigger.SetTrigger(ref taskTrigger);
	}

	internal void Bind(Trigger trigger)
	{
		Bind(trigger.iTaskTrigger);
	}

	internal void Unbind()
	{
		if (iTaskTrigger != null)
		{
			Marshal.ReleaseComObject(iTaskTrigger);
			iTaskTrigger = null;
		}
	}

	public override string ToString()
	{
		if (iTaskTrigger != null)
		{
			iTaskTrigger.GetTriggerString(out var TriggerString);
			return CoTaskMem.LPWStrToString(TriggerString);
		}
		return "Unbound " + GetType().ToString();
	}

	public override bool Equals(object obj)
	{
		return taskTrigger.Equals(((Trigger)obj).taskTrigger);
	}

	public override int GetHashCode()
	{
		return taskTrigger.GetHashCode();
	}
}
