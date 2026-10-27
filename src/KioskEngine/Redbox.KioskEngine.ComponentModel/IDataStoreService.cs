using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Redbox.KioskEngine.ComponentModel;

public interface IDataStoreService
{
	void Set(string dataStore, string tableName, string keyName, object data);

	void BulkSet(string dataStore, string tableName, IDictionary<string, object> values);

	void Clear(string dataStore, string tableName);

	object Get(string dataStore, string tableName, string keyName);

	Dictionary<string, object> GetAll(string dataStore, string tableName);

	void Drop(string dataStore, string tableName);

	long Count(string dataStore, string tableName);

	void Remove(string dataStore, string tableName, string keyName);

	void CloseStore(string dataStore);

	void CloseAllStores();

	ReadOnlyCollection<string> GetKeys(string dataStore, string tableName);

	ReadOnlyCollection<string> GetTables(string dataStore);
}
