using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using Microsoft.Data.Sqlite;
using Redbox.Core;
using Redbox.Data.Sqlite;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Environment;

// .NET 10 build: the key/value data stores (Lua's data store functions, LocalData, ...) on SQLite, with the same
// tables as the VistaDB version: one table per name, columns Key (unique, case-insensitive) and Value (the Lua value as
// formatted by IKernelService.FormatLuaValue).
//
// Rows come back in insertion order (ORDER BY rowid), as VistaDB walked them; converted tables keep VistaDB's order.
//
// Differences from VistaDbDataStoreProvider, on purpose: GetAll/GetKeys skip rows with an empty key or value instead of
// looping forever on them (the VistaDB loop only advanced inside its "if").
internal class SqliteDataStoreProvider : DataStoreProvider
{
	protected override void OnClear(string dataStore, string tableName)
	{
		Run(dataStore, tableName, "OnClear", requireTable: true, delegate(SqliteConnection connection)
		{
			SqliteValues.ExecuteNonQuery(connection, $"DELETE FROM {Quote(tableName)}");
			return 0;
		});
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
		return Run(dataStore, tableName, "OnCount", requireTable: true, delegate(SqliteConnection connection)
		{
			using SqliteCommand command = connection.CreateCommand();
			command.CommandText = $"SELECT COUNT(*) FROM {Quote(tableName)}";
			return Convert.ToInt64(command.ExecuteScalar(), CultureInfo.InvariantCulture);
		});
	}

	protected override object OnGet(string dataStore, string tableName, string keyName)
	{
		return Run(dataStore, tableName, $"OnGet({dataStore},{tableName},{keyName})", requireTable: true, (SqliteConnection connection) => InternalGet(connection, tableName, keyName));
	}

	protected override Dictionary<string, object> OnGetAll(string dataStore, string tableName)
	{
		return Run(dataStore, tableName, "OnGetAll", requireTable: true, delegate(SqliteConnection connection)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			using SqliteCommand command = connection.CreateCommand();
			command.CommandText = $"SELECT [Key], [Value] FROM {Quote(tableName)} ORDER BY rowid";
			using SqliteDataReader reader = command.ExecuteReader();
			while (reader.Read())
			{
				string key = reader.IsDBNull(0) ? null : reader.GetString(0);
				string value = reader.IsDBNull(1) ? null : reader.GetString(1);
				if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
				{
					dictionary[key] = value;
				}
			}
			return dictionary;
		}) ?? new Dictionary<string, object>();
	}

	protected override void OnDrop(string dataStore, string tableName)
	{
		Run(dataStore, tableName, "OnDrop", requireTable: true, delegate(SqliteConnection connection)
		{
			SqliteValues.ExecuteNonQuery(connection, $"DROP TABLE {Quote(tableName)}");
			return 0;
		});
	}

	protected override ReadOnlyCollection<string> OnGetKeys(string dataStore, string tableName)
	{
		return (Run(dataStore, tableName, "OnGetKeys", requireTable: true, delegate(SqliteConnection connection)
		{
			List<string> list = new List<string>();
			using SqliteCommand command = connection.CreateCommand();
			command.CommandText = $"SELECT [Key] FROM {Quote(tableName)} ORDER BY rowid";
			using SqliteDataReader reader = command.ExecuteReader();
			while (reader.Read())
			{
				if (!reader.IsDBNull(0) && reader.GetString(0).Length > 0)
				{
					list.Add(reader.GetString(0));
				}
			}
			return list;
		}) ?? new List<string>()).AsReadOnly();
	}

	protected override void OnRemove(string dataStore, string tableName, string keyName)
	{
		Run(dataStore, tableName, $"OnRemove({dataStore},{tableName},{keyName})", requireTable: true, delegate(SqliteConnection connection)
		{
			using SqliteCommand command = connection.CreateCommand();
			command.CommandText = $"DELETE FROM {Quote(tableName)} WHERE [Key] = @key";
			command.Parameters.AddWithValue("@key", keyName);
			command.ExecuteNonQuery();
			return 0;
		});
	}

	protected override void OnSet(string dataStore, string tableName, string keyName, object data)
	{
		OnBulkSet(dataStore, tableName, new Dictionary<string, object> { [keyName] = data }, $"OnSet({dataStore},{tableName},{keyName})");
	}

	protected override void OnBulkSet(string dataStore, string tableName, IDictionary<string, object> values)
	{
		OnBulkSet(dataStore, tableName, values, $"OnBulkSet({dataStore},{tableName})");
	}

	protected override ReadOnlyCollection<string> OnGetTables(string dataStore)
	{
		SqliteConnection connection = DataStoreCache.GetWorkStore(DataStoreProvider.GetPath(dataStore));
		List<string> list = new List<string>();
		if (connection == null)
		{
			return list.AsReadOnly();
		}
		try
		{
			lock (connection)
			{
				using SqliteCommand command = connection.CreateCommand();
				command.CommandText = $"SELECT name FROM sqlite_master WHERE type = 'table' AND name NOT LIKE 'sqlite\\_%' ESCAPE '\\' AND name <> '{ConversionStamp.TableName}' ORDER BY rowid";
				using SqliteDataReader reader = command.ExecuteReader();
				while (reader.Read())
				{
					list.Add(reader.GetString(0));
				}
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"Exception in SqliteDataStoreProvider.OnGetTables({dataStore})", e);
			throw;
		}
		return list.AsReadOnly();
	}

	private void OnBulkSet(string dataStore, string tableName, IDictionary<string, object> values, string operation)
	{
		SqliteConnection connection = DataStoreCache.GetWorkStore(DataStoreProvider.GetPath(dataStore));
		if (connection == null)
		{
			return;
		}
		IKernelService kernelService = ServiceLocator.Instance.GetService<IKernelService>();
		try
		{
			lock (connection)
			{
				using SqliteTransaction transaction = connection.BeginTransaction();
				if (!SqliteValues.TableExists(connection, tableName))
				{
					using SqliteCommand create = connection.CreateCommand();
					create.Transaction = transaction;
					create.CommandText = $"CREATE TABLE {Quote(tableName)} ( [Key] {SqliteValues.TextType} NOT NULL, [Value] {SqliteValues.TextType}, CONSTRAINT [IX_Key] PRIMARY KEY ( [Key] ) )";
					create.ExecuteNonQuery();
				}
				using SqliteCommand command = connection.CreateCommand();
				command.Transaction = transaction;
				// An existing key (compared case-insensitively, like VistaDB's IX_Key) keeps its spelling; only the value changes.
				command.CommandText = $"INSERT INTO {Quote(tableName)} ( [Key], [Value] ) VALUES ( @key, @value ) ON CONFLICT ( [Key] ) DO UPDATE SET [Value] = excluded.[Value]";
				SqliteParameter key = command.Parameters.Add("@key", SqliteType.Text);
				SqliteParameter value = command.Parameters.Add("@value", SqliteType.Text);
				foreach (KeyValuePair<string, object> item in values)
				{
					key.Value = item.Key;
					value.Value = (object)kernelService.FormatLuaValue(item.Value) ?? DBNull.Value;
					command.ExecuteNonQuery();
				}
				transaction.Commit();
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"Exception in SqliteDataStoreProvider.{operation}", e);
			throw;
		}
	}

	private static T Run<T>(string dataStore, string tableName, string operation, bool requireTable, Func<SqliteConnection, T> action)
	{
		try
		{
			SqliteConnection connection = DataStoreCache.GetWorkStore(DataStoreProvider.GetPath(dataStore));
			if (connection == null)
			{
				return default(T);
			}
			lock (connection)
			{
				if (requireTable && !SqliteValues.TableExists(connection, tableName))
				{
					return default(T);
				}
				return action(connection);
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"Exception in SqliteDataStoreProvider.{operation}", e);
			throw;
		}
	}

	private static object InternalGet(SqliteConnection connection, string tableName, string keyName)
	{
		using SqliteCommand command = connection.CreateCommand();
		command.CommandText = $"SELECT [Value] FROM {Quote(tableName)} WHERE [Key] = @key";
		command.Parameters.AddWithValue("@key", keyName);
		object value = command.ExecuteScalar();
		return value is DBNull ? null : value;
	}

	// Table names come from Lua; double quotes can be escaped ([...] cannot in SQLite).
	private static string Quote(string tableName)
	{
		return "\"" + tableName.Replace("\"", "\"\"") + "\"";
	}
}
