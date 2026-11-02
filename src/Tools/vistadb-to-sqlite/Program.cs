using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Data.Sqlite;
using Redbox.Data.Sqlite;
using VistaDB;
using VistaDB.DDA;

namespace Redbox.Tools.VistaDbToSqlite;

// Added on the modernize branch (not decompiled). Converts a VistaDB 3.5 database (the kiosk's .data / .vdb3 files)
// into a SQLite database for the .NET 10 builds: every table with its columns, rows, primary key and indexes. Runs on
// .NET Framework because VistaDB.NET20 does. The .NET 10 components start it themselves (Redbox.Data.Sqlite.LegacyDatabase);
// it can also be run by hand:
//
//   vistadb-to-sqlite <source.vdb3|.data> <destination.sqlite> [--overwrite]
//
// Exit codes: 0 converted, 1 usage, 2 conversion failed (no destination file is left behind).
internal static class Program
{
	private static int Main(string[] args)
	{
		List<string> paths = args.Where(a => !a.StartsWith("--", StringComparison.Ordinal)).ToList();
		bool overwrite = args.Contains("--overwrite");
		if (paths.Count != 2)
		{
			Console.Error.WriteLine("usage: vistadb-to-sqlite <source> <destination.sqlite> [--overwrite]");
			return 1;
		}
		string source = Path.GetFullPath(paths[0]);
		string destination = Path.GetFullPath(paths[1]);
		if (!File.Exists(source) || !LegacyDatabase.IsVistaDb(source))
		{
			Console.Error.WriteLine($"{source}: not a VistaDB database");
			return 1;
		}
		if (File.Exists(destination))
		{
			if (!overwrite)
			{
				Console.Error.WriteLine($"{destination} exists (use --overwrite)");
				return 1;
			}
			File.Delete(destination);
		}
		Stopwatch timer = Stopwatch.StartNew();
		try
		{
			Convert(source, destination);
			Console.WriteLine($"vistadb-to-sqlite: {Path.GetFileName(source)} -> {Path.GetFileName(destination)} in {timer.Elapsed.TotalSeconds:0.0} s");
			return 0;
		}
		catch (Exception e)
		{
			Console.Error.WriteLine($"vistadb-to-sqlite: converting {source} failed: {e}");
			try
			{
				if (File.Exists(destination))
				{
					File.Delete(destination);
				}
			}
			catch (IOException)
			{
			}
			return 2;
		}
	}

	private static void Convert(string source, string destination)
	{
		using IVistaDBDDA dda = VistaDBEngine.Connections.OpenDDA();
		using IVistaDBDatabase database = dda.OpenDatabase(source, VistaDBDatabaseOpenMode.SharedReadOnly, null);
		bool caseSensitive = database.CaseSensitive;
		using SqliteConnection sqlite = SqliteValues.Open(destination);
		SqliteValues.ExecuteNonQuery(sqlite, "PRAGMA journal_mode = DELETE");
		HashSet<string> indexNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		foreach (string tableName in database.EnumTables().Cast<string>())
		{
			using IVistaDBTableSchema schema = database.TableSchema(tableName);
			TableShape shape = TableShape.From(schema, caseSensitive);
			SqliteValues.ExecuteNonQuery(sqlite, shape.CreateTableSql());
			long rows = CopyRows(database, tableName, shape, sqlite);
			CopyIdentity(shape, tableName, sqlite);
			foreach (string sql in shape.CreateIndexSql(indexNames))
			{
				SqliteValues.ExecuteNonQuery(sqlite, sql);
			}
			Console.WriteLine($"  {tableName}: {shape.Columns.Count} columns, {rows} rows, {shape.Indexes.Count} indexes{(shape.IdentityColumn == null ? string.Empty : $", identity {shape.IdentityColumn} seed {shape.IdentitySeed} step {shape.IdentityStep}")}{(shape.IdentityKeyColumn == null ? string.Empty : " (AUTOINCREMENT key)")}, primary {string.Join("+", shape.Indexes.Where(i => i.Primary).SelectMany(i => i.Columns).Select(c => c.Key))}");
		}
		ConversionStamp.FromFile(source).Write(sqlite);
	}

	private static long CopyRows(IVistaDBDatabase database, string tableName, TableShape shape, SqliteConnection sqlite)
	{
		long count = 0;
		using IVistaDBTable table = database.OpenTable(tableName, exclusive: false, readOnly: true);
		using SqliteTransaction transaction = sqlite.BeginTransaction();
		using SqliteCommand insert = sqlite.CreateCommand();
		insert.Transaction = transaction;
		insert.CommandText = $"INSERT INTO [{tableName}] ( {string.Join(", ", shape.Columns.Select(c => $"[{c.Name}]"))} ) VALUES ( {string.Join(", ", shape.Columns.Select((c, i) => $"@p{i}"))} )";
		List<SqliteParameter> parameters = shape.Columns.Select((c, i) => insert.Parameters.Add($"@p{i}", SqliteType.Text)).ToList();
		table.First();
		while (!table.EndOfTable)
		{
			IVistaDBRow row = table.CurrentRow;
			for (int i = 0; i < shape.Columns.Count; i++)
			{
				IVistaDBColumn column = row[shape.Columns[i].Name];
				object? value = column == null || column.IsNull ? null : column.Value;
				SetParameter(parameters[i], value);
			}
			insert.ExecuteNonQuery();
			count++;
			table.Next();
		}
		transaction.Commit();
		return count;
	}

	// VistaDB's identity continues from its seed (the next value; the schema keeps it current), even when the rows that
	// used the earlier values are gone (the message queue is usually empty). SQLite's AUTOINCREMENT continues after
	// sqlite_sequence, so seed - step goes there (or the highest ID, if that is larger).
	private static void CopyIdentity(TableShape shape, string tableName, SqliteConnection sqlite)
	{
		if (shape.IdentityKeyColumn == null || !long.TryParse(shape.IdentitySeed, NumberStyles.Integer, CultureInfo.InvariantCulture, out long seed))
		{
			return;
		}
		if (!long.TryParse(shape.IdentityStep, NumberStyles.Integer, CultureInfo.InvariantCulture, out long step) || step <= 0)
		{
			step = 1;
		}
		using SqliteCommand command = sqlite.CreateCommand();
		command.CommandText = "DELETE FROM sqlite_sequence WHERE name = @name; INSERT INTO sqlite_sequence ( name, seq ) VALUES ( @name, MAX(@seq, IFNULL((SELECT MAX(rowid) FROM [" + tableName + "]), 0)) )";
		command.Parameters.AddWithValue("@name", tableName);
		command.Parameters.AddWithValue("@seq", seed - step);
		command.ExecuteNonQuery();
	}

	private static void SetParameter(SqliteParameter parameter, object? value)
	{
		switch (value)
		{
		case null:
		case DBNull _:
			parameter.SqliteType = SqliteType.Text;
			parameter.Value = DBNull.Value;
			break;
		case string s:
			parameter.SqliteType = SqliteType.Text;
			parameter.Value = s;
			break;
		case bool b:
			parameter.SqliteType = SqliteType.Integer;
			parameter.Value = b ? 1L : 0L;
			break;
		case byte or sbyte or short or ushort or int or uint or long:
			parameter.SqliteType = SqliteType.Integer;
			parameter.Value = System.Convert.ToInt64(value, CultureInfo.InvariantCulture);
			break;
		case float or double:
			parameter.SqliteType = SqliteType.Real;
			parameter.Value = System.Convert.ToDouble(value, CultureInfo.InvariantCulture);
			break;
		case decimal d:
			// Text keeps the exact value; Microsoft.Data.Sqlite reads it back with GetDecimal.
			parameter.SqliteType = SqliteType.Text;
			parameter.Value = d.ToString(CultureInfo.InvariantCulture);
			break;
		case DateTime dt:
			parameter.SqliteType = SqliteType.Text;
			parameter.Value = SqliteValues.FormatDateTime(dt);
			break;
		case Guid g:
			parameter.SqliteType = SqliteType.Text;
			parameter.Value = g.ToString();
			break;
		case byte[] bytes:
			parameter.SqliteType = SqliteType.Blob;
			parameter.Value = bytes;
			break;
		default:
			parameter.SqliteType = SqliteType.Text;
			parameter.Value = System.Convert.ToString(value, CultureInfo.InvariantCulture);
			break;
		}
	}
}
