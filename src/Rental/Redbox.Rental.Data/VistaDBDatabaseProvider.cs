using System;
using System.Data;
using System.IO;
using Redbox.Core;
using VistaDB;
using VistaDB.DDA;
using VistaDB.Provider;

namespace Redbox.Rental.Data;

internal class VistaDBDatabaseProvider : IDisposable
{
	private string _path;

	private string _name;

	private IVistaDBDDA _dda;

	private IVistaDBDatabase _database;

	public VistaDBDatabaseProvider(string name, string path)
	{
		_path = path;
		_name = name;
	}

	public bool Open()
	{
		VistaDBDatabaseOpenMode mode = VistaDBDatabaseOpenMode.NonexclusiveReadWrite;
		try
		{
			if (!File.Exists(_path))
			{
				LogHelper.Instance.Log("Database: {0} Path:{1} does not exist", _name, _path);
				return false;
			}
			_dda = VistaDBEngine.Connections.OpenDDA();
			_database = _dda.OpenDatabase(_path, mode, null);
			return true;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Unhandled exception in VistaDBDatabaseProvider.Open", e);
			return false;
		}
	}

	public bool Close()
	{
		try
		{
			if (_database != null)
			{
				_database.Dispose();
				_database = null;
			}
			if (_dda != null)
			{
				_dda.Dispose();
				_dda = null;
			}
			return true;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Unhandled exception in VistaDBDatabaseProvider.Close", e);
			return false;
		}
	}

	public DataTable GetSqlData(string sql)
	{
		return GetSqlData(sql, nolog: false);
	}

	public DataTable GetSqlData(string sql, bool nolog)
	{
		DateTime now = DateTime.Now;
		try
		{
			DataTable dataTable = new DataTable();
			VistaDBConnection conn = new VistaDBConnection("Open Mode=NonExclusiveReadWrite;Data Source=" + _path);
			VistaDBDataAdapter vistaDBDataAdapter = new VistaDBDataAdapter("SET OPTIMIZATION ON; " + sql, conn);
			try
			{
				vistaDBDataAdapter.Fill(dataTable);
			}
			finally
			{
				((IDisposable)(object)vistaDBDataAdapter)?.Dispose();
			}
			TimeSpan timeSpan = DateTime.Now - now;
			if (!nolog)
			{
				LogHelper.Instance.Log($"{_name} GetSqlData({sql}) complete.  Rows: {dataTable.Rows.Count} Milliseconds: {timeSpan.TotalMilliseconds}");
			}
			return dataTable;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"Exception in Database {_name} GetSqlData({sql})", e);
		}
		return null;
	}

	public void Dispose()
	{
		Close();
	}
}
