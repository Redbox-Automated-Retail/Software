using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API.Services.Dual;

internal class DualCacheService : IDualCacheService
{
	private const int _lockWait = 2000;

	private static readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

	private readonly List<long> _keys = new List<long>();

	private readonly IMemoryCache _cache;

	private readonly ILogger<DualCacheService> _logger;

	public DualCacheService(ILogger<DualCacheService> logger, IMemoryCache cache)
	{
		_logger = logger;
		_cache = cache;
	}

	public async Task Refresh(IDictionary<long, int> source)
	{
		try
		{
			if (!source.Any() || !(await _lock.WaitAsync(2000)))
			{
				return;
			}
			try
			{
				List<long> newKeys = source.Keys.ToList();
				List<long> list = _keys.ToList();
				list.RemoveAll((long k) => newKeys.Contains(k));
				foreach (long item in list)
				{
					_keys.Remove(item);
					_cache.Remove(item);
				}
				foreach (long item2 in newKeys)
				{
					_cache.Set(item2, source[item2]);
				}
				_keys.AddRange(newKeys);
				_logger.LogInfoWithSource("dual cache updated", "Refresh");
			}
			finally
			{
				_lock.Release();
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception refreshing dual cache", "Refresh");
		}
	}

	public async Task Update(IEnumerable<long> expected, IDictionary<long, int> actual)
	{
		try
		{
			if (!(await _lock.WaitAsync(2000)))
			{
				return;
			}
			try
			{
				foreach (long item in expected)
				{
					if (actual.ContainsKey(item))
					{
						_cache.Set(item, actual[item]);
						continue;
					}
					_keys.Remove(item);
					_cache.Remove(item);
				}
			}
			finally
			{
				_lock.Release();
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception updating dual cache", "Update");
		}
	}

	public async Task<IEnumerable<long>> Existing(IEnumerable<long> expected)
	{
		List<long> candidates = new List<long>();
		try
		{
			if (await _lock.WaitAsync(2000))
			{
				try
				{
					foreach (long item in expected)
					{
						if (_cache.TryGetValue<int>(item, out var value) && value > 0)
						{
							candidates.Add(item);
						}
					}
				}
				finally
				{
					_lock.Release();
				}
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception checking existing in dual cache", "Existing");
		}
		return candidates;
	}
}
