using System;
using System.IO;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using VistaDB.DDA;
using VistaDB.Provider;

namespace Redbox.KioskEngine.Environment;

public class QueueServiceVistaDBData : IQueueServiceData, IDisposable
{
	internal VistaDBConnection Connection { get; set; }

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
			DatabasePath = path;
			if (File.Exists(DatabasePath))
			{
				LogHelper.Instance.Log("> Pack message database...");
				using IVistaDBDDA vistaDBDDA = VistaDBEngine.Connections.OpenDDA();
				vistaDBDDA.PackDatabase(DatabasePath, null, backup: false, null);
			}
			SchemaMigrator.Migrate(DatabasePath);
			Connection = new VistaDBConnection($"Data Source={DatabasePath};Open Mode=ExclusiveReadWrite;");
			Connection.Open();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in QueueServiceVistDBData.Initialize.", e);
			try
			{
				string text = DatabasePath + ".corrupt";
				LogHelper.Instance.Log("Attempt to move '{0}' to '{1}'.", DatabasePath, text);
				File.Move(DatabasePath, text);
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
		return ProcessVistaDBCommand(delegate
		{
			using VistaDBCommand vistaDBCommand = new VistaDBCommand();
			vistaDBCommand.Connection = Connection;
			vistaDBCommand.CommandText = "DELETE FROM Queue";
			vistaDBCommand.ExecuteNonQuery();
		}, "Clear");
	}

	public ErrorList GetDepth(out int count)
	{
		int tempCount = 0;
		ErrorList result = ProcessVistaDBCommand(delegate
		{
			using VistaDBCommand vistaDBCommand = new VistaDBCommand();
			vistaDBCommand.Connection = Connection;
			vistaDBCommand.CommandText = "SELECT COUNT(*) FROM Queue";
			tempCount = (int)vistaDBCommand.ExecuteScalar();
		}, "GetDepth");
		count = tempCount;
		return result;
	}

	public ErrorList DeleteMessage(int id)
	{
		return ProcessVistaDBCommand(delegate
		{
			using VistaDBCommand vistaDBCommand = new VistaDBCommand();
			vistaDBCommand.Connection = Connection;
			vistaDBCommand.CommandText = "DELETE FROM Queue WHERE ID = @ID";
			vistaDBCommand.Parameters.Add("@ID", id);
			vistaDBCommand.ExecuteNonQuery();
		}, "DeleteMessage");
	}

	public ErrorList DeleteMessageByType(string type)
	{
		return ProcessVistaDBCommand(delegate
		{
			using VistaDBCommand vistaDBCommand = new VistaDBCommand();
			vistaDBCommand.Connection = Connection;
			vistaDBCommand.CommandText = "DELETE FROM Queue WHERE Type = @Type";
			vistaDBCommand.Parameters.Add("@Type", type);
			vistaDBCommand.ExecuteNonQuery();
		}, "DeleteMessageByType");
	}

	public ErrorList Enqueue(byte priority, string type, byte[] encryptedMessage)
	{
		return ProcessVistaDBCommand(delegate
		{
			using VistaDBCommand vistaDBCommand = new VistaDBCommand();
			vistaDBCommand.Connection = Connection;
			vistaDBCommand.CommandText = "INSERT INTO Queue( Priority, Type, Data ) VALUES( @Priority, @Type, @Data )";
			vistaDBCommand.Parameters.Add("@Type", type);
			vistaDBCommand.Parameters.Add("@Priority", priority);
			vistaDBCommand.Parameters.Add("@Data", encryptedMessage);
			vistaDBCommand.ExecuteNonQuery();
		}, "Enqueue");
	}

	public ErrorList Enqueue(byte priority, string type, byte[] encryptedMessage, out int newMessageId)
	{
		newMessageId = 0;
		int tempMessageId = 0;
		ErrorList result = ProcessVistaDBCommand(delegate
		{
			using VistaDBCommand vistaDBCommand = new VistaDBCommand();
			vistaDBCommand.Connection = Connection;
			vistaDBCommand.CommandText = "INSERT INTO Queue( Priority, Type, Data ) VALUES( @Priority, @Type, @Data ); SELECT @@IDENTITY;";
			vistaDBCommand.Parameters.Add("@Type", type);
			vistaDBCommand.Parameters.Add("@Priority", priority);
			vistaDBCommand.Parameters.Add("@Data", encryptedMessage);
			tempMessageId = Convert.ToInt32(vistaDBCommand.ExecuteScalar());
		}, "Enqueue");
		newMessageId = tempMessageId;
		return result;
	}

	public ErrorList Update(int id, byte[] message)
	{
		return ProcessVistaDBCommand(delegate
		{
			using VistaDBCommand vistaDBCommand = new VistaDBCommand();
			vistaDBCommand.Connection = Connection;
			vistaDBCommand.CommandText = "UPDATE Queue SET Data = @Data WHERE ID = @Id";
			vistaDBCommand.Parameters.Add("@Id", id);
			vistaDBCommand.Parameters.Add("@Data", message);
			vistaDBCommand.ExecuteNonQuery();
		}, "Update");
	}

	public ErrorList ForEach(Action<IMessage> action)
	{
		return ProcessVistaDBCommand(delegate
		{
			using VistaDBCommand vistaDBCommand = new VistaDBCommand();
			vistaDBCommand.Connection = Connection;
			vistaDBCommand.CommandText = "SELECT * FROM Queue ORDER BY Priority DESC, ID ASC";
			using VistaDBDataReader vistaDBDataReader = vistaDBCommand.ExecuteReader();
			while (vistaDBDataReader.Read())
			{
				action(ToMessage(vistaDBDataReader));
			}
		}, "ForEach");
	}

	public ErrorList ForEach(Action<IMessage> action, string type)
	{
		return ProcessVistaDBCommand(delegate
		{
			using VistaDBCommand vistaDBCommand = new VistaDBCommand();
			vistaDBCommand.Connection = Connection;
			vistaDBCommand.CommandText = "SELECT * FROM Queue WHERE Type = @Type ORDER BY Priority DESC, ID ASC";
			vistaDBCommand.Parameters.Add("@Type", type);
			using VistaDBDataReader vistaDBDataReader = vistaDBCommand.ExecuteReader();
			while (vistaDBDataReader.Read())
			{
				action(ToMessage(vistaDBDataReader));
			}
		}, "ForEach");
	}

	public ErrorList SelectNextMessage(IQueueServicePriority queueServicePriority, out IMessage message)
	{
		IMessage resultMessage = null;
		ErrorList result = ProcessVistaDBCommand(delegate
		{
			using VistaDBCommand vistaDBCommand = new VistaDBCommand();
			vistaDBCommand.Connection = Connection;
			vistaDBCommand.CommandText = "SELECT TOP 1 ID, Type, Priority, Data, CreatedOn FROM Queue WHERE Priority >= @PriorityValue ORDER BY Priority DESC, ID ASC;";
			vistaDBCommand.Parameters.Add("@PriorityValue", queueServicePriority?.MinimumPriorityValue ?? 0);
			using VistaDBDataReader vistaDBDataReader = vistaDBCommand.ExecuteReader();
			if (vistaDBDataReader.Read())
			{
				resultMessage = ToMessage(vistaDBDataReader);
			}
		}, "SelectNextMessage");
		message = resultMessage;
		return result;
	}

	public void Dispose()
	{
		if (Connection != null)
		{
			Connection.Dispose();
		}
	}

	private ErrorList ProcessVistaDBCommand(Action action, string methodName)
	{
		ErrorList errorList = new ErrorList();
		if (IsConnectionValid(errorList))
		{
			try
			{
				action();
			}
			catch (Exception e)
			{
				LogExceptionAndAddToErrors(errorList, methodName, e);
			}
		}
		return errorList;
	}

	private void LogExceptionAndAddToErrors(ErrorList errors, string exceptionSourceMethodName, Exception e)
	{
		LogHelper.Instance.Log("An unhandled exception was raised in " + exceptionSourceMethodName + ".", e);
		errors.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("Q999", "An unhandled exception was raised in QueueServiceVistaDBData." + exceptionSourceMethodName + ".", e));
	}

	private bool IsConnectionValid(ErrorList errors)
	{
		bool result = true;
		if (Connection == null)
		{
			errors.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("Q998", "No database connection available.", "The initialization of the service may have failed; please reinitialize the Queue Service."));
			result = false;
		}
		return result;
	}

	private IMessage ToMessage(VistaDBDataReader reader)
	{
		return new QueueMessage
		{
			ID = (int)reader["ID"],
			Type = (string)reader["Type"],
			CreatedOn = (DateTime)reader["CreatedOn"],
			Data = (byte[])reader["Data"],
			Priority = (byte)reader["Priority"]
		};
	}
}
