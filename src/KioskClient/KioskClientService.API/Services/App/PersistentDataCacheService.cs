using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace KioskClientService.API.Services.App;

public class PersistentDataCacheService : IPersistentDataCacheService
{
	private ILogger<PersistentDataCacheService> _logger;

	private IStoreService _storeService;

	private ConcurrentDictionary<string, object> _cache = new ConcurrentDictionary<string, object>();

	private string _dataCachePath;

	private string DataCachePath
	{
		get
		{
			if (string.IsNullOrWhiteSpace(_dataCachePath))
			{
				string path = Path.Combine(_storeService.DataPath, "DataCache");
				_dataCachePath = Path.GetFullPath(path);
				Directory.CreateDirectory(_dataCachePath);
			}
			return _dataCachePath;
		}
	}

	public PersistentDataCacheService(ILogger<PersistentDataCacheService> logger, IStoreService storeService)
	{
		_logger = logger;
		_storeService = storeService;
	}

	public async Task<PersistentDataWrapper<T>> Read<T>(string fileName, bool useCache = false, bool log = true) where T : IPersistentData
	{
		try
		{
			if (log)
			{
				_logger?.LogDebugWithSource($"Read({fileName},useCache={useCache})", "Read");
			}
			PersistentDataWrapper<T> persistentDataWrapper = null;
			if (useCache && _cache.TryGetValue(fileName, out var value) && value != null)
			{
				if (log)
				{
					_logger?.LogDebugWithSource("Found cached value for file: " + fileName, "Read");
				}
				return value as PersistentDataWrapper<T>;
			}
			string dataFilePath = GetDataFilePath(fileName);
			if (File.Exists(dataFilePath))
			{
				PersistentDataWrapper<T> persistentDataWrapper2 = JsonConvert.DeserializeObject<PersistentDataWrapper<T>>(await File.ReadAllTextAsync(dataFilePath));
				persistentDataWrapper = persistentDataWrapper2;
			}
			if (useCache)
			{
				_cache[fileName] = persistentDataWrapper;
			}
			return persistentDataWrapper;
		}
		catch (Exception ex)
		{
			_logger?.LogErrorWithSource(ex, "unhandled exception occurred", "Read");
			return null;
		}
	}

	public async Task<bool> Write<T>(T data, string fileName) where T : IPersistentData
	{
		try
		{
			_logger?.LogDebugWithSource("Write(" + fileName + ")", "Write");
			PersistentDataWrapper<T> persistentDataWrapper = new PersistentDataWrapper<T>();
			persistentDataWrapper.Data = data;
			persistentDataWrapper.Modified = DateTime.Now;
			persistentDataWrapper.DataType = typeof(T).FullName;
			PersistentDataWrapper<T> value = persistentDataWrapper;
			string contents = JsonConvert.SerializeObject(value, Formatting.Indented, new JsonSerializerSettings
			{
				NullValueHandling = NullValueHandling.Ignore,
				Converters = new List<JsonConverter>
				{
					new StringEnumConverter()
				}
			});
			await File.WriteAllTextAsync(GetDataFilePath(fileName), contents);
			ClearCache(fileName);
			return true;
		}
		catch (Exception ex)
		{
			_logger?.LogErrorWithSource(ex, "Exception while writing to " + fileName, "Write");
			return false;
		}
	}

	public bool Delete(string fileName)
	{
		try
		{
			_logger?.LogDebugWithSource("Delete(" + fileName + ")", "Delete");
			string dataFilePath = GetDataFilePath(fileName);
			if (File.Exists(dataFilePath))
			{
				File.Delete(dataFilePath);
			}
			ClearCache(fileName);
			return true;
		}
		catch (Exception ex)
		{
			_logger?.LogErrorWithSource(ex, "unhandled exception occurred", "Delete");
			return false;
		}
	}

	private void ClearCache(string fileName)
	{
		if (_cache.ContainsKey(fileName))
		{
			_logger?.LogDebugWithSource("Clearing cached value " + fileName, "ClearCache");
			_cache.TryRemove(fileName, out var _);
		}
	}

	private string GetDataFilePath(string fileName)
	{
		return Path.Combine(DataCachePath, Path.ChangeExtension(fileName, "json"));
	}
}
