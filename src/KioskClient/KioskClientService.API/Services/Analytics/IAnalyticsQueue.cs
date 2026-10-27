using System;
using System.Threading.Tasks;

namespace KioskClientService.API.Services.Analytics;

public interface IAnalyticsQueue
{
	Task<T> Enqueue<T>(AnalyticsType type, Func<Task<T>> taskGenerator);

	Task Enqueue(AnalyticsType type, Func<Task> taskGenerator);
}
