using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Redbox.Core;

public class LogHelper
{
	private List<ILogger> m_loggers;

	public static LogHelper Instance => Singleton<LogHelper>.Instance;

	public List<ILogger> Loggers
	{
		get
		{
			if (m_loggers == null)
			{
				m_loggers = new List<ILogger>();
			}
			return m_loggers;
		}
		set
		{
			m_loggers = value;
		}
	}

	public ILogger Logger
	{
		get
		{
			if (Loggers == null || Loggers.Count <= 0)
			{
				return null;
			}
			return Loggers[0];
		}
		set
		{
			Loggers.Add(value);
		}
	}

	internal static string CallingFunction
	{
		get
		{
			StackTrace stackTrace = new StackTrace(2);
			StackFrame frame = stackTrace.GetFrame(0);
			return frame.GetMethod().DeclaringType.FullName + "." + frame.GetMethod().Name;
		}
	}

	public void LogTo(string loggerName, string message)
	{
		Loggers.Find((ILogger each) => each.LoggerName == loggerName)?.Log(message);
	}

	public void LogTo(string loggerName, string message, Exception e)
	{
		Loggers.Find((ILogger each) => each.LoggerName == loggerName)?.Log(message, e);
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

	public void LogStart()
	{
		Log("{0} - start...", CallingFunction);
	}

	public void LogFinish()
	{
		Log("{0} - finish...", CallingFunction);
	}

	public void LogFunctionStep(string mask, params object[] parms)
	{
		Log($"{CallingFunction} - {string.Format(mask, parms)}");
	}

	public void LogFailure(string operation, string reason)
	{
		Log($"{operation}: Fail, Reason: {reason}");
	}

	public void LogError(string code, string description)
	{
		Log($"{CallingFunction} - error: {code}; '{description}'");
	}

	public void LogException(string message, Exception e)
	{
		Log($"{CallingFunction} - {message}, {e.GetType()}, '{e.Message}'");
	}

	internal void LogError(string callingfunction, string code, string description)
	{
		Log($"{callingfunction} - error: {code}; '{description}'");
	}

	internal void LogException(string callingfunction, string message, Exception e)
	{
		Log($"{callingfunction} - {message}, {e.GetType()}, '{e.Message}'");
	}

	private LogHelper()
	{
	}
}
