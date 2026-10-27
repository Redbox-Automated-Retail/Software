using System;

namespace TaskScheduler;

[Flags]
public enum MonthsOfTheYear : short
{
	January = 1,
	February = 2,
	March = 4,
	April = 8,
	May = 0x10,
	June = 0x20,
	July = 0x40,
	August = 0x80,
	September = 0x100,
	October = 0x200,
	November = 0x400,
	December = 0x800
}
