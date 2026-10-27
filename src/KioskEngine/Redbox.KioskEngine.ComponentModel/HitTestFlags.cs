using System;

namespace Redbox.KioskEngine.ComponentModel;

[Flags]
public enum HitTestFlags
{
	None = 0,
	Enabled = 1,
	Obstruction = 2,
	DrawHotSpot = 4
}
