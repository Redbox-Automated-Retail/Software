using TaskSchedulerInterop;

namespace TaskScheduler;

public class OnIdleTrigger : Trigger
{
	public OnIdleTrigger()
	{
		taskTrigger.Type = TaskTriggerType.EVENT_TRIGGER_ON_IDLE;
	}

	internal OnIdleTrigger(ITaskTrigger iTrigger)
		: base(iTrigger)
	{
	}
}
