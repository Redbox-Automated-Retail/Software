using System;

namespace Redbox.HAL.Script.Framework;

[Flags]
internal enum ReadDiskOptions
{
	None = 0,
	LeaveCaptureResult = 1,
	CheckForDuplicate = 2,
	LeaveNoReadResult = 4,
	CenterDisk = 8
}
