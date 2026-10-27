using System;

namespace Redbox.UpdateManager.ComponentModel;

public interface ITask
{
	string Name { get; set; }

	string Payload { get; set; }

	PayloadType PayloadType { get; set; }

	ScheduleType ScheduleType { get; set; }

	TimeSpan? StartOffset { get; set; }

	DateTime? StartTime { get; set; }

	DateTime? EndTime { get; set; }

	string CronExpression { get; set; }

	TimeSpan? RepeatInterval { get; set; }
}
