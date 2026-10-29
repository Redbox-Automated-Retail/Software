namespace TaskScheduler;

internal enum TriggerType
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
