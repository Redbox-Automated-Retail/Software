using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;

namespace Redbox.TimeKeeper;

public abstract class DatabaseCommand : IDisposable
{
	private SqlCommand _command = null!; // set by the protected constructors

	private SqlCommand? _invalidCommand;

	protected SqlCommand Command => _command;

	public string CommandText
	{
		get
		{
			return _command.CommandText;
		}
		set
		{
			_command.CommandText = value;
		}
	}

	public int CommandTimeout
	{
		get
		{
			return _command.CommandTimeout;
		}
		set
		{
			_command.CommandTimeout = value;
		}
	}

	public CommandType CommandType => _command.CommandType;

	public SqlParameterCollection Parameters => _command.Parameters;

	private DatabaseCommand()
	{
	}

	protected DatabaseCommand(string query, string[] parameterNames, params object[] parameters)
		: this(query)
	{
		for (int i = 0; i < parameters.Length; i++)
		{
			Command.Parameters.Add(parameterNames[i], parameters[i]);
		}
	}

	protected DatabaseCommand(string query)
	{
		_command = new SqlCommand(query);
		Command.CommandType = CommandType.StoredProcedure;
	}

	private void AddParameter(SqlParameter parameter)
	{
		Command.Parameters.Add(parameter);
	}

	public void AddParameters(SqlParameter[] parameters)
	{
		Command.Parameters.Clear();
		for (int i = 0; i < parameters.Length; i++)
		{
			AddParameter(parameters[i]);
		}
	}

	public void AddParameters(Hashtable parameters)
	{
		Command.Parameters.Clear();
		foreach (object parameter in parameters)
		{
			DictionaryEntry dictionaryEntry = (DictionaryEntry)parameter;
			AddParameter(new SqlParameter(dictionaryEntry.Key.ToString(), dictionaryEntry.Value));
		}
	}

	protected void AddParameter(string parameterName, object parameterValue)
	{
		AddParameter(new SqlParameter(parameterName, parameterValue));
	}

	public static implicit operator SqlCommand(DatabaseCommand d)
	{
		return d.Command;
	}

	public static bool operator ==(object one, DatabaseCommand two)
	{
		return two.Equals(one);
	}

	public static bool operator !=(object one, DatabaseCommand two)
	{
		return !two.Equals(one);
	}

	public static bool operator ==(DatabaseCommand one, object two)
	{
		return one.Equals(two);
	}

	public static bool operator !=(DatabaseCommand one, object two)
	{
		return !one.Equals(two);
	}

	public override bool Equals(object? obj)
	{
		if (obj is SqlCommand)
		{
			return CompareCommand((obj as SqlCommand)!);
		}
		if (!obj!.GetType().Equals(GetType())) // a null obj threw here before too
		{
			return false;
		}
		DatabaseCommand databaseCommand = (obj as DatabaseCommand)!; // same type as this
		return CompareCommand(databaseCommand.Command);
	}

	private bool CompareCommand(SqlCommand command)
	{
		_invalidCommand = command;
		if (command.CommandText != Command.CommandText)
		{
			return false;
		}
		if (command.CommandTimeout != Command.CommandTimeout)
		{
			return false;
		}
		if (command.Parameters.Count != Command.Parameters.Count)
		{
			return false;
		}
		foreach (IDbDataParameter parameter in Command.Parameters)
		{
			if (!command.Parameters.Contains(parameter.ParameterName))
			{
				return false;
			}
			if (command.Parameters[parameter.ParameterName].Value.ToString() != parameter.Value.ToString())
			{
				return false;
			}
			if (command.Parameters[parameter.ParameterName].Direction != parameter.Direction)
			{
				return false;
			}
		}
		_invalidCommand = null;
		return true;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public override string ToString()
	{
		string parameters = string.Empty;
		string invalidParameters = string.Empty;
		foreach (SqlParameter parameter in Parameters)
		{
			parameters += $"{parameter.ParameterName}={parameter.Value}, ";
		}
		if (_invalidCommand != null)
		{
			foreach (SqlParameter parameter2 in _invalidCommand.Parameters)
			{
				invalidParameters += $"{parameter2.ParameterName}={parameter2.Value}, ";
			}
		}
		string command = $"\n{CommandText}:{parameters}";
		string invalidCommand = $"\n{_invalidCommand!.CommandText}:{invalidParameters}"; // throws when there is none, as before
		return command + invalidCommand;
	}

	public void Dispose()
	{
		if (_command != null)
		{
			_command.Dispose();
		}
	}
}
