using System;
using System.Diagnostics;

namespace Redbox.Core;

public class LogHelper
{
	public static LogHelper Instance => Singleton<LogHelper>.Instance;

	public ILogger Logger { get; set; }

	internal static string CallingFunction
	{
		get
		{
			StackTrace stackTrace = new StackTrace(2);
			StackFrame frame = stackTrace.GetFrame(0);
			return frame.GetMethod().DeclaringType.FullName + "." + frame.GetMethod().Name;
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
