using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Redbox.Core;
using Redbox.Rental.Model;

namespace Redbox.Rental.Services.App;

public class PersistentDataCacheService : IPersistentDataCacheService
{
	private readonly LogHelper _logger = LogHelper.Instance;

	private readonly IStoreManager _storeManager = ServiceLocator.Instance.GetService<IStoreManager>();

	private readonly Dictionary<string, object> _cache = new Dictionary<string, object>();

	private string _dataCachePath;

	private string DataCachePath
	{
		get
		{
			if (string.IsNullOrWhiteSpace(_dataCachePath))
			{
				string path = Path.Combine(_storeManager.ApplicationDataPath, "DataCache");
				_dataCachePath = Path.GetFullPath(path);
				Directory.CreateDirectory(_dataCachePath);
			}
			return _dataCachePath;
		}
	}

	public PersistentDataWrapper<T> Read<T>(string fileName, bool useCache = false) where T : IPersistentData
	{
		try
		{
			_logger?.LogInformation($"PersistentDataCacheService.Read({fileName},useCache={useCache})");
			PersistentDataWrapper<T> persistentDataWrapper = null;
			if (useCache && _cache.TryGetValue(fileName, out var value) && value != null)
			{
				_logger?.LogInformation("PersistentDataCacheService.Read - Found cached value");
				return value as PersistentDataWrapper<T>;
			}
			string dataFilePath = GetDataFilePath(fileName);
			if (File.Exists(dataFilePath))
			{
				persistentDataWrapper = JsonConvert.DeserializeObject<PersistentDataWrapper<T>>(File.ReadAllText(dataFilePath));
			}
			if (useCache)
			{
				_cache[fileName] = persistentDataWrapper;
			}
			return persistentDataWrapper;
		}
		catch (Exception e)
		{
			_logger?.LogException("PersistentDataCacheService.Read - unhandled exception occurred", e);
			return null;
		}
	}

	public bool Write<T>(T data, string fileName) where T : IPersistentData
	{
		try
		{
			_logger?.LogInformation("PersistentDataCacheService.Write(" + fileName + ")");
			string contents = JsonConvert.SerializeObject(new PersistentDataWrapper<T>
			{
				Data = data,
				Modified = DateTime.Now,
				DataType = typeof(T).FullName
			}, Formatting.Indented, new JsonSerializerSettings
			{
				NullValueHandling = NullValueHandling.Ignore
			});
			File.WriteAllText(GetDataFilePath(fileName), contents);
			if (_cache.ContainsKey(fileName))
			{
				_logger?.LogInformation("PersistentDataCacheService.Write - clearing cached value");
				_cache.Remove(fileName);
			}
			return true;
		}
		catch (Exception e)
		{
			_logger?.LogException("PersistentDataCacheService.Write - unhandled exception occurred", e);
			return false;
		}
	}

	public bool Delete(string fileName)
	{
		try
		{
			_logger?.LogInformation("PersistentDataCacheService.Delete(" + fileName);
			string dataFilePath = GetDataFilePath(fileName);
			if (File.Exists(dataFilePath))
			{
				File.Delete(dataFilePath);
			}
			return true;
		}
		catch (Exception e)
		{
			_logger?.LogException("PersistentDataCacheService.Delete - unhandled exception occurred", e);
			return false;
		}
	}

	public bool Exists(string fileName)
	{
		try
		{
			_logger?.LogInformation("PersistentDataCacheService.Exists(" + fileName + ")");
			return File.Exists(GetDataFilePath(fileName));
		}
		catch (Exception e)
		{
			_logger?.LogException("PersistentDataCacheService.Read - unhandled exception occurred", e);
			return false;
		}
	}

	private string GetDataFilePath(string fileName)
	{
		return Path.Combine(DataCachePath, Path.ChangeExtension(fileName, "json"));
	}
}
