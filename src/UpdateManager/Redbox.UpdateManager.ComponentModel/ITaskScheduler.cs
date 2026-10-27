using System;
using System.Collections.Generic;

namespace Redbox.UpdateManager.ComponentModel;

public interface ITaskScheduler
{
	bool IsRunning { get; }

	ErrorList Stop();

	ErrorList Start();

	ErrorList Restart();

	ErrorList Delete(string name);

	ErrorList Clear();

	ErrorList List(out List<ITask> tasks);

	ErrorList ForceTask(string name, out bool success);

	ErrorList ScheduleCronJob(string name, string payload, string payloadTypeString, TimeSpan? startOffset, DateTime? startTime, DateTime? endTime, string cronExpression);

	ErrorList ScheduleCronJob(string name, string payload, PayloadType payloadType, TimeSpan? startOffset, DateTime? startTime, DateTime? endTime, string cronExpression);

	ErrorList ScheduleSimpleJob(string name, string payload, string payloadTypeString, TimeSpan? startOffset, DateTime? startTime, DateTime? endTime, TimeSpan repeatInterval);

	ErrorList ScheduleSimpleJob(string name, string payload, PayloadType payloadType, TimeSpan? startOffset, DateTime? startTime, DateTime? endTime, TimeSpan repeatInterval);
}
