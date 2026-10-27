using System;

namespace Redbox.Core;

public class LogHelper
{
	public static LogHelper Instance => Singleton<LogHelper>.Instance;

	public ILogger Logger { get; set; }

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

	public void Log(string message, params object[] parms)
	{
		if (Logger != null)
		{
			Logger.Log(message, parms);
		}
	}

	public void Log(string message, Exception e, params object[] parms)
	{
		if (Logger != null)
		{
			Logger.Log(string.Format(message, parms), e);
		}
	}

	public void Log(string message, Exception e, LogEntryType type)
	{
		if (Logger != null)
		{
			Logger.Log(message, e, type);
		}
	}

	private LogHelper()
	{
	}
}
