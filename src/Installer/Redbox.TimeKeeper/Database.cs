using System.Data.SqlClient;

namespace Redbox.TimeKeeper;

public class Database : IDatabase
{
	private string? _connectionString = null;

	public string? ConnectionString => _connectionString;

	protected Database()
	{
	}

	public Database(string connectionString)
	{
		_connectionString = connectionString;
	}

	public long Modify(SqlCommand command)
	{
		InitializeCommand(command);
		try
		{
			return command.ExecuteNonQuery();
		}
		finally
		{
			command.Connection.Close();
			command.Dispose();
		}
	}

	private void InitializeCommand(SqlCommand command)
	{
		command.Connection = new SqlConnection(ConnectionString);
		command.Connection.Open();
		command.Dispose();
	}
}
