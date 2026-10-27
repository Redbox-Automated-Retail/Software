using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace KioskClientService.API.Services.Analytics;

public class AnalyticsQueue : IAnalyticsQueue
{
	private readonly ConcurrentDictionary<AnalyticsType, SemaphoreSlim> _semaphores = new ConcurrentDictionary<AnalyticsType, SemaphoreSlim>();

	public async Task<T> Enqueue<T>(AnalyticsType type, Func<Task<T>> taskGenerator)
	{
		SemaphoreSlim semaphore = _semaphores.GetOrAdd(type, (AnalyticsType _) => new SemaphoreSlim(1, 1));
		await semaphore.WaitAsync();
		try
		{
			return await taskGenerator();
		}
		finally
		{
			semaphore.Release();
		}
	}

	public async Task Enqueue(AnalyticsType type, Func<Task> taskGenerator)
	{
		SemaphoreSlim semaphore = _semaphores.GetOrAdd(type, (AnalyticsType _) => new SemaphoreSlim(1, 1));
		await semaphore.WaitAsync(20000);
		try
		{
			await taskGenerator();
		}
		finally
		{
			semaphore.Release();
		}
	}
}
