using System;
using System.Collections.Generic;

namespace Redbox.KioskEngine.ComponentModel;

public interface ISchedulerService
{
	void Start();

	void Clear();

	void Reset();

	void Shutdown();

	ErrorList Initialize();

	void AddScheduleEntry(string jobname, string label, DateTime startTime, DateTime? endTime, string cronExpression, string functionName, string programName, string misfireInstruction, object[] parms);

	bool RemoveScheduleEntry(string name);

	ISchedulerEntry GetScheduleEntry(string jobname);

	List<ISchedulerEntry> GetScheduleEntries();
}
