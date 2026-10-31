using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Data.Sqlite;
using Redbox.Core;
using Redbox.Data.Sqlite;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Environment;

// .NET 10 build: SQLite in place of VistaDB (the .NET Framework build keeps DataStoreCache.cs). The callers still pass
// the VistaDB path ("<store>.data"); the database is "<store>.sqlite" next to it, converted from the VistaDB file the
// first time (see Redbox.Data.Sqlite.LegacyDatabase).
public class DataStoreCache
{
	private static readonly IDictionary<string, SqliteConnection> m_databases = new Dictionary<string, SqliteConnection>(StringComparer.OrdinalIgnoreCase);

	public static void CloseAll()
	{
		LogHelper.Instance.Log("Close all data store caches.");
		lock (m_databases)
		{
			foreach (string item in new List<string>(m_databases.Keys))
			{
				LogHelper.Instance.Log("...Close data store: {0}", item);
				CloseStore(item);
			}
		}
	}

	public static void CloseStore(string path)
	{
		lock (m_databases)
		{
			if (m_databases.TryGetValue(path, out SqliteConnection connection))
			{
				connection.Dispose();
				m_databases.Remove(path);
			}
		}
	}

	// Data stores the engine writes (Local.data, cache.data, ...): converted once, created when missing.
	public static SqliteConnection GetWorkStore(string path)
	{
		return GetStore(path, readOnly: false);
	}

	// profile.data, delivered by the update store: converted again whenever it changes; null when it does not exist.
	public static SqliteConnection GetProfileStore(string path)
	{
		return GetStore(path, readOnly: true);
	}

	private static SqliteConnection GetStore(string path, bool readOnly)
	{
		lock (m_databases)
		{
			try
			{
				if (m_databases.TryGetValue(path, out SqliteConnection cached))
				{
					return cached;
				}
				string sqlitePath = LegacyDatabase.Resolve(path, readOnly ? LegacyDatabaseKind.ReadOnlySource : LegacyDatabaseKind.Writable, delegate(string message)
				{
					LogHelper.Instance.Log(message);
				});
				if (readOnly && !File.Exists(sqlitePath))
				{
					return null;
				}
				SqliteConnection connection = SqliteValues.Open(sqlitePath, readOnly);
				m_databases[path] = connection;
				return connection;
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("Unhandled exception in DataStoreCache.GetStore; file :" + Path.GetFileName(path) + ".", e);
				ServiceLocator.Instance.GetService<IEnvironmentNotificationService>()?.RaiseCorruptDb();
				throw;
			}
		}
	}
}
