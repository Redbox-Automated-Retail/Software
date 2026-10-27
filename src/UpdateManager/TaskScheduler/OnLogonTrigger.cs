using TaskSchedulerInterop;

namespace TaskScheduler;

public class OnLogonTrigger : Trigger
{
	public OnLogonTrigger()
	{
		taskTrigger.Type = TaskTriggerType.EVENT_TRIGGER_AT_LOGON;
	}

	internal OnLogonTrigger(ITaskTrigger iTrigger)
		: base(iTrigger)
	{
	}
}
