using TaskSchedulerInterop;

namespace TaskScheduler;

public class WeeklyTrigger : StartableTrigger
{
	public short WeeksInterval
	{
		get
		{
			return (short)taskTrigger.Data.weekly.WeeksInterval;
		}
		set
		{
			taskTrigger.Data.weekly.WeeksInterval = (ushort)value;
			SyncTrigger();
		}
	}

	public DaysOfTheWeek WeekDays
	{
		get
		{
			return (DaysOfTheWeek)taskTrigger.Data.weekly.DaysOfTheWeek;
		}
		set
		{
			taskTrigger.Data.weekly.DaysOfTheWeek = (ushort)value;
			SyncTrigger();
		}
	}

	public WeeklyTrigger(short hour, short minutes, DaysOfTheWeek daysOfTheWeek, short weeksInterval)
	{
		SetStartTime((ushort)hour, (ushort)minutes);
		taskTrigger.Type = TaskTriggerType.TIME_TRIGGER_WEEKLY;
		taskTrigger.Data.weekly.WeeksInterval = (ushort)weeksInterval;
		taskTrigger.Data.weekly.DaysOfTheWeek = (ushort)daysOfTheWeek;
	}

	public WeeklyTrigger(short hour, short minutes, DaysOfTheWeek daysOfTheWeek)
		: this(hour, minutes, daysOfTheWeek, 1)
	{
	}

	internal WeeklyTrigger(ITaskTrigger iTrigger)
		: base(iTrigger)
	{
	}
}
