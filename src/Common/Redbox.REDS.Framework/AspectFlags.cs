using System;

namespace Redbox.REDS.Framework;

[Flags]
public enum AspectFlags : byte
{
	None = 0,
	DeferredLoad = 1
}
