using TaskSchedulerInterop;

namespace TaskScheduler;

public class OnSystemStartTrigger : Trigger
{
	public OnSystemStartTrigger()
	{
		taskTrigger.Type = TaskTriggerType.EVENT_TRIGGER_AT_SYSTEMSTART;
	}

	internal OnSystemStartTrigger(ITaskTrigger iTrigger)
		: base(iTrigger)
	{
	}
}
