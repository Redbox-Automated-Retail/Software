using System;
using TaskSchedulerInterop;

namespace TaskScheduler;

public class RunOnceTrigger : StartableTrigger
{
	public RunOnceTrigger(DateTime runDateTime)
	{
		taskTrigger.BeginYear = (ushort)runDateTime.Year;
		taskTrigger.BeginMonth = (ushort)runDateTime.Month;
		taskTrigger.BeginDay = (ushort)runDateTime.Day;
		SetStartTime((ushort)runDateTime.Hour, (ushort)runDateTime.Minute);
		taskTrigger.Type = TaskTriggerType.TIME_TRIGGER_ONCE;
	}

	internal RunOnceTrigger(ITaskTrigger iTrigger)
		: base(iTrigger)
	{
	}
}
