using System;
using System.Globalization;
using System.IO;
using Microsoft.Data.Sqlite;

namespace Redbox.Data.Sqlite;

// Records in a converted SQLite file which VistaDB file it was made from, so a changed source (profile.data is
// replaced by the update store) is converted again and an unchanged one is not.
public sealed class ConversionStamp
{
	public const string TableName = "__vistadb_source";

	public string SourceName { get; set; }

	public long SourceLength { get; set; }

	public DateTime SourceLastWriteUtc { get; set; }

	public static ConversionStamp FromFile(string path)
	{
		FileInfo info = new FileInfo(path);
		return new ConversionStamp
		{
			SourceName = info.Name,
			SourceLength = info.Length,
			SourceLastWriteUtc = info.LastWriteTimeUtc
		};
	}

	public bool Matches(ConversionStamp other)
	{
		return other != null
			&& string.Equals(SourceName, other.SourceName, StringComparison.OrdinalIgnoreCase)
			&& SourceLength == other.SourceLength
			&& SourceLastWriteUtc == other.SourceLastWriteUtc;
	}

	public void Write(SqliteConnection connection)
	{
		SqliteValues.ExecuteNonQuery(connection, $"CREATE TABLE IF NOT EXISTS [{TableName}] ( SourceName TEXT NOT NULL, SourceLength INTEGER NOT NULL, SourceLastWriteUtc TEXT NOT NULL, ConvertedUtc TEXT NOT NULL )");
		SqliteValues.ExecuteNonQuery(connection, $"DELETE FROM [{TableName}]");
		using SqliteCommand command = connection.CreateCommand();
		command.CommandText = $"INSERT INTO [{TableName}] ( SourceName, SourceLength, SourceLastWriteUtc, ConvertedUtc ) VALUES ( @name, @length, @written, @converted )";
		command.Parameters.AddWithValue("@name", SourceName);
		command.Parameters.AddWithValue("@length", SourceLength);
		command.Parameters.AddWithValue("@written", SourceLastWriteUtc.Ticks.ToString(CultureInfo.InvariantCulture));
		command.Parameters.AddWithValue("@converted", SqliteValues.FormatDateTime(DateTime.UtcNow));
		command.ExecuteNonQuery();
	}

	public static ConversionStamp Read(string sqlitePath)
	{
		if (!File.Exists(sqlitePath))
		{
			return null;
		}
		try
		{
			using SqliteConnection connection = SqliteValues.Open(sqlitePath, readOnly: true);
			if (!SqliteValues.TableExists(connection, TableName))
			{
				return null;
			}
			using SqliteCommand command = connection.CreateCommand();
			command.CommandText = $"SELECT SourceName, SourceLength, SourceLastWriteUtc FROM [{TableName}] LIMIT 1";
			using SqliteDataReader reader = command.ExecuteReader();
			if (!reader.Read())
			{
				return null;
			}
			return new ConversionStamp
			{
				SourceName = reader.GetString(0),
				SourceLength = reader.GetInt64(1),
				SourceLastWriteUtc = new DateTime(long.Parse(reader.GetString(2), CultureInfo.InvariantCulture), DateTimeKind.Utc)
			};
		}
		catch (SqliteException)
		{
			return null;
		}
	}
}
