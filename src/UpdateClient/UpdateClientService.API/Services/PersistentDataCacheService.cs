using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Redbox.NetCore.Logging.Extensions;

namespace UpdateClientService.API.Services;

public class PersistentDataCacheService : IPersistentDataCacheService
{
	private readonly ILogger<PersistentDataCacheService> _logger;

	private readonly IStoreService _storeService;

	private static SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

	private const int _lockWait = 2000;

	private readonly ConcurrentDictionary<string, object> _cache = new ConcurrentDictionary<string, object>();

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

	public async Task<PersistentDataWrapper<T>> Read<T>(string fileName, bool useCache = false, string baseDirectory = null, bool log = true) where T : IPersistentData
	{
		PersistentDataWrapper<T> result = new PersistentDataWrapper<T>();
		try
		{
			if (await _lock.WaitAsync(2000))
			{
				try
				{
					result = await InnerRead<T>(fileName, useCache, baseDirectory, log);
				}
				finally
				{
					_lock.Release();
				}
			}
			else
			{
				_logger.LogErrorWithSource("Unable to get lock for reading filename " + fileName + " with baseDirectory " + baseDirectory + ".", "Read", "/sln/src/UpdateClientService.API/Services/PersistentDataCacheService.cs");
			}
			return result;
		}
		catch (Exception ex)
		{
			_logger?.LogErrorWithSource(ex, "Exception while reading filename: " + fileName + " with baseDirectory: " + baseDirectory, "Read", "/sln/src/UpdateClientService.API/Services/PersistentDataCacheService.cs");
			return result;
		}
	}

	private async Task<PersistentDataWrapper<T>> InnerRead<T>(string fileName, bool useCache = false, string baseDirectory = null, bool log = true) where T : IPersistentData
	{
		PersistentDataWrapper<T> result = new PersistentDataWrapper<T>();
		try
		{
			if (log)
			{
				_logger?.LogInfoWithSource($"Read({fileName},useCache={useCache})", "InnerRead", "/sln/src/UpdateClientService.API/Services/PersistentDataCacheService.cs");
			}
			if (useCache && _cache.TryGetValue(fileName, out var value) && value != null)
			{
				if (log)
				{
					_logger?.LogInformation("Found cached value");
				}
				return value as PersistentDataWrapper<T>;
			}
			string path = ((baseDirectory == null) ? GetDataFilePath(fileName) : Path.Combine(baseDirectory, fileName));
			if (File.Exists(path))
			{
				using FileStream file = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
				using StreamReader reader = new StreamReader(file);
				PersistentDataWrapper<T> persistentDataWrapper = JsonConvert.DeserializeObject<PersistentDataWrapper<T>>(await reader.ReadToEndAsync());
				result = persistentDataWrapper;
			}
			if (useCache)
			{
				_cache[fileName] = result;
			}
			return result;
		}
		catch (Exception ex)
		{
			_logger?.LogErrorWithSource(ex, "Exception while reading filename: " + fileName + " with baseDirectory: " + baseDirectory, "InnerRead", "/sln/src/UpdateClientService.API/Services/PersistentDataCacheService.cs");
			return result;
		}
	}

	public async Task<List<PersistentDataWrapper<T>>> ReadLike<T>(string pattern, string directory = null, bool useCache = true) where T : IPersistentData
	{
		List<PersistentDataWrapper<T>> result = new List<PersistentDataWrapper<T>>();
		try
		{
			directory = directory ?? DataCachePath;
			if (Directory.Exists(directory))
			{
				if (await _lock.WaitAsync(2000))
				{
					try
					{
						string[] files = Directory.GetFiles(directory, "*" + pattern + "*");
						string[] array = files;
						foreach (string path in array)
						{
							List<PersistentDataWrapper<T>> list = result;
							list.Add(await InnerRead<T>(Path.GetFileName(path), useCache, Path.GetDirectoryName(path), log: false));
						}
					}
					finally
					{
						_lock.Release();
					}
				}
				else
				{
					_logger.LogErrorWithSource("Unable to get lock for reading with pattern " + pattern + " in directory " + directory + ".", "ReadLike", "/sln/src/UpdateClientService.API/Services/PersistentDataCacheService.cs");
				}
			}
			return result;
		}
		catch (Exception ex)
		{
			_logger?.LogErrorWithSource(ex, "unhandled exception occurred calling ReadLike(" + pattern + " in directory: " + directory + ")", "ReadLike", "/sln/src/UpdateClientService.API/Services/PersistentDataCacheService.cs");
			return result;
		}
	}

	public async Task<List<PersistentDataWrapper<T>>> ReadLike<T>(Regex pattern, string directory = null, bool useCache = true) where T : IPersistentData
	{
		List<PersistentDataWrapper<T>> result = new List<PersistentDataWrapper<T>>();
		try
		{
			directory = directory ?? DataCachePath;
			if (Directory.Exists(directory))
			{
				if (await _lock.WaitAsync(2000))
				{
					try
					{
						string[] files = Directory.GetFiles(directory, "*.*");
						files = files.Where((string f) => pattern.IsMatch(Path.GetFileName(f))).ToArray();
						string[] array = files;
						foreach (string path in array)
						{
							List<PersistentDataWrapper<T>> list = result;
							list.Add(await InnerRead<T>(Path.GetFileName(path), useCache, Path.GetDirectoryName(path), log: false));
						}
					}
					finally
					{
						_lock.Release();
					}
				}
				else
				{
					_logger.LogErrorWithSource($"Unable to get lock for reading with regex pattern {pattern} in directory {directory}.", "ReadLike", "/sln/src/UpdateClientService.API/Services/PersistentDataCacheService.cs");
				}
			}
			return result;
		}
		catch (Exception ex)
		{
			_logger?.LogErrorWithSource(ex, $"unhandled exception occurred calling ReadLike({pattern} in directory {directory})", "ReadLike", "/sln/src/UpdateClientService.API/Services/PersistentDataCacheService.cs");
			return result;
		}
	}

	public async Task<bool> Write<T>(T data, string fileName, string baseDirectory = null) where T : IPersistentData
	{
		_ = 1;
		try
		{
			_logger?.LogInfoWithSource("Write(" + fileName + ")", "Write", "/sln/src/UpdateClientService.API/Services/PersistentDataCacheService.cs");
			PersistentDataWrapper<T> persistentDataWrapper = new PersistentDataWrapper<T>();
			persistentDataWrapper.Data = data;
			persistentDataWrapper.Modified = DateTime.Now;
			persistentDataWrapper.DataType = typeof(T).FullName;
			PersistentDataWrapper<T> value = persistentDataWrapper;
			string json = JsonConvert.SerializeObject(value, Formatting.Indented, new JsonSerializerSettings
			{
				NullValueHandling = NullValueHandling.Ignore
			});
			string filePath = ((baseDirectory == null) ? GetDataFilePath(fileName) : Path.Combine(baseDirectory, fileName));
			if (baseDirectory != null)
			{
				Directory.CreateDirectory(baseDirectory);
			}
			if (await _lock.WaitAsync(2000))
			{
				try
				{
					await File.WriteAllTextAsync(filePath, json);
					ClearCache(fileName);
				}
				finally
				{
					_lock.Release();
				}
				return true;
			}
			_logger.LogErrorWithSource("Unable to get lock for writing to fileName " + fileName + " with baseDiretory " + baseDirectory + ".", "Write", "/sln/src/UpdateClientService.API/Services/PersistentDataCacheService.cs");
			return false;
		}
		catch (Exception ex)
		{
			_logger?.LogErrorWithSource(ex, "Exception while writing to filename: " + fileName + " with baseDirectory " + baseDirectory, "Write", "/sln/src/UpdateClientService.API/Services/PersistentDataCacheService.cs");
			return false;
		}
	}

	public async Task<bool> Delete(string fileName, string baseDirectory = null)
	{
		try
		{
			_logger?.LogInfoWithSource("Delete(" + fileName + ")", "Delete", "/sln/src/UpdateClientService.API/Services/PersistentDataCacheService.cs");
			string filePath = ((baseDirectory == null) ? GetDataFilePath(fileName) : Path.Combine(baseDirectory, fileName));
			if (await _lock.WaitAsync(2000))
			{
				try
				{
					if (File.Exists(filePath))
					{
						File.Delete(filePath);
					}
					ClearCache(fileName);
					return true;
				}
				finally
				{
					_lock.Release();
				}
			}
			_logger.LogErrorWithSource("Unable to get lock for deleting to fileName " + fileName + " with baseDiretory " + baseDirectory + ".", "Delete", "/sln/src/UpdateClientService.API/Services/PersistentDataCacheService.cs");
			return false;
		}
		catch (Exception ex)
		{
			_logger?.LogErrorWithSource(ex, "Exception while deleting filename " + fileName + " with baseDirectory " + baseDirectory, "Delete", "/sln/src/UpdateClientService.API/Services/PersistentDataCacheService.cs");
			return false;
		}
	}

	public async Task<bool> DeleteLike(string pattern, string directory = null)
	{
		try
		{
			_logger?.LogInfoWithSource("DeleteLike(" + pattern + ")", "DeleteLike", "/sln/src/UpdateClientService.API/Services/PersistentDataCacheService.cs");
			directory = directory ?? DataCachePath;
			if (await _lock.WaitAsync(2000))
			{
				try
				{
					if (Directory.Exists(directory))
					{
						string[] files = Directory.GetFiles(directory, "*" + pattern + "*");
						string[] array = files;
						foreach (string path in array)
						{
							File.Delete(path);
						}
					}
					return true;
				}
				finally
				{
					_lock.Release();
				}
			}
			_logger.LogErrorWithSource("Unable to get lock for deleting files with pattern " + pattern + " in diretory " + directory + ".", "DeleteLike", "/sln/src/UpdateClientService.API/Services/PersistentDataCacheService.cs");
			return false;
		}
		catch (Exception ex)
		{
			_logger?.LogErrorWithSource(ex, "Exception while deleting with pattern: " + pattern + " and directory: " + directory, "DeleteLike", "/sln/src/UpdateClientService.API/Services/PersistentDataCacheService.cs");
			return false;
		}
	}

	private void ClearCache(string fileName)
	{
		if (_cache.ContainsKey(fileName))
		{
			_logger?.LogInfoWithSource("Clearing cached value " + fileName, "ClearCache", "/sln/src/UpdateClientService.API/Services/PersistentDataCacheService.cs");
			_cache.TryRemove(fileName, out var _);
		}
	}

	private string GetDataFilePath(string fileName)
	{
		return Path.Combine(DataCachePath, Path.ChangeExtension(fileName, "json"));
	}
}
