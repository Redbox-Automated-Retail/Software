using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.ProductLookupCatalog;

namespace Redbox.KioskEngine.Environment;

internal class DataStoreProvider : IDataStoreService
{
	public static DataStoreProvider GetProvider(string dataStore)
	{
		string path = GetPath(dataStore);
		if (File.Exists(path) && Archive.IsValid(path))
		{
			return new ProductLookupDataStoreProvider();
		}
		return new VistaDbDataStoreProvider();
	}

	public void RegisterDataStoreProvider(string dataStore, string provider)
	{
	}

	public void Set(string dataStore, string tableName, string keyName, object data)
	{
		OnSet(dataStore, tableName, keyName, data);
	}

	public void BulkSet(string dataStore, string tableName, IDictionary<string, object> values)
	{
		OnBulkSet(dataStore, tableName, values);
	}

	public void Clear(string dataStore, string tableName)
	{
		OnClear(dataStore, tableName);
	}

	public object Get(string dataStore, string tableName, string keyName)
	{
		return OnGet(dataStore, tableName, keyName);
	}

	public Dictionary<string, object> GetAll(string dataStore, string tableName)
	{
		return OnGetAll(dataStore, tableName);
	}

	public void Drop(string dataStore, string tableName)
	{
		OnDrop(dataStore, tableName);
	}

	public long Count(string dataStore, string tableName)
	{
		return OnCount(dataStore, tableName);
	}

	public void Remove(string dataStore, string tableName, string keyName)
	{
		OnRemove(dataStore, tableName, keyName);
	}

	public void CloseAllStores()
	{
		OnCloseAllStores();
	}

	public void CloseStore(string dataStore)
	{
		OnCloseStore(dataStore);
	}

	public ReadOnlyCollection<string> GetKeys(string dataStore, string tableName)
	{
		return OnGetKeys(dataStore, tableName);
	}

	public ReadOnlyCollection<string> GetTables(string dataStore)
	{
		return OnGetTables(dataStore);
	}

	protected DataStoreProvider()
	{
	}

	protected virtual void OnSet(string dataStore, string tableName, string keyName, object data)
	{
	}

	protected virtual void OnBulkSet(string dataStore, string tableName, IDictionary<string, object> values)
	{
	}

	protected virtual void OnClear(string dataStore, string tableName)
	{
	}

	protected virtual object OnGet(string dataStore, string tableName, string keyName)
	{
		return null;
	}

	protected virtual Dictionary<string, object> OnGetAll(string dataStore, string tableName)
	{
		return null;
	}

	protected virtual void OnDrop(string dataStore, string tableName)
	{
	}

	protected virtual long OnCount(string dataStore, string tableName)
	{
		return 0L;
	}

	protected virtual void OnRemove(string dataStore, string tableName, string keyName)
	{
	}

	protected virtual void OnCloseAllStores()
	{
	}

	protected virtual void OnCloseStore(string dataStore)
	{
	}

	protected virtual ReadOnlyCollection<string> OnGetKeys(string dataStore, string tableName)
	{
		return new List<string>().AsReadOnly();
	}

	protected virtual ReadOnlyCollection<string> OnGetTables(string dataStore)
	{
		return new List<string>().AsReadOnly();
	}

	internal static string GetPath(string dataStore)
	{
		return Path.Combine(ServiceLocator.Instance.GetService<IEngineApplication>().DataPath, $"{dataStore}.data");
	}
}
