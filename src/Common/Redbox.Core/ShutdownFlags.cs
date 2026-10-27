using System;

namespace Redbox.Core;

[Flags]
public enum ShutdownFlags : uint
{
	LogOff = 0u,
	ShutDown = 1u,
	Reboot = 2u,
	Force = 4u,
	PowerOff = 8u,
	ForceIfHung = 0x10u
}
