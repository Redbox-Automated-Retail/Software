using System;
using System.Globalization;
using Microsoft.Data.Sqlite;

namespace Redbox.Data.Sqlite;

// How values that VistaDB stored natively are written in the SQLite files.
public static class SqliteValues
{
	// Sortable, so that comparisons in SQL (e.g. "SampleDate < '...'") work on the text; fractional seconds only when
	// present, as Microsoft.Data.Sqlite also writes DateTime parameters.
	public const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss.FFFFFFF";

	public static string FormatDateTime(DateTime value)
	{
		return value.ToString(DateTimeFormat, CultureInfo.InvariantCulture);
	}

	// Text columns compare case-insensitively, as in the VistaDB databases (created with CASE SENSITIVE FALSE).
	public const string TextType = "TEXT COLLATE NOCASE";

	// Connections are not pooled, so a closed connection releases the file (VistaDB behaved that way, and files such
	// as profile.data are replaced while the engine runs).
	public static SqliteConnection Open(string path, bool readOnly = false)
	{
		SqliteConnectionStringBuilder builder = new SqliteConnectionStringBuilder
		{
			DataSource = path,
			Mode = readOnly ? SqliteOpenMode.ReadOnly : SqliteOpenMode.ReadWriteCreate,
			Pooling = false,
			DefaultTimeout = 30
		};
		SqliteConnection connection = new SqliteConnection(builder.ToString());
		connection.Open();
		return connection;
	}

	public static bool TableExists(SqliteConnection connection, string tableName)
	{
		using SqliteCommand command = connection.CreateCommand();
		command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = @name COLLATE NOCASE";
		command.Parameters.AddWithValue("@name", tableName);
		return Convert.ToInt64(command.ExecuteScalar(), CultureInfo.InvariantCulture) > 0;
	}

	public static int ExecuteNonQuery(SqliteConnection connection, string sql)
	{
		using SqliteCommand command = connection.CreateCommand();
		command.CommandText = sql;
		return command.ExecuteNonQuery();
	}
}
