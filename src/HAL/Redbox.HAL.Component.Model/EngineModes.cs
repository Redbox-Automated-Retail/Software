using System;

namespace Redbox.HAL.Component.Model;

[Flags]
public enum EngineModes
{
	Normal = 0,
	Maintenance = 1,
	Diagnostic = 2,
	Merchandizing = 4
}
