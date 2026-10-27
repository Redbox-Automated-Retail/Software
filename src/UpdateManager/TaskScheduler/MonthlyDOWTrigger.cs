using TaskSchedulerInterop;

namespace TaskScheduler;

public class MonthlyDOWTrigger : StartableTrigger
{
	public short WhichWeeks
	{
		get
		{
			return (short)taskTrigger.Data.monthlyDOW.WhichWeek;
		}
		set
		{
			taskTrigger.Data.monthlyDOW.WhichWeek = (ushort)value;
			SyncTrigger();
		}
	}

	public DaysOfTheWeek WeekDays
	{
		get
		{
			return (DaysOfTheWeek)taskTrigger.Data.monthlyDOW.DaysOfTheWeek;
		}
		set
		{
			taskTrigger.Data.monthlyDOW.DaysOfTheWeek = (ushort)value;
			SyncTrigger();
		}
	}

	public MonthsOfTheYear Months
	{
		get
		{
			return (MonthsOfTheYear)taskTrigger.Data.monthlyDOW.Months;
		}
		set
		{
			taskTrigger.Data.monthlyDOW.Months = (ushort)value;
			SyncTrigger();
		}
	}

	public MonthlyDOWTrigger(short hour, short minutes, DaysOfTheWeek daysOfTheWeek, WhichWeek whichWeeks, MonthsOfTheYear months)
	{
		SetStartTime((ushort)hour, (ushort)minutes);
		taskTrigger.Type = TaskTriggerType.TIME_TRIGGER_MONTHLYDOW;
		taskTrigger.Data.monthlyDOW.WhichWeek = (ushort)whichWeeks;
		taskTrigger.Data.monthlyDOW.DaysOfTheWeek = (ushort)daysOfTheWeek;
		taskTrigger.Data.monthlyDOW.Months = (ushort)months;
	}

	public MonthlyDOWTrigger(short hour, short minutes, DaysOfTheWeek daysOfTheWeek, WhichWeek whichWeeks)
		: this(hour, minutes, daysOfTheWeek, whichWeeks, MonthsOfTheYear.January | MonthsOfTheYear.February | MonthsOfTheYear.March | MonthsOfTheYear.April | MonthsOfTheYear.May | MonthsOfTheYear.June | MonthsOfTheYear.July | MonthsOfTheYear.August | MonthsOfTheYear.September | MonthsOfTheYear.October | MonthsOfTheYear.November | MonthsOfTheYear.December)
	{
	}

	internal MonthlyDOWTrigger(ITaskTrigger iTrigger)
		: base(iTrigger)
	{
	}
}
