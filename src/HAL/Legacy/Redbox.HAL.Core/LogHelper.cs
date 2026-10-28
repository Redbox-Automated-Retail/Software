using System;

namespace Redbox.HAL.Core;

public class LogHelper
{
	public static LogHelper Instance => Singleton<LogHelper>.Instance;

	public ILogger Logger { get; set; }

	public void Log(string msg)
	{
		Log(msg, LogEntryType.Info);
	}

	public void Log(string fmt, params object[] stuff)
	{
		if (Logger != null)
		{
			string message = string.Format(fmt, stuff);
			Log(message, LogEntryType.Info);
		}
	}

	public void Log(string message, Exception e)
	{
		if (Logger != null)
		{
			Logger.Log(message, e);
		}
	}

	public void Log(string message, LogEntryType type)
	{
		if (Logger != null)
		{
			Logger.Log(message, type);
		}
	}

	public void Log(string message, Exception e, LogEntryType type)
	{
		if (Logger != null)
		{
			Logger.Log(message, e, type);
		}
	}

	public void Log(LogEntryType type, string message, params object[] args)
	{
		if (Logger != null && IsLevelEnabled(type))
		{
			string message2 = string.Format(message, args);
			Logger.Log(message2, type);
		}
	}

	public bool IsLevelEnabled(LogEntryType logEntryLevel)
	{
		if (Logger == null)
		{
			return false;
		}
		return Logger.IsLevelEnabled(logEntryLevel);
	}

	private LogHelper()
	{
	}
}
