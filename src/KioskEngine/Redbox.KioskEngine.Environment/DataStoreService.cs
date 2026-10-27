using System.Collections.Generic;
using System.Collections.ObjectModel;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.IDE;

namespace Redbox.KioskEngine.Environment;

public class DataStoreService : IDataStoreService
{
	private readonly IDictionary<string, DataStoreProvider> m_providers = new Dictionary<string, DataStoreProvider>();

	public static DataStoreService Instance => Singleton<DataStoreService>.Instance;

	public ErrorList Initialize()
	{
		ErrorList result = new ErrorList();
		ServiceLocator.Instance.AddService(typeof(IDataStoreService), Instance);
		ServiceLocator.Instance.GetService<IPreferenceService>()?.AddPreferencePage("DataStoreService_General", "Core", "Engine Core/Data Stores", PreferencePageTarget.LocalSystem, new DataStorePreferencePage());
		return result;
	}

	public void CloseAllStores()
	{
		DataStoreCache.CloseAll();
	}

	public void CloseStore(string dataStore)
	{
		GetProvider(dataStore).CloseStore(dataStore);
	}

	public void Set(string dataStore, string tableName, string keyName, object data)
	{
		GetProvider(dataStore).Set(dataStore, tableName, keyName, data);
	}

	public void BulkSet(string dataStore, string tableName, IDictionary<string, object> values)
	{
		GetProvider(dataStore).BulkSet(dataStore, tableName, values);
	}

	public void Clear(string dataStore, string tableName)
	{
		GetProvider(dataStore).Clear(dataStore, tableName);
	}

	public object Get(string dataStore, string tableName, string keyName)
	{
		return GetProvider(dataStore).Get(dataStore, tableName, keyName);
	}

	public Dictionary<string, object> GetAll(string dataStore, string tableName)
	{
		return GetProvider(dataStore).GetAll(dataStore, tableName);
	}

	public void Drop(string dataStore, string tableName)
	{
		GetProvider(dataStore).Drop(dataStore, tableName);
	}

	public long Count(string dataStore, string tableName)
	{
		return GetProvider(dataStore).Count(dataStore, tableName);
	}

	public void Remove(string dataStore, string tableName, string keyName)
	{
		GetProvider(dataStore).Remove(dataStore, tableName, keyName);
	}

	public ReadOnlyCollection<string> GetKeys(string dataStore, string tableName)
	{
		return GetProvider(dataStore).GetKeys(dataStore, tableName);
	}

	public ReadOnlyCollection<string> GetTables(string dataStore)
	{
		return GetProvider(dataStore).GetTables(dataStore);
	}

	private DataStoreService()
	{
	}

	private DataStoreProvider GetProvider(string dataStore)
	{
		if (m_providers.ContainsKey(dataStore))
		{
			return m_providers[dataStore];
		}
		DataStoreProvider provider = DataStoreProvider.GetProvider(dataStore);
		m_providers[dataStore] = provider;
		return provider;
	}
}
