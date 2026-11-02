using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.IO;
using Microsoft.Data.Sqlite;
using Redbox.Data.Sqlite;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Timers;

namespace Redbox.HAL.DataStorage;

// .NET 10 build: the HAL tables on SQLite. The class keeps its name (the tables derive from it) and runs the same SQL the
// tables generate for VistaDB, against HALData.sqlite / HALCounters.sqlite next to the .vdb3 files (converted from them
// the first time, see Redbox.Data.Sqlite.LegacyDatabase). Differences, all internal:
// - dates are written as sortable text (FormatDateTime), so the tables' date comparisons work on SQLite;
// - Exists / UpdateTableKeys use SQLite's catalog instead of VistaDB's [database schema];
// - a missing primary key is added as a unique index (SQLite cannot add a constraint to an existing table);
// - CREATE/DROP statements report 0 on success, as VistaDB's ExecuteNonQuery did.
public abstract class AbstractVistaDBDataTable<T> : IDataTable<T>
{
	private delegate string Statement(T obj);

	private readonly bool UseTransaction;

	private readonly string SourcePath;

	private string? m_databasePath;

	public bool Exists
	{
		get
		{
			try
			{
				using SqliteConnection connection = Connect();
				return SqliteValues.TableExists(connection, Name);
			}
			catch
			{
				return false;
			}
		}
	}

	public string Name { get; private set; }

	protected virtual string PrimaryKeyColumns => string.Empty;

	protected virtual string PrimaryKeyToken => string.Empty;

	public List<T> LoadEntries(bool debug)
	{
		using ExecutionTimer executionTimer = new ExecutionTimer();
		List<T> list = OnLoadEntries();
		executionTimer.Stop();
		if (debug)
		{
			LogHelper.Instance.Log("[{0}] Loaded {1} entries ( {2}ms )", Name, list.Count, executionTimer.ElapsedMilliseconds);
		}
		return list;
	}

	public List<T> LoadEntries()
	{
		return LoadEntries(debug: false);
	}

	public bool Update(T item)
	{
		return ExecuteStatement(UpdateStatement, item);
	}

	public bool Update(IList<T> entries)
	{
		return ExecuteStatements(UpdateStatement, entries);
	}

	public bool Insert(T p)
	{
		return ExecuteStatement(InsertStatement, p);
	}

	public bool Insert(IList<T> entries)
	{
		using ExecutionTimer executionTimer = new ExecutionTimer();
		bool result = ExecuteStatements(InsertStatement, entries);
		executionTimer.Stop();
		LogHelper.Instance.Log("[{0}] Insertion of {1} entries ( {2}ms )", Name, entries.Count, executionTimer.ElapsedMilliseconds);
		return result;
	}

	public bool Delete(T item)
	{
		return ExecuteStatement(DeleteStatement, item);
	}

	public bool Delete(IList<T> entries)
	{
		return ExecuteStatements(DeleteStatement, entries);
	}

	public int DeleteAll()
	{
		return ExecuteStatement($"DELETE FROM [{Name}]");
	}

	public bool Create()
	{
		return ExecuteStatement(CreateStatement()) == 0;
	}

	public bool Drop()
	{
		return ExecuteStatement($"DROP TABLE {Name}") >= 0;
	}

	public int Clean()
	{
		string text = CleanStatement();
		if (string.IsNullOrEmpty(text))
		{
			return 0;
		}
		return ExecuteStatement(text);
	}

	public int GetRowCount()
	{
		using SqliteConnection connection = Connect();
		using SqliteCommand command = connection.CreateCommand();
		command.CommandText = $"select count(*) from {Name}";
		return Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture);
	}

	public void UpdateTable(TextWriter writer, ErrorList errors)
	{
		if (!string.IsNullOrEmpty(PrimaryKeyToken))
		{
			UpdateTableKeys(writer, errors);
		}
	}

	public void ExectuteSelectQuery(OnSelectResult callback, string query)
	{
		using SqliteConnection connection = Connect();
		using SqliteCommand command = connection.CreateCommand();
		command.CommandText = query;
		using SqliteDataReader reader = command.ExecuteReader();
		while (reader.Read())
		{
			callback?.Invoke(reader);
		}
	}

	public bool Recreate(ErrorList errors)
	{
		if (!Drop())
		{
			errors.Add(Error.NewError("I345", "Drop table error", $"Failed to drop table '{Name}'."));
			return false;
		}
		if (!Create())
		{
			errors.Add(Error.NewError("I346", "Create table error", $"Failed to create table '{Name}'."));
			return false;
		}
		return true;
	}

	protected AbstractVistaDBDataTable(IDataTableDescriptor descriptor, string tableName)
	{
		SourcePath = descriptor.Source;
		Name = tableName;
		UseTransaction = descriptor.UseTransaction;
	}

	protected abstract List<T> OnLoadEntries();

	protected abstract string UpdateStatement(T obj);

	protected abstract string InsertStatement(T obj);

	protected abstract string DeleteStatement(T obj);

	protected abstract string CreateStatement();

	protected virtual string CleanStatement()
	{
		return string.Empty;
	}

	// How the tables write a DateTime into their SQL. VistaDB parsed DateTime.ToString(); SQLite keeps text, so it is
	// written sortable (to the second, as ToString() did).
	protected static string FormatDateTime(DateTime value)
	{
		return value.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
	}

	protected SqliteConnection Connect()
	{
		if (m_databasePath == null)
		{
			m_databasePath = LegacyDatabase.Resolve(SourcePath, LegacyDatabaseKind.Writable, delegate(string message)
			{
				LogHelper.Instance.Log(message);
			});
		}
		return SqliteValues.Open(m_databasePath);
	}

	private void UpdateTableKeys(TextWriter writer, ErrorList errors)
	{
		bool requiresAlter = true;
		using (SqliteConnection connection = Connect())
		{
			using SqliteCommand command = connection.CreateCommand();
			// A primary key from the CREATE statement or the conversion, or the unique index added below.
			command.CommandText = "SELECT (SELECT COUNT(*) FROM pragma_table_info(@table) WHERE pk > 0) + (SELECT COUNT(*) FROM sqlite_master WHERE type = 'index' AND tbl_name = @table COLLATE NOCASE AND name LIKE '%' || @token)";
			command.Parameters.AddWithValue("@table", Name);
			command.Parameters.AddWithValue("@token", PrimaryKeyToken);
			requiresAlter = Convert.ToInt64(command.ExecuteScalar(), CultureInfo.InvariantCulture) == 0;
		}
		if (requiresAlter)
		{
			string text = $"CREATE UNIQUE INDEX [{Name}_{PrimaryKeyToken}] ON [{Name}] {PrimaryKeyColumns}";
			try
			{
				using SqliteConnection connection = Connect();
				SqliteValues.ExecuteNonQuery(connection, text);
				writer.WriteLine("[{0}] Successfully altered table with PK info.", Name);
				return;
			}
			catch (Exception e)
			{
				errors.Add(Error.NewError($"{Name} table alter failure", $"Failed to alter table {Name} to include primary key", e));
				writer.WriteLine("[RunStatement] of '{0}' caught an exception.", text);
				return;
			}
		}
		writer.WriteLine("[{0}] Table does not require alter.", Name);
	}

	private bool ExecuteStatement(Statement s, T obj)
	{
		string text = s(obj);
		int affected = ExecuteStatement(text);
		if (1 != affected)
		{
			LogHelper.Instance.Log("[AbstractSQLDataTable] ERROR: Execute statement '{0}' returned {1}", text, affected);
			return false;
		}
		return true;
	}

	private bool ExecuteStatements(Statement s, IList<T> entries)
	{
		List<string> statements = new List<string>();
		using (new DisposeableList<string>(statements))
		{
			foreach (T entry in entries)
			{
				statements.Add(s(entry));
			}
			if (statements.Count == 0)
			{
				return false;
			}
			int executed = (UseTransaction ? ExecuteTransaction(statements) : ExecuteStatements(statements));
			if (entries.Count != executed)
			{
				LogHelper.Instance.Log("[AbstractSQLDataTable] ERROR: Execute statements returned {0} ( expected {1} )", executed, entries.Count);
				return false;
			}
			return true;
		}
	}

	private int ExecuteStatement(string statement)
	{
		using SqliteConnection connection = Connect();
		try
		{
			return Execute(connection, null, statement);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"[RunStatement] of '{statement}' caught an exception.", e);
			return -1;
		}
	}

	private int ExecuteStatements(List<string> statements)
	{
		int succeeded = 0;
		using SqliteConnection connection = Connect();
		foreach (string statement in statements)
		{
			try
			{
				if (1 == Execute(connection, null, statement))
				{
					succeeded++;
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log($"[RunStatement] of '{statement}' caught an exception.", e);
			}
		}
		return succeeded;
	}

	private int ExecuteTransaction(List<string> statements)
	{
		using SqliteConnection connection = Connect();
		SqliteTransaction transaction = connection.BeginTransaction();
		try
		{
			foreach (string statement in statements)
			{
				Execute(connection, transaction, statement);
			}
			transaction.Commit();
			return statements.Count;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("[ExecuteTransaction] of caught an exception.", e);
			try
			{
				transaction.Rollback();
				LogHelper.Instance.Log("[ExecuteTransaction] Rollback succeeded.");
			}
			catch (Exception e2)
			{
				LogHelper.Instance.Log("[ExecuteTransaction] Rollback Exception ", e2);
			}
			return 0;
		}
	}

	private static int Execute(SqliteConnection connection, SqliteTransaction? transaction, string statement)
	{
		using SqliteCommand command = connection.CreateCommand();
		command.Transaction = transaction;
		command.CommandText = statement;
		int affected = command.ExecuteNonQuery();
		string verb = statement.TrimStart().Split(' ')[0];
		if (verb.Equals("CREATE", StringComparison.OrdinalIgnoreCase) || verb.Equals("DROP", StringComparison.OrdinalIgnoreCase) || verb.Equals("ALTER", StringComparison.OrdinalIgnoreCase))
		{
			return 0;
		}
		return affected;
	}
}
