using System;
using System.Collections.Generic;

namespace Redbox.KioskEngine.ComponentModel;

public interface ISchedulerEntry
{
	string JobName { get; set; }

	string Label { get; set; }

	DateTime StartTime { get; set; }

	DateTime? EndTime { get; set; }

	string CronExpression { get; set; }

	string FunctionName { get; set; }

	string Program { get; set; }

	string MisfireInstruction { get; set; }

	List<object> Parameters { get; set; }
}
