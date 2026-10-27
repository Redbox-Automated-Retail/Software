using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Timers;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;
using Redbox.Rental.Model;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.Health;

namespace Redbox.Rental.Kernel;

public static class DataServiceFunctions
{
	private static readonly Timer _cacheAlertTimer = new Timer(TimeSpan.FromDays(1.0).TotalMilliseconds)
	{
		AutoReset = true
	};

	private static bool _cacheAlertSent = false;

	public static string CacheTableName = "cache";

	[KernelFunction(Name = "kernel.dataserviceget")]
	internal static object DataServiceGet(string dataStore, string tableName, string keyName)
	{
		try
		{
			if (dataStore.Equals(CacheTableName))
			{
				return ServiceLocator.Instance.GetService<IDataStoreService>()?.Get(dataStore, tableName, keyName);
			}
			return null;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"An unhandled exception occured - DataServiceFunctions.DataServiceGet({dataStore},{tableName},{keyName}).", e);
			return null;
		}
	}

	[KernelFunction(Name = "kernel.dataservicegetall")]
	internal static LuaTable DataServiceGetAll(string dataStore, string tableName)
	{
		try
		{
			if (dataStore.Equals(CacheTableName))
			{
				IDataStoreService service = ServiceLocator.Instance.GetService<IDataStoreService>();
				if (service == null)
				{
					return null;
				}
				Dictionary<string, object> all = service.GetAll(dataStore, tableName);
				if (all == null)
				{
					return null;
				}
				IKernelService service2 = ServiceLocator.Instance.GetService<IKernelService>();
				LuaTable keysTable = (LuaTable)service2.NewTable();
				LuaTable cacheTable = (LuaTable)service2.NewTable();
				int i = 1;
				all.ForEach(delegate(KeyValuePair<string, object> r)
				{
					keysTable[i] = r.Key;
					cacheTable[r.Key] = r.Value;
					i++;
				});
				cacheTable["keys"] = keysTable;
				return cacheTable;
			}
			return null;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"An unhandled exception occured - DataServiceFunctions.DataServiceGetAll({dataStore},{tableName}).", e);
			throw;
		}
	}

	[KernelFunction(Name = "kernel.dataservicegetkeys")]
	internal static LuaTable DataServiceGetKeys(string dataStore, string tableName)
	{
		try
		{
			if (dataStore.Equals(CacheTableName))
			{
				LuaTable luaTable = (LuaTable)ServiceLocator.Instance.GetService<IKernelService>().NewTable();
				IDataStoreService service = ServiceLocator.Instance.GetService<IDataStoreService>();
				if (service == null)
				{
					return luaTable;
				}
				ReadOnlyCollection<string> keys = service.GetKeys(dataStore, tableName);
				int num = 1;
				foreach (string item in keys)
				{
					luaTable[num++] = item;
				}
				return luaTable;
			}
			return null;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"An unhandled exception occured - DataServiceFunctions.DataServiceGetKeys({dataStore},{tableName}).", e);
			ServiceLocator.Instance.GetService<IEnvironmentNotificationService>()?.RaiseCorruptDb();
			return null;
		}
	}

	[KernelFunction(Name = "kernel.dataservicecount")]
	internal static long DataServiceCount(string dataStore, string tableName)
	{
		try
		{
			if (dataStore.Equals(CacheTableName))
			{
				return ServiceLocator.Instance.GetService<IDataStoreService>()?.Count(dataStore, tableName) ?? 0;
			}
			return 0L;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"An unhandled exception occured - DataServiceFunctions.DataServiceCount({dataStore},{tableName}).", e);
			throw;
		}
	}

	[KernelFunction(Name = "kernel.dataserviceclose")]
	internal static void DataServiceClose(string dataStore)
	{
		LogHelper.Instance.Log("DataServiceFunctions.DataServiceClose called");
		if (dataStore.Equals(CacheTableName))
		{
			ServiceLocator.Instance.GetService<IDataStoreService>()?.CloseStore(dataStore);
		}
	}

	[KernelFunction(Name = "kernel.dataserviceremove")]
	internal static void DataServiceRemove(string dataStore, string tableName, string keyName)
	{
		try
		{
			if (dataStore.Equals(CacheTableName))
			{
				IDataStoreService service = ServiceLocator.Instance.GetService<IDataStoreService>();
				if (service != null)
				{
					service.Remove(dataStore, tableName, keyName);
					ServiceLocator.Instance.GetService<IDataService>().CacheTablesRemove(tableName, keyName);
				}
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"An unhandled exception occured - DataServiceFunctions.DataServiceRemove({dataStore},{tableName},{keyName}).", e);
		}
	}

	[KernelFunction(Name = "kernel.dataserviceclear")]
	internal static void DataServiceClear(string dataStore, string tableName)
	{
		try
		{
			if (dataStore.Equals(CacheTableName))
			{
				ServiceLocator.Instance.GetService<IDataStoreService>()?.Clear(dataStore, tableName);
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"An unhandled exception occured - DataServiceFunctions.DataServiceClear({dataStore},{tableName}).", e);
		}
	}

	[KernelFunction(Name = "kernel.dataserviceset")]
	internal static void DataServiceSet(string dataStore, string tableName, string keyName, object data)
	{
		try
		{
			if (dataStore.Equals(CacheTableName))
			{
				IDataStoreService service = ServiceLocator.Instance.GetService<IDataStoreService>();
				if (service != null)
				{
					service.Set(dataStore, tableName, keyName, data);
					ServiceLocator.Instance.GetService<IDataService>().CacheTablesUpdate(tableName, keyName, data);
				}
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"An unhandled exception occured - DataServiceFunctions.DataServiceSet({dataStore},{tableName},{keyName},{Convert.ToString(data)}).", e);
			if (dataStore != null && dataStore.Equals(CacheTableName) && !_cacheAlertSent)
			{
				SendAlert(dataStore, tableName, keyName);
				_cacheAlertSent = true;
				if (!_cacheAlertTimer.Enabled)
				{
					StartCacheAlertTimer();
				}
			}
		}
	}

	private static void SendAlert(string dataStore, string tableName, string keyName)
	{
		ServiceLocator.Instance.GetService<IHealthServices>()?.SendAlert(ServiceLocator.Instance.GetService<IStoreManager>()?.KioskId.ToString(), "ApplicationCrash", "mm:DataStoreError, SOFT-FILE03", "<h2>Kiosk Engine Data File Error</h2><p>Could not find key [ " + keyName + " ] in table [ " + tableName + " ] for data store [ " + dataStore + " ]</p>", DateTime.Now, null);
	}

	private static void StartCacheAlertTimer()
	{
		_cacheAlertTimer.Elapsed += ResetCacheAlertSent;
		_cacheAlertTimer.Start();
	}

	private static void ResetCacheAlertSent(object sender, ElapsedEventArgs e)
	{
		_cacheAlertSent = false;
	}
}
