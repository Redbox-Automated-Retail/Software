using System;

namespace Redbox.KioskEngine.ComponentModel;

[Flags]
public enum RenderStateFlags
{
	None = 0,
	Drawn = 1,
	Occluded = 2
}
