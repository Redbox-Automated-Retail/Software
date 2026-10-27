using System;
using System.Collections.Generic;
using System.IO;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using VistaDB;
using VistaDB.DDA;

namespace Redbox.KioskEngine.Environment;

public class DataStoreCache
{
	private sealed class DatabaseCache
	{
		public IVistaDBDDA DDA;

		public IVistaDBDatabase Instance;
	}

	private static readonly IDictionary<string, DatabaseCache> m_databases = new Dictionary<string, DatabaseCache>();

	public static void CloseAll()
	{
		LogHelper.Instance.Log("Close all data store caches.");
		foreach (string item in new List<string>(m_databases.Keys))
		{
			LogHelper.Instance.Log("...Close data store: {0}", item);
			CloseStore(item);
		}
	}

	public static void CloseStore(string path)
	{
		if (m_databases.ContainsKey(path))
		{
			DatabaseCache databaseCache = m_databases[path];
			databaseCache.Instance.Dispose();
			databaseCache.DDA.Dispose();
			m_databases.Remove(path);
		}
	}

	public static IVistaDBDatabase GetWorkStore(string path)
	{
		return GetStore(path, VistaDBDatabaseOpenMode.NonexclusiveReadWrite);
	}

	public static IVistaDBDatabase GetProfileStore(string path)
	{
		return GetStore(path, VistaDBDatabaseOpenMode.SharedReadOnly);
	}

	private static IVistaDBDatabase GetStore(string path, VistaDBDatabaseOpenMode mode)
	{
		try
		{
			if (m_databases.ContainsKey(path))
			{
				return m_databases[path].Instance;
			}
			DatabaseCache databaseCache = new DatabaseCache
			{
				DDA = VistaDBEngine.Connections.OpenDDA()
			};
			if (!File.Exists(path))
			{
				if (mode == VistaDBDatabaseOpenMode.SharedReadOnly)
				{
					return null;
				}
				databaseCache.Instance = databaseCache.DDA.CreateDatabase(path, stayExclusive: false, null, 0, 0, caseSensitive: false);
			}
			if (databaseCache.Instance == null)
			{
				databaseCache.Instance = databaseCache.DDA.OpenDatabase(path, mode, null);
			}
			m_databases[path] = databaseCache;
			return databaseCache.Instance;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Unhandled exception in DataStoreCache.GetStore; file :" + Path.GetFileName(path) + ".", e);
			ServiceLocator.Instance.GetService<IEnvironmentNotificationService>()?.RaiseCorruptDb();
			throw;
		}
	}
}
