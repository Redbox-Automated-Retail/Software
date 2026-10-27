using System;

namespace TaskScheduler;

[Flags]
public enum DaysOfTheWeek : short
{
	Sunday = 1,
	Monday = 2,
	Tuesday = 4,
	Wednesday = 8,
	Thursday = 0x10,
	Friday = 0x20,
	Saturday = 0x40
}
