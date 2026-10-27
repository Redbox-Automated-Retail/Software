using System;

namespace Redbox.HAL.Component.Model;

[Flags]
public enum DeviceStatus
{
	None = 0,
	Unknown = 1,
	Found = 2,
	Enabled = 4,
	Disabled = 8,
	DriverMatched = 0x10,
	Configured = 0x20
}
