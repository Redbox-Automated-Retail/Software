using System;
using TaskSchedulerInterop;

namespace TaskScheduler;

public class MonthlyTrigger : StartableTrigger
{
	public MonthsOfTheYear Months
	{
		get
		{
			return (MonthsOfTheYear)taskTrigger.Data.monthlyDate.Months;
		}
		set
		{
			taskTrigger.Data.monthlyDOW.Months = (ushort)value;
			SyncTrigger();
		}
	}

	public int[] Days
	{
		get
		{
			return MaskToIndices((int)taskTrigger.Data.monthlyDate.Days);
		}
		set
		{
			taskTrigger.Data.monthlyDate.Days = (uint)IndicesToMask(value);
			SyncTrigger();
		}
	}

	public MonthlyTrigger(short hour, short minutes, int[] daysOfMonth, MonthsOfTheYear months)
	{
		SetStartTime((ushort)hour, (ushort)minutes);
		taskTrigger.Type = TaskTriggerType.TIME_TRIGGER_MONTHLYDATE;
		taskTrigger.Data.monthlyDate.Months = (ushort)months;
		taskTrigger.Data.monthlyDate.Days = (uint)IndicesToMask(daysOfMonth);
	}

	public MonthlyTrigger(short hour, short minutes, int[] daysOfMonth)
		: this(hour, minutes, daysOfMonth, MonthsOfTheYear.January | MonthsOfTheYear.February | MonthsOfTheYear.March | MonthsOfTheYear.April | MonthsOfTheYear.May | MonthsOfTheYear.June | MonthsOfTheYear.July | MonthsOfTheYear.August | MonthsOfTheYear.September | MonthsOfTheYear.October | MonthsOfTheYear.November | MonthsOfTheYear.December)
	{
	}

	internal MonthlyTrigger(ITaskTrigger iTrigger)
		: base(iTrigger)
	{
	}

	private static int[] MaskToIndices(int mask)
	{
		int num = 0;
		for (int i = 0; mask >> i > 0; i++)
		{
			num += 1 & (mask >> i);
		}
		int[] array = new int[num];
		num = 0;
		for (int j = 0; mask >> j > 0; j++)
		{
			if ((1 & (mask >> j)) == 1)
			{
				array[num++] = j + 1;
			}
		}
		return array;
	}

	private static int IndicesToMask(int[] indices)
	{
		int num = 0;
		foreach (int num2 in indices)
		{
			if (num2 < 1 || num2 > 31)
			{
				throw new ArgumentException("Days must be in the range 1..31");
			}
			num |= 1 << num2 - 1;
		}
		return num;
	}
}
