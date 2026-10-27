using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Redbox.Core;
using Redbox.IPC.Framework;

namespace Redbox.UpdateManager.Client;

public class ConfigurationClient
{
	internal class InternalClientConfigFile
	{
		internal class TypeMap
		{
			public string TypeName { get; set; }

			public Type Type { get; set; }

			public static List<TypeMap> GetTypeMapList()
			{
				return new List<TypeMap>
				{
					new TypeMap
					{
						TypeName = "bool",
						Type = typeof(bool)
					},
					new TypeMap
					{
						TypeName = "boolean",
						Type = typeof(bool)
					},
					new TypeMap
					{
						TypeName = "string",
						Type = typeof(string)
					},
					new TypeMap
					{
						TypeName = "integer",
						Type = typeof(int)
					},
					new TypeMap
					{
						TypeName = "int",
						Type = typeof(int)
					},
					new TypeMap
					{
						TypeName = "long",
						Type = typeof(long)
					},
					new TypeMap
					{
						TypeName = "float",
						Type = typeof(float)
					},
					new TypeMap
					{
						TypeName = "double",
						Type = typeof(double)
					},
					new TypeMap
					{
						TypeName = "date",
						Type = typeof(DateTime)
					},
					new TypeMap
					{
						TypeName = "datetime",
						Type = typeof(DateTime)
					},
					new TypeMap
					{
						TypeName = "timespan",
						Type = typeof(TimeSpan)
					},
					new TypeMap
					{
						TypeName = "blob",
						Type = typeof(byte[])
					}
				};
			}

			public static Type GetType(string typeName)
			{
				TypeMap typeMap = (from tm in GetTypeMapList()
					where tm.TypeName.Equals(typeName, StringComparison.CurrentCultureIgnoreCase)
					select tm).FirstOrDefault();
				if (typeMap == null)
				{
					return typeof(string);
				}
				return typeMap.Type;
			}
		}

		private string _filePath;

		private DateTime _lastCheckup;

		private TimeSpan _checkTime;

		private Func<bool> _canActivate;

		private Action _activateAction;

		private string _defaultCategory;

		private int _inCheckup;

		private ClientConfigFile _clientConfigFile;

		private const string ConfigFileStaged = ".configstaged";

		private readonly object _lock = new object();

		internal string DefaultCategory => _defaultCategory;

		internal string FilePath => _filePath;

		internal TimeSpan CheckTime => _checkTime;

		internal bool HasCanActivate => _canActivate != null;

		internal bool HasActivateAction => _activateAction != null;

		internal InternalClientConfigFile(string filepath, string defaultCategory, TimeSpan checkTime, Func<bool> canActivate, Action activateAction)
		{
			_lastCheckup = DateTime.Now;
			_checkTime = checkTime;
			if (canActivate == null)
			{
				canActivate = () => true;
			}
			_canActivate = canActivate;
			_activateAction = activateAction;
			_filePath = filepath;
			_defaultCategory = defaultCategory;
			ActivateConfigFile();
		}

		internal bool TryGetValue<T>(string setting, out T value)
		{
			return TryGetValue<T>(_defaultCategory, setting, out value);
		}

		internal bool TryGetValue<T>(string category, string setting, out T value)
		{
			value = default(T);
			lock (_lock)
			{
				try
				{
					if (_clientConfigFile == null)
					{
						return false;
					}
					DateTime date = DateTime.Now;
					ClientConfigurationData clientConfigurationData = (from d in _clientConfigFile.ConfigurationData
						where d.CategoryName.Equals(category, StringComparison.CurrentCultureIgnoreCase) && d.SettingName.Equals(setting, StringComparison.CurrentCultureIgnoreCase) && d.EffectiveTime <= date && (!d.ExpireTime.HasValue || d.ExpireTime > date)
						orderby d.DefaultTypeRank descending, d.EffectiveTime descending
						select d).FirstOrDefault();
					if (clientConfigurationData == null)
					{
						return false;
					}
					if (TypeMap.GetType(clientConfigurationData.SettingDataType) == typeof(byte[]))
					{
						byte[] value2 = new byte[0];
						if (clientConfigurationData.Value.Length > 0)
						{
							value2 = Convert.FromBase64String(clientConfigurationData.Value);
						}
						value = (T)Convert.ChangeType(value2, typeof(T));
						return true;
					}
					value = (T)Convert.ChangeType(clientConfigurationData.Value, typeof(T));
					return true;
				}
				catch (Exception e)
				{
					LogHelper.Instance.Log("Unhandled exception occurred in InternalClientConfigFile.TryGetValue", e);
					return false;
				}
			}
		}

		internal bool TryGetObject(string setting, out object value)
		{
			return TryGetObject(_defaultCategory, setting, out value);
		}

		internal bool TryGetObject(string category, string setting, out object value)
		{
			value = null;
			lock (_lock)
			{
				try
				{
					if (_clientConfigFile == null)
					{
						return false;
					}
					DateTime date = DateTime.Now;
					ClientConfigurationData clientConfigurationData = (from d in _clientConfigFile.ConfigurationData
						where d.CategoryName.Equals(category, StringComparison.CurrentCultureIgnoreCase) && d.SettingName.Equals(setting, StringComparison.CurrentCultureIgnoreCase) && d.EffectiveTime <= date && (!d.ExpireTime.HasValue || d.ExpireTime > date)
						orderby d.DefaultTypeRank descending, d.EffectiveTime descending
						select d).FirstOrDefault();
					if (clientConfigurationData == null)
					{
						return false;
					}
					if (TypeMap.GetType(clientConfigurationData.SettingDataType) == typeof(byte[]))
					{
						value = new byte[0];
						if (clientConfigurationData.Value.Length > 0)
						{
							value = Convert.FromBase64String(clientConfigurationData.Value);
						}
						return true;
					}
					value = Convert.ChangeType(clientConfigurationData.Value, TypeMap.GetType(clientConfigurationData.SettingDataType));
					return true;
				}
				catch (Exception e)
				{
					LogHelper.Instance.Log("Unhandled exception occurred in InternalClientConfigFile.TryGetObject", e);
					return false;
				}
			}
		}

		internal bool TryGetConfigurationData(string setting, out ClientConfigurationData value)
		{
			return TryGetConfigurationData(_defaultCategory, setting, out value);
		}

		internal bool TryGetConfigurationData(string category, string setting, out ClientConfigurationData value)
		{
			value = null;
			lock (_lock)
			{
				try
				{
					if (_clientConfigFile == null)
					{
						return false;
					}
					DateTime date = DateTime.Now;
					ClientConfigurationData clientConfigurationData = (from d in _clientConfigFile.ConfigurationData
						where d.CategoryName.Equals(category, StringComparison.CurrentCultureIgnoreCase) && d.SettingName.Equals(setting, StringComparison.CurrentCultureIgnoreCase) && d.EffectiveTime <= date && (!d.ExpireTime.HasValue || d.ExpireTime > date)
						orderby d.DefaultTypeRank descending, d.EffectiveTime descending
						select d).FirstOrDefault();
					if (clientConfigurationData == null)
					{
						return false;
					}
					value = clientConfigurationData;
					return true;
				}
				catch (Exception e)
				{
					LogHelper.Instance.Log("Unhandled exception occurred in InternalClientConfigFile.TryGetConfigData", e);
					return false;
				}
			}
		}

		internal bool GetAllConfigurationData(out List<ClientConfigurationData> list)
		{
			list = new List<ClientConfigurationData>();
			lock (_lock)
			{
				try
				{
					if (_clientConfigFile == null)
					{
						return false;
					}
					foreach (ClientConfigurationData configurationDatum in _clientConfigFile.ConfigurationData)
					{
						list.Add(configurationDatum);
					}
					return true;
				}
				catch (Exception e)
				{
					LogHelper.Instance.Log("Unhandled exception occurred in InternalClientConfigFile.GetAllConfigurationData", e);
					return false;
				}
			}
		}

		internal bool GetAllConfigurationData(string category, string setting, out List<ClientConfigurationData> list)
		{
			bool allConfigurationData = GetAllConfigurationData(out list);
			if (allConfigurationData)
			{
				list = list.Where((ClientConfigurationData d) => d.CategoryName.Equals(category, StringComparison.CurrentCultureIgnoreCase) && d.SettingName.Equals(setting, StringComparison.CurrentCultureIgnoreCase)).ToList();
			}
			return allConfigurationData;
		}

		internal bool GetAllConfigurationData(string setting, out List<ClientConfigurationData> list)
		{
			return GetAllConfigurationData(_defaultCategory, setting, out list);
		}

		internal bool GetCategoryNames(out List<string> list)
		{
			list = new List<string>();
			lock (_lock)
			{
				try
				{
					if (_clientConfigFile == null)
					{
						return false;
					}
					List<string> list2 = _clientConfigFile.ConfigurationData.Select((ClientConfigurationData cd) => cd.CategoryName).Distinct().ToList();
					list = list2;
					return true;
				}
				catch (Exception e)
				{
					LogHelper.Instance.Log("Unhandled exception occurred in InternalClientConfigFile.GetCategoryNames", e);
					return false;
				}
			}
		}

		internal bool GetSettingNames(out List<string> list)
		{
			return GetSettingNames(_defaultCategory, out list);
		}

		internal bool GetSettingNames(string category, out List<string> list)
		{
			list = new List<string>();
			lock (_lock)
			{
				try
				{
					if (_clientConfigFile == null)
					{
						return false;
					}
					List<string> list2 = (from cd in _clientConfigFile.ConfigurationData
						where cd.CategoryName.Equals(category, StringComparison.CurrentCultureIgnoreCase)
						select cd.SettingName).Distinct().ToList();
					list = list2;
					return true;
				}
				catch (Exception e)
				{
					LogHelper.Instance.Log("Unhandled exception occurred in InternalClientConfigFile.GetSettingNames", e);
					return false;
				}
			}
		}

		internal void CheckForUpdates()
		{
			try
			{
				if (Interlocked.CompareExchange(ref _inCheckup, 1, 0) == 1)
				{
					return;
				}
				try
				{
					if (IsExpired())
					{
						if (NeedsActivation() && _canActivate())
						{
							ActivateConfigFile();
						}
						UpdateLastCheckup();
					}
				}
				finally
				{
					_inCheckup = 0;
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("There was an exception in ConfigurationClient.CheckForUpdates", e);
			}
		}

		internal void UpdateLastCheckup()
		{
			_lastCheckup = DateTime.Now;
		}

		private bool IsExpired()
		{
			return _lastCheckup.Add(_checkTime) < DateTime.Now;
		}

		private void InternalActivateFile()
		{
			if (_clientConfigFile.OID > 0)
			{
				Instance.ActivateConfigFile(_clientConfigFile.OID);
			}
		}

		private bool NeedsActivation()
		{
			return File.Exists(Path.ChangeExtension(_filePath, ".configstaged"));
		}

		private void ActivateConfigFile()
		{
			LoadConfigFile();
			if (NeedsActivation())
			{
				InternalActivateFile();
				if (_activateAction != null)
				{
					_activateAction();
				}
			}
		}

		private void LoadConfigFile()
		{
			lock (_lock)
			{
				try
				{
					_clientConfigFile = null;
					if (!File.Exists(_filePath))
					{
						LogHelper.Instance.Log("Error: Config file {0} does not exist.", _filePath);
						return;
					}
					string text = File.ReadAllText(_filePath);
					if (string.IsNullOrEmpty(text))
					{
						LogHelper.Instance.Log("Error: Config file {0} is empty.", _filePath);
					}
					else
					{
						_clientConfigFile = text.ToObject<ClientConfigFile>();
					}
				}
				catch (Exception e)
				{
					LogHelper.Instance.Log("An unhandled exception occurred in InternalClientConfigFile.LoadConfigFile.", e);
					_clientConfigFile = null;
				}
			}
		}
	}

	private readonly Dictionary<string, InternalClientConfigFile> _clientConfigFileList = new Dictionary<string, InternalClientConfigFile>();

	private readonly object _lock = new object();

	private Timer _changeTimer;

	private bool _isRunning;

	private string _url;

	private int _timeout;

	public static ConfigurationClient Instance => Singleton<ConfigurationClient>.Instance;

	public void Initialize()
	{
		Initialize("rcp://localhost:7004", 60000);
	}

	public void Initialize(string url, int timeout)
	{
		_url = url;
		_timeout = timeout;
		if (_changeTimer == null)
		{
			_changeTimer = new Timer(delegate
			{
				CheckAllForUpdates();
			});
			_isRunning = false;
		}
		else
		{
			Stop();
		}
	}

	public void Start()
	{
		_changeTimer.Change(new TimeSpan(0L), new TimeSpan(0, 0, 120));
		_isRunning = true;
	}

	public void Stop()
	{
		_changeTimer.Change(-1, -1);
		_isRunning = false;
	}

	public void Add(string key, string filepath, string defaultCategory)
	{
		Add(key, filepath, defaultCategory, null, null, null);
	}

	public void Add(string key, string filepath, string defaultCategory, TimeSpan? checkTime, Func<bool> canActivate, Action activateAction)
	{
		lock (_lock)
		{
			if (_clientConfigFileList.ContainsKey(key))
			{
				throw new Exception($"Exception occurred when trying to add a health item with a duplicate key {key}");
			}
			_clientConfigFileList.Add(key, new InternalClientConfigFile(filepath, defaultCategory, checkTime ?? new TimeSpan(0, 0, 120), canActivate, activateAction));
		}
	}

	public void Remove(string key)
	{
		lock (_lock)
		{
			if (_clientConfigFileList.TryGetValue(key, out var _))
			{
				_clientConfigFileList.Remove(key);
				return;
			}
			throw new Exception($"Error trying to remove a configfile item with a non-existent key {key}");
		}
	}

	public List<ConfigurationClientItem> GetItems()
	{
		List<ConfigurationClientItem> list = new List<ConfigurationClientItem>();
		lock (_lock)
		{
			foreach (KeyValuePair<string, InternalClientConfigFile> clientConfigFile in _clientConfigFileList)
			{
				list.Add(new ConfigurationClientItem
				{
					Key = clientConfigFile.Key,
					DefaultCategory = clientConfigFile.Value.DefaultCategory,
					FilePath = clientConfigFile.Value.FilePath,
					CheckTime = clientConfigFile.Value.CheckTime,
					HasCanActivate = clientConfigFile.Value.HasCanActivate,
					HasActivateAction = clientConfigFile.Value.HasActivateAction
				});
			}
			return list;
		}
	}

	public void RemoveAll()
	{
		lock (_lock)
		{
			_clientConfigFileList.Clear();
		}
	}

	public void CheckAllForUpdates()
	{
		if (!_isRunning)
		{
			return;
		}
		lock (_lock)
		{
			foreach (KeyValuePair<string, InternalClientConfigFile> clientConfigFile in _clientConfigFileList)
			{
				clientConfigFile.Value.CheckForUpdates();
			}
		}
	}

	public bool TryGetValue<T>(string key, string setting, out T value)
	{
		value = default(T);
		lock (_lock)
		{
			if (!_clientConfigFileList.TryGetValue(key, out var value2))
			{
				return false;
			}
			return value2.TryGetValue<T>(setting, out value);
		}
	}

	public bool TryGetValue<T>(string key, string category, string setting, out T value)
	{
		value = default(T);
		lock (_lock)
		{
			if (!_clientConfigFileList.TryGetValue(key, out var value2))
			{
				return false;
			}
			return value2.TryGetValue<T>(category, setting, out value);
		}
	}

	public bool TryGetObject(string key, string setting, out object value)
	{
		value = null;
		lock (_lock)
		{
			if (!_clientConfigFileList.TryGetValue(key, out var value2))
			{
				return false;
			}
			return value2.TryGetObject(setting, out value);
		}
	}

	public bool TryGetObject(string key, string category, string setting, out object value)
	{
		value = null;
		lock (_lock)
		{
			if (!_clientConfigFileList.TryGetValue(key, out var value2))
			{
				return false;
			}
			return value2.TryGetObject(category, setting, out value);
		}
	}

	public bool TryGetConfigurationData(string key, string setting, out ClientConfigurationData value)
	{
		value = null;
		lock (_lock)
		{
			if (!_clientConfigFileList.TryGetValue(key, out var value2))
			{
				return false;
			}
			return value2.TryGetConfigurationData(setting, out value);
		}
	}

	public bool TryGetConfigurationData(string key, string category, string setting, out ClientConfigurationData value)
	{
		value = null;
		lock (_lock)
		{
			if (!_clientConfigFileList.TryGetValue(key, out var value2))
			{
				return false;
			}
			return value2.TryGetConfigurationData(category, setting, out value);
		}
	}

	public bool GetAllConfigurationData(string key, out List<ClientConfigurationData> list)
	{
		list = new List<ClientConfigurationData>();
		lock (_lock)
		{
			if (!_clientConfigFileList.TryGetValue(key, out var value))
			{
				return false;
			}
			return value.GetAllConfigurationData(out list);
		}
	}

	public bool GetAllConfigurationData(string key, string category, string setting, out List<ClientConfigurationData> list)
	{
		list = new List<ClientConfigurationData>();
		lock (_lock)
		{
			if (!_clientConfigFileList.TryGetValue(key, out var value))
			{
				return false;
			}
			return value.GetAllConfigurationData(category, setting, out list);
		}
	}

	public bool GetAllConfigurationData(string key, string setting, out List<ClientConfigurationData> list)
	{
		list = new List<ClientConfigurationData>();
		lock (_lock)
		{
			if (!_clientConfigFileList.TryGetValue(key, out var value))
			{
				return false;
			}
			return value.GetAllConfigurationData(setting, out list);
		}
	}

	public bool GetCategoryNames(string key, out List<string> list)
	{
		list = new List<string>();
		lock (_lock)
		{
			if (!_clientConfigFileList.TryGetValue(key, out var value))
			{
				return false;
			}
			return value.GetCategoryNames(out list);
		}
	}

	public bool GetSettingNames(string key, string category, out List<string> list)
	{
		list = new List<string>();
		lock (_lock)
		{
			if (!_clientConfigFileList.TryGetValue(key, out var value))
			{
				return false;
			}
			return value.GetSettingNames(category, out list);
		}
	}

	public bool GetSettingNames(string key, out List<string> list)
	{
		list = new List<string>();
		lock (_lock)
		{
			if (!_clientConfigFileList.TryGetValue(key, out var value))
			{
				return false;
			}
			return value.GetSettingNames(out list);
		}
	}

	internal void ActivateConfigFile(long configFileId)
	{
		using UpdateManagerService updateManagerService = UpdateManagerService.GetService(_url, _timeout);
		ClientCommandResult clientCommandResult = updateManagerService.ActivateConfigFile(configFileId);
		if (!clientCommandResult.Success)
		{
			LogHelper.Instance.Log("Error activating configuration file {0}.", configFileId);
			clientCommandResult.Errors.ForEach(delegate(Redbox.IPC.Framework.Error e)
			{
				LogHelper.Instance.Log("Code {0}, Description {1}, Details {3}", e.Code, e.Description, e.Details);
			});
		}
	}

	private ConfigurationClient()
	{
	}
}
