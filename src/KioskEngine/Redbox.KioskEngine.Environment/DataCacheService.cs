using System;
using System.IO;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using VistaDB.DDA;

namespace Redbox.KioskEngine.Environment;

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
			using IVistaDBTable vistaDBTable = DataStoreCache.GetProfileStore(GetPath()).OpenTable("Cache", exclusive: false, readOnly: true);
			if (vistaDBTable.Find($"Name: '{name}'; Type: {(int)type}", "IX_Key", partialMatching: false, softPosition: false))
			{
				return (byte[])vistaDBTable.CurrentRow["Data"].Value;
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
