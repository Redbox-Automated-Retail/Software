using System;

namespace Redbox.KioskEngine.ComponentModel;

public class QueueServiceConstants
{
	public const string ExceptionErrorCode = "Q999";

	public const string ConnectionErrorCode = "Q998";

	public const string RetryLimitExceeded = "Q997";

	public const string System = "system";

	public const string QueueService = "QueueService";

	public const string OvernightPriorityTimeRange = "OvernightPriorityTimeRange";

	public const string PeakPriorityOnlyTimeRange = "PeakPriorityOnlyTimeRange";

	public static TimeSpan OvernightPriorityStartTimeDefault = TimeSpan.FromHours(1.0);

	public static TimeSpan OvernightPriorityEndTimeDefault = TimeSpan.FromHours(5.0);

	public static TimeSpan PeakPriorityStartTimeDefault = TimeSpan.FromHours(14.0);

	public static TimeSpan PeakPriorityEndTimeDefault = TimeSpan.FromHours(22.0);
}
