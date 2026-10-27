using System;
using System.Collections.ObjectModel;

namespace Redbox.UpdateManager.ComponentModel;

public interface IWindowsTaskScheduler
{
	bool RemoveScheduledTask(string name);

	void CreateScheduledTask(string name, string applicationName, string arguments, string workingDirectory, string comment, string creatorName, string accountName, string password);

	void AddOnIdleTrigger(string name, DateTime beginDate, DateTime? endDate, short? idleWaitMinutes);

	void AddDailyTrigger(string name, DateTime beginDate, DateTime? endDate, short hours, short minutes, short? daysInterval, int? minutesInterval);

	void AddSystemStartupTrigger(string name, DateTime beginDate, DateTime? endDate);

	ReadOnlyCollection<string> GetScheduledTasks();
}
