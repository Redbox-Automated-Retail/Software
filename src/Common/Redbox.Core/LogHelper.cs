using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

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
			StackFrame frame = new StackTrace(2).GetFrame(0);
			return frame.GetMethod().DeclaringType.FullName + "." + frame.GetMethod().Name;
		}
	}

	public event LoggedEventHandler Logged;

	public void LogTo(string loggerName, string message)
	{
		ILogger logger = Loggers.Find((ILogger each) => each.LoggerName == loggerName);
		if (logger != null)
		{
			logger.Log(message);
			try
			{
				OnLogged(message);
			}
			catch (Exception)
			{
			}
		}
	}

	public void LogTo(string loggerName, string message, Exception e)
	{
		ILogger logger = Loggers.Find((ILogger each) => each.LoggerName == loggerName);
		if (logger != null)
		{
			logger.Log(message, e);
			try
			{
				OnLogged(string.Format(message, e));
			}
			catch (Exception)
			{
				OnLogged(message + "\n" + e);
			}
		}
	}

	public void Log(string message, Exception e)
	{
		if (Logger == null)
		{
			return;
		}
		Logger.Log(message, e);
		try
		{
			OnLogged(string.Format(message, e));
		}
		catch (Exception)
		{
			OnLogged(message + "\n" + e);
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
		if (Logger == null)
		{
			return;
		}
		Logger.Log(message, parms);
		try
		{
			OnLogged(string.Format(message, parms));
		}
		catch (Exception)
		{
			OnLogged(message + "\n" + parms.ToJson());
		}
	}

	public void Log(string message, Exception e, params object[] parms)
	{
		if (Logger == null)
		{
			return;
		}
		Logger.Log(string.Format(message, parms), e);
		try
		{
			OnLogged(string.Format(string.Format(message, parms), e));
		}
		catch (Exception)
		{
			OnLogged(message + "\n" + parms.ToJson() + "\n" + e);
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

	public void LogFullException(string message, Exception e)
	{
		Type type = e.GetType();
		StringBuilder stringBuilder = new StringBuilder();
		while (e != null)
		{
			stringBuilder.Append(e.Message);
			e = e.InnerException;
		}
		Log($"{CallingFunction} - {message}, {type}, '{stringBuilder}'");
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

	public void OnLogged(string message)
	{
		if (Logged != null)
		{
			Logged(message);
		}
	}
}
