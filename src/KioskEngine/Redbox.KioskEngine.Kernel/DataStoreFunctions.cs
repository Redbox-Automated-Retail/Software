using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;

namespace Redbox.KioskEngine.Kernel;

internal static class DataStoreFunctions
{
	[KernelFunction(Name = "kernel.datastoreget")]
	internal static object Get(string dataStore, string tableName, string keyName)
	{
		try
		{
			return ServiceLocator.Instance.GetService<IDataStoreService>()?.Get(dataStore, tableName, keyName);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"An exception was raised in Kernel.DataStoreFunctions.Get({dataStore},{tableName},{keyName}).", e);
			EnvironmentFunctions.HandleDataStoreErrors(dataStore);
			return null;
		}
	}

	[KernelFunction(Name = "kernel.datastoregetall")]
	internal static LuaTable GetAll(string dataStore, string tableName)
	{
		try
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
			ServiceLocator.Instance.GetService<IKernelService>();
			LuaTable keysTable = new LuaTable(KernelService.Instance.LuaRuntime);
			LuaTable cacheTable = new LuaTable(KernelService.Instance.LuaRuntime);
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
		catch (Exception e)
		{
			LogHelper.Instance.Log($"An exception was raised in Kernel.DataStoreFunctions.GetAll({dataStore},{tableName}).", e);
			EnvironmentFunctions.HandleDataStoreErrors(dataStore);
			return null;
		}
	}

	[KernelFunction(Name = "kernel.datastoreclear")]
	internal static void Clear(string dataStore, string tableName)
	{
		IDataStoreService service = ServiceLocator.Instance.GetService<IDataStoreService>();
		if (service == null)
		{
			return;
		}
		try
		{
			service.Clear(dataStore, tableName);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"An exception was raised in Kernel.DataStoreFunctions.Clear({dataStore},{tableName}).", e);
			EnvironmentFunctions.HandleDataStoreErrors(dataStore);
		}
	}

	[KernelFunction(Name = "kernel.datastoreset")]
	internal static void Set(string dataStore, string tableName, string keyName, object value)
	{
		try
		{
			ServiceLocator.Instance.GetService<IDataStoreService>()?.Set(dataStore, tableName, keyName, value);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"An exception was raised in Kernel.DataStoreFunctions.Set({dataStore},{tableName},{keyName}).", e);
			EnvironmentFunctions.HandleDataStoreErrors(dataStore);
		}
	}

	[KernelFunction(Name = "kernel.datastorecount")]
	internal static long Count(string dataStore, string tableName)
	{
		try
		{
			return ServiceLocator.Instance.GetService<IDataStoreService>()?.Count(dataStore, tableName) ?? 0;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"An exception was raised in Kernel.DataStoreFunctions.Count({dataStore},{tableName}).", e);
			EnvironmentFunctions.HandleDataStoreErrors(dataStore);
			return 0L;
		}
	}

	[KernelFunction(Name = "kernel.datastoreremove")]
	internal static void Remove(string dataStore, string tableName, string keyName)
	{
		try
		{
			ServiceLocator.Instance.GetService<IDataStoreService>()?.Remove(dataStore, tableName, keyName);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"An exception was raised in Kernel.DataStoreFunctions.Remove({dataStore},{tableName},{keyName}).", e);
			EnvironmentFunctions.HandleDataStoreErrors(dataStore);
		}
	}

	[KernelFunction(Name = "kernel.datastoregetkeys")]
	internal static LuaTable GetKeys(string dataStore, string tableName)
	{
		try
		{
			LuaTable luaTable = new LuaTable(KernelService.Instance.LuaRuntime);
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
		catch (Exception e)
		{
			LogHelper.Instance.Log($"An exception was raised in Kernel.DataStoreFunctions.GetKeys({dataStore},{tableName}).", e);
			EnvironmentFunctions.HandleDataStoreErrors(dataStore);
			return null;
		}
	}

	[KernelFunction(Name = "kernel.datastorecloseall")]
	internal static void CloseAllDataStores()
	{
		try
		{
			ServiceLocator.Instance.GetService<IDataStoreService>()?.CloseAllStores();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An exception was raised in Kernel.DataStoreFunctions.CloseAllDataStores()", e);
			EnvironmentFunctions.HandleDataStoreErrors("ALL");
		}
	}

	[KernelFunction(Name = "kernel.datastoreclose")]
	internal static void CloseDataStore(string name)
	{
		try
		{
			ServiceLocator.Instance.GetService<IDataStoreService>()?.CloseStore(name);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"An exception was raised in Kernel.DataStoreFunctions.CloseDataStore({name}).", e);
			EnvironmentFunctions.HandleDataStoreErrors(name);
		}
	}

	[KernelFunction(Name = "kernel.datastoredrop")]
	internal static void DropDataStore(string dataStore, string tableName)
	{
		try
		{
			ServiceLocator.Instance.GetService<IDataStoreService>()?.Drop(dataStore, tableName);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"An exception was raised in Kernel.DataStoreFunctions.DropDataStore({dataStore},{tableName}).", e);
			EnvironmentFunctions.HandleDataStoreErrors(dataStore);
		}
	}
}
