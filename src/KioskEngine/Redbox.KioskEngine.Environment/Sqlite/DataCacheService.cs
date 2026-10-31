using System;
using System.IO;
using Microsoft.Data.Sqlite;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Environment;

// .NET 10 build: reads the Cache table of profile.data (converted to profile.sqlite) instead of opening it with VistaDB.
public class DataCacheService : IDataCacheService
{
	private static string _profilePath;

	public static DataCacheService Instance => Singleton<DataCacheService>.Instance;

	public void Shutdown()
	{
		LogHelper.Instance.Log("Shutdown DataCacheService.");
		DataStoreCache.CloseAll();
	}

	public byte[] GetContent(DataCacheType type, string name)
	{
		try
		{
			SqliteConnection connection = DataStoreCache.GetProfileStore(GetPath());
			lock (connection)
			{
				using SqliteCommand command = connection.CreateCommand();
				// The VistaDB lookup was IX_Key with "Name: '<name>'; Type: <type>".
				command.CommandText = "SELECT [Data] FROM [Cache] WHERE [Name] = @name AND [Type] = @type";
				command.Parameters.AddWithValue("@name", name);
				command.Parameters.AddWithValue("@type", (int)type);
				object data = command.ExecuteScalar();
				if (data != null && !(data is DBNull))
				{
					return (byte[])data;
				}
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Unable to load cached data due to unhandled exception.", e);
		}
		return null;
	}

	private static string GetPath()
	{
		if (string.IsNullOrWhiteSpace(_profilePath))
		{
			_profilePath = Path.Combine(ServiceLocator.Instance.GetService<IEngineApplication>().DataPath, "profile.data");
		}
		return _profilePath;
	}

	private DataCacheService()
	{
	}
}
