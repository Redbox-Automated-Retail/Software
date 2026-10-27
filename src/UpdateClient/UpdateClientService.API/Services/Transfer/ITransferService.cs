using System;
using System.Collections.Generic;
using Redbox.NetCore.Middleware.Http;

namespace UpdateClientService.API.Services.Transfer;

public interface ITransferService
{
	int MaxBandwidthWhileWithInSchedule { get; }

	int MaxBandwidthWhileOutsideOfSchedule { get; }

	byte StartOfScheduleInHoursFromMidnight { get; }

	byte EndOfScheduleInHoursFromMidnight { get; }

	bool UseSystemMaxOutsideOfSchedule { get; }

	bool EnableMaximumBandwitdthThrottle { get; }

	List<Error> CancelAll();

	List<Error> GetRepositoriesInTransit(out HashSet<long> inTransit);

	List<Error> GetJobs(out List<ITransferJob> jobs, bool allUsers);

	List<Error> CreateDownloadJob(string name, out ITransferJob job);

	List<Error> CreateUploadJob(string name, out ITransferJob job);

	List<Error> GetJob(Guid id, out ITransferJob job);

	List<Error> AreJobsRunning(out bool isRunning);

	bool SetMaxBandwidthWhileWithInSchedule(int i);

	bool SetMaxBandwidthWhileOutsideOfSchedule(int max);

	bool SetStartOfScheduleInHoursFromMidnight(byte b);

	bool SetEndOfScheduleInHoursFromMidnight(byte b);

	bool SetUseSystemMaxOutsideOfSchedule(bool flag);

	bool SetEnableMaximumBandwitdthThrottle(bool flag);
}
