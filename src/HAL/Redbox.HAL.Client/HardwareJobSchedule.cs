using System;

namespace Redbox.HAL.Client;

public sealed class HardwareJobSchedule
{
	public DateTime? StartTime { get; set; }

	public HardwareJobPriority Priority { get; set; }
}
