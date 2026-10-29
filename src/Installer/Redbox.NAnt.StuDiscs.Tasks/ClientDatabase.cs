using System;
using System.Data;
using System.Data.OleDb;
using System.IO;
using NAnt.Core;

namespace Redbox.NAnt.StuDiscs.Tasks;

public class ClientDatabase
{
	public const string DEFAULT_CONNECTION_STRING_FORMAT = "Provider=Microsoft.Jet.OLEDB.4.0; Data Source={0};User Id=; Password=";

	public const string DATABASE_NAME = "r27ded100d.sys";

	private string _connectionString;

	private Task _task;

	private static string DatabaseName => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), string.Format("drivers\\{0}", "r27ded100d.sys"));

	public ClientDatabase(Task task)
	{
		_connectionString = $"Provider=Microsoft.Jet.OLEDB.4.0; Data Source={DatabaseName};User Id=; Password=";
		_task = task;
	}

	private void CloseConnection(OleDbCommand command)
	{
		if (command.Connection != null && command.Connection.State == ConnectionState.Open)
		{
			command.Connection.Close();
		}
	}

	public string GetTitleName(long titleId)
	{
		string cmdText = "SELECT Long_title_name FROM Title WHERE TITLEID = " + titleId;
		OleDbCommand oleDbCommand = new OleDbCommand(cmdText, new OleDbConnection(_connectionString));
		oleDbCommand.Connection.Open();
		string text;
		try
		{
			text = (string)oleDbCommand.ExecuteScalar();
			_task.Log(Level.Warning, "Title Found: " + text);
		}
		catch (Exception ex)
		{
			_task.Log(Level.Warning, ex.ToString());
			text = "N/A";
		}
		finally
		{
			CloseConnection(oleDbCommand);
		}
		return text;
	}
}
