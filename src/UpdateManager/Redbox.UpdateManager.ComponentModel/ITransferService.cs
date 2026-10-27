using System;
using System.Collections.Generic;

namespace Redbox.UpdateManager.ComponentModel;

public interface ITransferService
{
	int MaxBandwidthWhileWithInSchedule { get; }

	int MaxBandwidthWhileOutsideOfSchedule { get; }

	byte StartOfScheduleInHoursFromMidnight { get; }

	byte EndOfScheduleInHoursFromMidnight { get; }

	bool UseSystemMaxOutsideOfSchedule { get; }

	bool EnableMaximumBandwitdthThrottle { get; }

	ErrorList CancelAll();

	ErrorList GetRepositoriesInTransit(out HashSet<string> inTransit);

	ErrorList GetJobs(out List<ITransferJob> jobs, bool allUsers);

	ErrorList CreateDownloadJob(string name, out ITransferJob job);

	ErrorList CreateUploadJob(string name, out ITransferJob job);

	ErrorList GetJob(Guid id, out ITransferJob job);

	ErrorList AreJobsRunning(out bool isRunning);

	bool SetMaxBandwidthWhileWithInSchedule(int i);

	bool SetMaxBandwidthWhileOutsideOfSchedule(int max);

	bool SetStartOfScheduleInHoursFromMidnight(byte b);

	bool SetEndOfScheduleInHoursFromMidnight(byte b);

	bool SetUseSystemMaxOutsideOfSchedule(bool flag);

	bool SetEnableMaximumBandwitdthThrottle(bool flag);
}
