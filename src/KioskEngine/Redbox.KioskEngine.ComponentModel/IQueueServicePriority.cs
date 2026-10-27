using System;
using System.Collections.Generic;

namespace Redbox.KioskEngine.ComponentModel;

public interface IQueueServicePriority
{
	QueueServicePriorityType PriorityType { get; set; }

	int MinimumPriorityValue { get; set; }

	int MaximumPriorityValue { get; set; }

	TimeSpan? StartTime { get; set; }

	TimeSpan? EndTime { get; set; }

	List<IQueueServicePriority> ExcludeTimeRanges { get; }

	string Description { get; set; }

	bool IsInTimeRange(TimeSpan timeSpan);
}
