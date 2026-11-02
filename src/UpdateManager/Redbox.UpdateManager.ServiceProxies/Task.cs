using System;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.ServiceProxies;

public class Task : ITask
{
	public DateTime? EndTime { get; set; }

	public string? CronExpression { get; set; }

	public TimeSpan? RepeatInterval { get; set; }

	// Name and Payload are set by the JSON reader.
	public string Name { get; set; } = null!;

	public string Payload { get; set; } = null!;

	public PayloadType PayloadType { get; set; }

	public ScheduleType ScheduleType { get; set; }

	public TimeSpan? StartOffset { get; set; }

	public DateTime? StartTime { get; set; }

	public PayloadType Type { get; set; }
}
