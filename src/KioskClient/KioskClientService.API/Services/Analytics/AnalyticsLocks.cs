using System.Threading;

namespace KioskClientService.API.Services.Analytics;

public static class AnalyticsLocks
{
	public static SemaphoreSlim DownloadAsset = new SemaphoreSlim(1, 1);
}
