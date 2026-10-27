using System;

namespace DeviceService.Domain;

[Flags]
public enum ReadCardContextState
{
	VasContinue = 1,
	PayContinue = 2,
	Continue = VasContinue | PayContinue,
	Stop = 4,
	Timeout = VasContinue | Stop,
	Canceled = PayContinue | Stop,
	Tampered = Continue | Stop,
	Blocked = 8
}
