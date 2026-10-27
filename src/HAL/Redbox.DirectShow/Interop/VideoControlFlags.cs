using System;
using System.Runtime.InteropServices;

namespace Redbox.DirectShow.Interop;

[Flags]
[ComVisible(false)]
public enum VideoControlFlags
{
	FlipHorizontal = 1,
	FlipVertical = 2,
	ExternalTriggerEnable = 4,
	Trigger = 8
}
