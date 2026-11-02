using System;
using System.Data;
using System.IO;
using Microsoft.Data.Sqlite;
using Redbox.Core;
using Redbox.Data.Sqlite;

namespace Redbox.Rental.Data;

// .NET 10 build: profile.data read through SQLite (profile.sqlite, converted again whenever the update store delivers a
// new profile.data) with the same API as VistaDBDatabaseProvider. The queries ProfileData sends are valid SQLite as
// they are; VistaDB's "SET OPTIMIZATION ON;" prefix is not sent.
internal class SqliteDatabaseProvider : IDisposable
{
	private readonly string _path;

	private readonly string _name;

	private SqliteConnection? _connection;

	public SqliteDatabaseProvider(string name, string path)
	{
		_path = path;
		_name = name;
	}

	public bool Open()
	{
		try
		{
			string sqlitePath = LegacyDatabase.Resolve(_path, LegacyDatabaseKind.ReadOnlySource, delegate(string message)
			{
				LogHelper.Instance.Log(message);
			});
			if (!File.Exists(sqlitePath))
			{
				LogHelper.Instance.Log("Database: {0} Path:{1} does not exist", _name, _path);
				return false;
			}
			_connection = SqliteValues.Open(sqlitePath, readOnly: true);
			return true;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Unhandled exception in SqliteDatabaseProvider.Open", e);
			return false;
		}
	}

	public bool Close()
	{
		try
		{
			_connection?.Dispose();
			_connection = null;
			return true;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Unhandled exception in SqliteDatabaseProvider.Close", e);
			return false;
		}
	}

	public DataTable? GetSqlData(string sql)
	{
		return GetSqlData(sql, nolog: false);
	}

	public DataTable? GetSqlData(string sql, bool nolog)
	{
		DateTime now = DateTime.Now;
		try
		{
			DataTable dataTable = new DataTable();
			using (SqliteCommand command = _connection!.CreateCommand()) // opened by Open; throws otherwise (caught below), as before
			{
				command.CommandText = sql;
				using SqliteDataReader reader = command.ExecuteReader();
				Fill(dataTable, reader);
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

	// DataTable.Load asks the reader for its schema table, which Microsoft.Data.Sqlite builds with several catalog queries
	// per call; ProfileData runs one query per cached image (about 10,000), so the columns are taken from the reader directly.
	private static void Fill(DataTable table, SqliteDataReader reader)
	{
		for (int i = 0; i < reader.FieldCount; i++)
		{
			table.Columns.Add(reader.GetName(i), typeof(object));
		}
		object[] values = new object[reader.FieldCount];
		table.BeginLoadData();
		while (reader.Read())
		{
			reader.GetValues(values);
			table.Rows.Add(values);
		}
		table.EndLoadData();
	}
}
