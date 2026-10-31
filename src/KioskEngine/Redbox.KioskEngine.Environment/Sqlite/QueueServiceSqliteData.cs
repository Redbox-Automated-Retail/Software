using System;
using System.Globalization;
using System.IO;
using Microsoft.Data.Sqlite;
using Redbox.Core;
using Redbox.Data.Sqlite;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Environment;

// .NET 10 build: the message queue (message.data) on SQLite, same table and behaviour as QueueServiceVistaDBData:
// Queue( ID identity, Priority, Type, Data, CreatedOn ) and System( SchemaVersion = 1 ), compacted at start-up, a
// corrupt file moved aside and recreated. The database is message.sqlite, converted from message.data the first time.
public class QueueServiceSqliteData : IQueueServiceData, IDisposable
{
	internal SqliteConnection Connection { get; set; }

	internal string DatabasePath { get; set; }

	public ErrorList Initialize(string path, int retry = 2)
	{
		ErrorList errorList = new ErrorList();
		if (retry <= 0)
		{
			LogHelper.Instance.LogError("Q997", "retry limit exceeded.");
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("Q997", "Unable to initialize the Queue Service database.", "retry limit exceeded."));
			return errorList;
		}
		try
		{
			DatabasePath = LegacyDatabase.Resolve(path, LegacyDatabaseKind.Writable, delegate(string message)
			{
				LogHelper.Instance.Log(message);
			});
			bool existed = File.Exists(DatabasePath);
			Connection = SqliteValues.Open(DatabasePath);
			if (existed)
			{
				LogHelper.Instance.Log("> Pack message database...");
				SqliteValues.ExecuteNonQuery(Connection, "VACUUM");
			}
			CreateSchema(Connection);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in QueueServiceSqliteData.Initialize.", e);
			try
			{
				Connection?.Dispose();
				Connection = null;
				string corrupt = File.Exists(DatabasePath ?? string.Empty) ? DatabasePath : path;
				string text = corrupt + ".corrupt";
				LogHelper.Instance.Log("Attempt to move '{0}' to '{1}'.", corrupt, text);
				File.Move(corrupt, text);
				LogHelper.Instance.Log("Attempt to retry QueueService.Initialize.");
				errorList.AddRange(Initialize(path, --retry));
			}
			catch (Exception e2)
			{
				LogHelper.Instance.Log("An unhandled exception was raised renaming a corrupt Queue Service data file.", e2);
				errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("Q999", "Unable to initialize the Queue Service database.", e));
			}
		}
		return errorList;
	}

	public ErrorList Clear()
	{
		return Process(delegate
		{
			SqliteValues.ExecuteNonQuery(Connection, "DELETE FROM Queue");
		}, "Clear");
	}

	public ErrorList GetDepth(out int count)
	{
		int tempCount = 0;
		ErrorList result = Process(delegate
		{
			using SqliteCommand command = Connection.CreateCommand();
			command.CommandText = "SELECT COUNT(*) FROM Queue";
			tempCount = Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture);
		}, "GetDepth");
		count = tempCount;
		return result;
	}

	public ErrorList DeleteMessage(int id)
	{
		return Process(delegate
		{
			using SqliteCommand command = Connection.CreateCommand();
			command.CommandText = "DELETE FROM Queue WHERE ID = @ID";
			command.Parameters.AddWithValue("@ID", id);
			command.ExecuteNonQuery();
		}, "DeleteMessage");
	}

	public ErrorList DeleteMessageByType(string type)
	{
		return Process(delegate
		{
			using SqliteCommand command = Connection.CreateCommand();
			command.CommandText = "DELETE FROM Queue WHERE Type = @Type";
			command.Parameters.AddWithValue("@Type", type);
			command.ExecuteNonQuery();
		}, "DeleteMessageByType");
	}

	public ErrorList Enqueue(byte priority, string type, byte[] encryptedMessage)
	{
		return Enqueue(priority, type, encryptedMessage, out _);
	}

	public ErrorList Enqueue(byte priority, string type, byte[] encryptedMessage, out int newMessageId)
	{
		int tempMessageId = 0;
		ErrorList result = Process(delegate
		{
			using SqliteCommand command = Connection.CreateCommand();
			// CreatedOn was the column default GETDATE() (local time) in the VistaDB schema.
			command.CommandText = "INSERT INTO Queue( Priority, Type, Data, CreatedOn ) VALUES( @Priority, @Type, @Data, @CreatedOn ); SELECT last_insert_rowid();";
			command.Parameters.AddWithValue("@Type", type);
			command.Parameters.AddWithValue("@Priority", (int)priority);
			command.Parameters.AddWithValue("@Data", encryptedMessage);
			command.Parameters.AddWithValue("@CreatedOn", SqliteValues.FormatDateTime(DateTime.Now));
			tempMessageId = Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture);
		}, "Enqueue");
		newMessageId = tempMessageId;
		return result;
	}

	public ErrorList Update(int id, byte[] message)
	{
		return Process(delegate
		{
			using SqliteCommand command = Connection.CreateCommand();
			command.CommandText = "UPDATE Queue SET Data = @Data WHERE ID = @Id";
			command.Parameters.AddWithValue("@Id", id);
			command.Parameters.AddWithValue("@Data", message);
			command.ExecuteNonQuery();
		}, "Update");
	}

	public ErrorList ForEach(Action<IMessage> action)
	{
		return Process(delegate
		{
			using SqliteCommand command = Connection.CreateCommand();
			command.CommandText = "SELECT ID, Type, Priority, Data, CreatedOn FROM Queue ORDER BY Priority DESC, ID ASC";
			using SqliteDataReader reader = command.ExecuteReader();
			while (reader.Read())
			{
				action(ToMessage(reader));
			}
		}, "ForEach");
	}

	public ErrorList ForEach(Action<IMessage> action, string type)
	{
		return Process(delegate
		{
			using SqliteCommand command = Connection.CreateCommand();
			command.CommandText = "SELECT ID, Type, Priority, Data, CreatedOn FROM Queue WHERE Type = @Type ORDER BY Priority DESC, ID ASC";
			command.Parameters.AddWithValue("@Type", type);
			using SqliteDataReader reader = command.ExecuteReader();
			while (reader.Read())
			{
				action(ToMessage(reader));
			}
		}, "ForEach");
	}

	public ErrorList SelectNextMessage(IQueueServicePriority queueServicePriority, out IMessage message)
	{
		IMessage resultMessage = null;
		ErrorList result = Process(delegate
		{
			using SqliteCommand command = Connection.CreateCommand();
			command.CommandText = "SELECT ID, Type, Priority, Data, CreatedOn FROM Queue WHERE Priority >= @PriorityValue ORDER BY Priority DESC, ID ASC LIMIT 1";
			command.Parameters.AddWithValue("@PriorityValue", queueServicePriority?.MinimumPriorityValue ?? 0);
			using SqliteDataReader reader = command.ExecuteReader();
			if (reader.Read())
			{
				resultMessage = ToMessage(reader);
			}
		}, "SelectNextMessage");
		message = resultMessage;
		return result;
	}

	public void Dispose()
	{
		Connection?.Dispose();
	}

	// The VistaDB schema after migration Migrations.message.data.0000001.sql (SchemaVersion 1).
	private static void CreateSchema(SqliteConnection connection)
	{
		SqliteValues.ExecuteNonQuery(connection, $"CREATE TABLE IF NOT EXISTS System( [Key] {SqliteValues.TextType} NOT NULL PRIMARY KEY, Value {SqliteValues.TextType} )");
		SqliteValues.ExecuteNonQuery(connection, "INSERT OR IGNORE INTO System([Key],Value) VALUES( 'SchemaVersion', '1' )");
		SqliteValues.ExecuteNonQuery(connection, $"CREATE TABLE IF NOT EXISTS Queue( ID INTEGER PRIMARY KEY AUTOINCREMENT, Priority INTEGER NOT NULL, Type {SqliteValues.TextType} NOT NULL, Data BLOB NOT NULL, CreatedOn DATETIME NOT NULL )");
	}

	private ErrorList Process(Action action, string methodName)
	{
		ErrorList errorList = new ErrorList();
		if (Connection == null)
		{
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("Q998", "No database connection available.", "The initialization of the service may have failed; please reinitialize the Queue Service."));
			return errorList;
		}
		try
		{
			lock (Connection)
			{
				action();
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in " + methodName + ".", e);
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("Q999", "An unhandled exception was raised in QueueServiceSqliteData." + methodName + ".", e));
		}
		return errorList;
	}

	private static IMessage ToMessage(SqliteDataReader reader)
	{
		return new QueueMessage
		{
			ID = reader.GetInt32(0),
			Type = reader.GetString(1),
			Priority = (byte)reader.GetInt32(2),
			Data = (byte[])reader.GetValue(3),
			CreatedOn = reader.GetDateTime(4)
		};
	}
}
