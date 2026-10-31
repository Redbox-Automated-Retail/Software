using System;

namespace Redbox.Core;

[Flags]
internal enum Redbox_002EMacros1309_002EShutdownFlags : uint
{
	LogOff = 0u,
	ShutDown = 1u,
	Reboot = 2u,
	Force = 4u,
	PowerOff = 8u,
	ForceIfHung = 0x10u
}
