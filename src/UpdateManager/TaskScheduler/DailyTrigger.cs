using TaskSchedulerInterop;

namespace TaskScheduler;

public class DailyTrigger : StartableTrigger
{
	public short DaysInterval
	{
		get
		{
			return (short)taskTrigger.Data.daily.DaysInterval;
		}
		set
		{
			taskTrigger.Data.daily.DaysInterval = (ushort)value;
			SyncTrigger();
		}
	}

	public DailyTrigger(short hour, short minutes, short daysInterval)
	{
		SetStartTime((ushort)hour, (ushort)minutes);
		taskTrigger.Type = TaskTriggerType.TIME_TRIGGER_DAILY;
		taskTrigger.Data.daily.DaysInterval = (ushort)daysInterval;
	}

	public DailyTrigger(short hour, short minutes)
		: this(hour, minutes, 1)
	{
	}

	internal DailyTrigger(ITaskTrigger iTrigger)
		: base(iTrigger)
	{
	}
}
