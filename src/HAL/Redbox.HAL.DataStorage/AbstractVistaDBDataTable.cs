using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Timers;
using VistaDB;
using VistaDB.Provider;

namespace Redbox.HAL.DataStorage;

public abstract class AbstractVistaDBDataTable<T> : IDataTable<T>
{
	private delegate string Statement(T obj);

	private readonly bool UseTransaction;

	private readonly string ConnectionString;

	public bool Exists
	{
		get
		{
			using VistaDBConnection vistaDBConnection = Connect();
			try
			{
				vistaDBConnection.GetTableSchema(Name);
				return true;
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
		int num = -1;
		using VistaDBConnection connection = Connect();
		using VistaDBCommand vistaDBCommand = new VistaDBCommand($"select count(*) from {Name}", connection);
		return (int)vistaDBCommand.ExecuteScalar();
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
		using VistaDBConnection connection = Connect();
		using VistaDBCommand vistaDBCommand = new VistaDBCommand(query, connection);
		using VistaDBDataReader vistaDBDataReader = vistaDBCommand.ExecuteReader();
		while (vistaDBDataReader.Read())
		{
			callback?.Invoke(vistaDBDataReader);
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
		ConnectionString = new VistaDBConnectionStringBuilder
		{
			DataSource = descriptor.Source,
			Pooling = descriptor.SupportsPooling,
			OpenMode = ((!descriptor.ExclusiveReadWrite) ? VistaDBDatabaseOpenMode.NonexclusiveReadWrite : VistaDBDatabaseOpenMode.ExclusiveReadWrite)
		}.ConnectionString;
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

	protected VistaDBConnection Connect()
	{
		VistaDBConnection vistaDBConnection = new VistaDBConnection(ConnectionString);
		vistaDBConnection.Open();
		return vistaDBConnection;
	}

	private void UpdateTableKeys(TextWriter writer, ErrorList errors)
	{
		bool requiresAlter = true;
		ExectuteSelectQuery(delegate(DbDataReader reader)
		{
			if (reader.GetString(0).Contains(PrimaryKeyToken))
			{
				requiresAlter = false;
			}
		}, "select name from [database schema] where typeid = 2");
		if (requiresAlter)
		{
			using (VistaDBConnection connection = Connect())
			{
				string text = $"alter table {Name} add constraint {PrimaryKeyToken} PRIMARY KEY {PrimaryKeyColumns}";
				using VistaDBCommand vistaDBCommand = new VistaDBCommand();
				vistaDBCommand.CommandText = text;
				vistaDBCommand.Connection = connection;
				try
				{
					vistaDBCommand.ExecuteNonQuery();
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
		}
		writer.WriteLine("[{0}] Table does not require alter.", Name);
	}

	private bool ExecuteStatement(Statement s, T obj)
	{
		string text = s(obj);
		int num = ExecuteStatement(text);
		if (1 != num)
		{
			LogHelper.Instance.Log("[AbstractSQLDataTable] ERROR: Execute statement '{0}' returned {1}", text, num);
			return false;
		}
		return true;
	}

	private bool ExecuteStatements(Statement s, IList<T> entries)
	{
		List<string> list = new List<string>();
		using (new DisposeableList<string>(list))
		{
			foreach (T entry in entries)
			{
				list.Add(s(entry));
			}
			if (list.Count == 0)
			{
				return false;
			}
			int num = (UseTransaction ? ExecuteTransaction(list) : ExecuteStatements(list));
			if (entries.Count != num)
			{
				LogHelper.Instance.Log("[AbstractSQLDataTable] ERROR: Execute statements returned {0} ( expected {1} )", num, entries.Count);
				return false;
			}
			return true;
		}
	}

	private int ExecuteStatement(string statement)
	{
		using VistaDBConnection connection = Connect();
		using VistaDBCommand vistaDBCommand = new VistaDBCommand();
		vistaDBCommand.Connection = connection;
		vistaDBCommand.CommandText = statement;
		try
		{
			return vistaDBCommand.ExecuteNonQuery();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"[RunStatement] of '{statement}' caught an exception.", e);
			return -1;
		}
	}

	private int ExecuteStatements(List<string> statements)
	{
		int num = 0;
		using VistaDBConnection connection = Connect();
		using VistaDBCommand vistaDBCommand = new VistaDBCommand();
		vistaDBCommand.Connection = connection;
		foreach (string statement in statements)
		{
			string arg = (vistaDBCommand.CommandText = statement);
			try
			{
				if (1 == vistaDBCommand.ExecuteNonQuery())
				{
					num++;
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log($"[RunStatement] of '{arg}' caught an exception.", e);
			}
		}
		return num;
	}

	private int ExecuteTransaction(List<string> statements)
	{
		using VistaDBConnection vistaDBConnection = Connect();
		VistaDBTransaction vistaDBTransaction = vistaDBConnection.BeginTransaction();
		using VistaDBCommand vistaDBCommand = new VistaDBCommand();
		vistaDBCommand.Transaction = vistaDBTransaction;
		vistaDBCommand.Connection = vistaDBConnection;
		try
		{
			foreach (string statement in statements)
			{
				vistaDBCommand.CommandText = statement;
				vistaDBCommand.ExecuteNonQuery();
			}
			vistaDBTransaction.Commit();
			return statements.Count;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("[ExecuteTransaction] of caught an exception.", e);
			try
			{
				vistaDBTransaction.Rollback();
				LogHelper.Instance.Log("[ExecuteTransaction] Rollback succeeded.");
			}
			catch (Exception e2)
			{
				LogHelper.Instance.Log("[ExecuteTransaction] Rollback Exception ", e2);
			}
			return 0;
		}
	}
}
