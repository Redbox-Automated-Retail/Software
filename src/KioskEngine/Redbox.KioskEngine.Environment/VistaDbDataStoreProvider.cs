using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using VistaDB;
using VistaDB.DDA;

namespace Redbox.KioskEngine.Environment;

internal class VistaDbDataStoreProvider : DataStoreProvider
{
	protected override void OnClear(string dataStore, string tableName)
	{
		try
		{
			IVistaDBDatabase workStore = DataStoreCache.GetWorkStore(DataStoreProvider.GetPath(dataStore));
			if (workStore == null || !workStore.ContainsTable(tableName))
			{
				return;
			}
			using IVistaDBTable vistaDBTable = workStore.OpenTable(tableName, exclusive: false, readOnly: false);
			vistaDBTable.First();
			while (!vistaDBTable.EndOfTable)
			{
				vistaDBTable.Delete();
				vistaDBTable.Next();
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"Exception in VistaDbDataStoreProvider.OnClear({dataStore},{tableName})", e);
			throw;
		}
	}

	protected override void OnCloseAllStores()
	{
		DataStoreCache.CloseAll();
	}

	protected override void OnCloseStore(string dataStore)
	{
		DataStoreCache.CloseStore(DataStoreProvider.GetPath(dataStore));
	}

	protected override long OnCount(string dataStore, string tableName)
	{
		try
		{
			IVistaDBDatabase workStore = DataStoreCache.GetWorkStore(DataStoreProvider.GetPath(dataStore));
			if (workStore == null)
			{
				return 0L;
			}
			if (!workStore.ContainsTable(tableName))
			{
				return 0L;
			}
			using IVistaDBTable vistaDBTable = workStore.OpenTable(tableName, exclusive: false, readOnly: true);
			return vistaDBTable.RowCount;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"Exception in VistaDbDataStoreProvider.OnCount({dataStore},{tableName})", e);
			throw;
		}
	}

	protected override object OnGet(string dataStore, string tableName, string keyName)
	{
		try
		{
			IVistaDBDatabase workStore = DataStoreCache.GetWorkStore(DataStoreProvider.GetPath(dataStore));
			if (workStore == null)
			{
				return null;
			}
			if (!workStore.ContainsTable(tableName))
			{
				return null;
			}
			using IVistaDBTable table = workStore.OpenTable(tableName, exclusive: false, readOnly: true);
			return InternalGet(table, keyName);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"Exception in VistaDbDataStoreProvider.OnGet({dataStore},{tableName},{keyName})", e);
			throw;
		}
	}

	protected override Dictionary<string, object> OnGetAll(string dataStore, string tableName)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		try
		{
			IVistaDBDatabase workStore = DataStoreCache.GetWorkStore(DataStoreProvider.GetPath(dataStore));
			if (workStore == null)
			{
				return dictionary;
			}
			if (!workStore.ContainsTable(tableName))
			{
				return dictionary;
			}
			using IVistaDBTable vistaDBTable = workStore.OpenTable(tableName, exclusive: false, readOnly: true);
			vistaDBTable.Next();
			while (!vistaDBTable.EndOfTable)
			{
				string text = vistaDBTable.CurrentRow["Key"].Value as string;
				string value = vistaDBTable.CurrentRow["Value"].Value as string;
				if (!string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(value))
				{
					dictionary[text] = value;
					vistaDBTable.Next();
				}
			}
			return dictionary;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"Exception in VistaDbDataStoreProvider.OnGetAll({dataStore},{tableName})", e);
			throw;
		}
	}

	protected override void OnDrop(string dataStore, string tableName)
	{
		try
		{
			IVistaDBDatabase workStore = DataStoreCache.GetWorkStore(DataStoreProvider.GetPath(dataStore));
			if (workStore != null && workStore.ContainsTable(tableName))
			{
				workStore.DropTable(tableName);
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"Exception in VistaDbDataStoreProvider.OnDrop({dataStore},{tableName})", e);
			throw;
		}
	}

	protected override ReadOnlyCollection<string> OnGetKeys(string dataStore, string tableName)
	{
		List<string> list = new List<string>();
		try
		{
			IVistaDBDatabase workStore = DataStoreCache.GetWorkStore(DataStoreProvider.GetPath(dataStore));
			if (workStore == null)
			{
				return list.AsReadOnly();
			}
			if (!workStore.ContainsTable(tableName))
			{
				return list.AsReadOnly();
			}
			using IVistaDBTable vistaDBTable = workStore.OpenTable(tableName, exclusive: false, readOnly: true);
			vistaDBTable.Next();
			while (!vistaDBTable.EndOfTable)
			{
				string text = vistaDBTable.CurrentRow["Key"].Value as string;
				if (!string.IsNullOrEmpty(text))
				{
					list.Add(text);
					vistaDBTable.Next();
				}
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"Exception in VistaDbDataStoreProvider.OnGetKeys({dataStore},{tableName})", e);
			throw;
		}
		return list.AsReadOnly();
	}

	protected override void OnRemove(string dataStore, string tableName, string keyName)
	{
		try
		{
			IVistaDBDatabase workStore = DataStoreCache.GetWorkStore(DataStoreProvider.GetPath(dataStore));
			if (workStore == null || !workStore.ContainsTable(tableName))
			{
				return;
			}
			using IVistaDBTable vistaDBTable = workStore.OpenTable(tableName, exclusive: false, readOnly: false);
			if (vistaDBTable.Find($"Key: '{keyName}'", "IX_Key", partialMatching: false, softPosition: false))
			{
				vistaDBTable.Delete();
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"Exception in VistaDbDataStoreProvider.OnRemove({dataStore},{tableName},{keyName})", e);
			throw;
		}
	}

	protected override void OnSet(string dataStore, string tableName, string keyName, object data)
	{
		IVistaDBDatabase workStore = DataStoreCache.GetWorkStore(DataStoreProvider.GetPath(dataStore));
		if (workStore == null)
		{
			return;
		}
		IVistaDBTable vistaDBTable = null;
		try
		{
			if (!workStore.ContainsTable(tableName))
			{
				IVistaDBTableSchema vistaDBTableSchema = workStore.NewTable(tableName);
				vistaDBTableSchema.AddColumn("Key", VistaDBType.VarChar, 250);
				vistaDBTableSchema.AddColumn("Value", VistaDBType.NText);
				vistaDBTable = workStore.CreateTable(vistaDBTableSchema, exclusive: false, readOnly: false);
				vistaDBTable.CreateIndex("IX_Key", "Key", primary: true, unique: true);
			}
			else
			{
				vistaDBTable = workStore.OpenTable(tableName, exclusive: false, readOnly: false);
			}
			IKernelService service = ServiceLocator.Instance.GetService<IKernelService>();
			if (InternalGet(vistaDBTable, keyName) == null)
			{
				vistaDBTable.Insert();
				vistaDBTable.PutString("Key", keyName);
			}
			vistaDBTable.PutString("Value", service.FormatLuaValue(data));
			vistaDBTable.Post();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"Exception in VistaDbDataStoreProvider.OnSet({dataStore},{tableName},{keyName})", e);
			throw;
		}
		finally
		{
			vistaDBTable?.Dispose();
		}
	}

	protected override void OnBulkSet(string dataStore, string tableName, IDictionary<string, object> values)
	{
		IVistaDBDatabase workStore = DataStoreCache.GetWorkStore(DataStoreProvider.GetPath(dataStore));
		if (workStore == null)
		{
			return;
		}
		IKernelService service = ServiceLocator.Instance.GetService<IKernelService>();
		IVistaDBTable vistaDBTable = null;
		try
		{
			if (!workStore.ContainsTable(tableName))
			{
				IVistaDBTableSchema vistaDBTableSchema = workStore.NewTable(tableName);
				vistaDBTableSchema.AddColumn("Key", VistaDBType.VarChar, 250);
				vistaDBTableSchema.AddColumn("Value", VistaDBType.NText);
				vistaDBTable = workStore.CreateTable(vistaDBTableSchema, exclusive: false, readOnly: false);
				vistaDBTable.CreateIndex("IX_Key", "Key", primary: true, unique: true);
			}
			else
			{
				vistaDBTable = workStore.OpenTable(tableName, exclusive: false, readOnly: false);
			}
			foreach (KeyValuePair<string, object> value in values)
			{
				if (InternalGet(vistaDBTable, value.Key) == null)
				{
					vistaDBTable.Insert();
					vistaDBTable.PutString("Key", value.Key);
				}
				vistaDBTable.PutString("Value", service.FormatLuaValue(value.Value));
				vistaDBTable.Post();
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"Exception in VistaDbDataStoreProvider.OnBulkSet({dataStore},{tableName})", e);
			throw;
		}
		finally
		{
			vistaDBTable?.Dispose();
		}
	}

	protected override ReadOnlyCollection<string> OnGetTables(string dataStore)
	{
		List<string> list = new List<string>();
		IVistaDBDatabase workStore = DataStoreCache.GetWorkStore(DataStoreProvider.GetPath(dataStore));
		if (workStore == null)
		{
			return list.AsReadOnly();
		}
		try
		{
			foreach (string item in workStore.EnumTables())
			{
				list.Add(item);
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"Exception in VistaDbDataStoreProvider.OnGetTables({dataStore})", e);
			throw;
		}
		return list.AsReadOnly();
	}

	private static object InternalGet(IVistaDBTable table, string keyName)
	{
		if (!table.Find($"Key: '{keyName}'", "IX_Key", partialMatching: false, softPosition: false))
		{
			return null;
		}
		return table.CurrentRow["Value"].Value;
	}
}
