namespace TaskScheduler;

public enum TriggerType
{
	RunOnce,
	RunDaily,
	RunWeekly,
	RunMonthly,
	RunMonthlyDOW,
	OnIdle,
	OnSystemStart,
	OnLogon
}
