using System;

namespace Redbox.HAL.Component.Model;

public sealed class LogHelper
{
	private static LogHelper m_instance;

	public static LogHelper Instance => m_instance;

	public void Log(string msg)
	{
		Log(msg, LogEntryType.Info);
	}

	public void Log(string fmt, params object[] stuff)
	{
		ILogger service = ServiceLocator.Instance.GetService<ILogger>();
		if (service != null)
		{
			string message = string.Format(fmt, stuff);
			Log(message, LogEntryType.Info);
		}
	}

	public void Log(string message, Exception e)
	{
		ServiceLocator.Instance.GetService<ILogger>()?.Log(message, e);
	}

	public void Log(string message, LogEntryType type)
	{
		ServiceLocator.Instance.GetService<ILogger>()?.Log(message, type);
	}

	public void Log(string message, Exception e, LogEntryType type)
	{
		ServiceLocator.Instance.GetService<ILogger>()?.Log(message, e, type);
	}

	public void Log(LogEntryType type, string message, params object[] args)
	{
		ILogger service = ServiceLocator.Instance.GetService<ILogger>();
		if (service != null && IsLevelEnabled(type))
		{
			string message2 = string.Format(message, args);
			service.Log(message2, type);
		}
	}

	public void WithContext(string fmt, params object[] stuff)
	{
		WithContext(LogEntryType.Info, fmt, stuff);
	}

	public void WithContext(LogEntryType type, string msg)
	{
		WithContext(toFormattedLog: true, type, msg);
	}

	public void WithContext(LogEntryType type, string fmt, params object[] stuff)
	{
		WithContext(type, string.Format(fmt, stuff));
	}

	public void WithContext(bool toFormattedLog, LogEntryType type, string fmt, params object[] stuff)
	{
		WithContext(toFormattedLog, type, string.Format(fmt, stuff));
	}

	public void WithContext(bool toFormattedLog, LogEntryType type, string msg)
	{
		IExecutionService service = ServiceLocator.Instance.GetService<IExecutionService>();
		if (service == null)
		{
			Log(type, msg);
			return;
		}
		IExecutionContext activeContext = service.GetActiveContext();
		if (activeContext == null)
		{
			Log(type, msg);
			return;
		}
		Log(type, "[{0}, {1}]: {2}", activeContext.ProgramName, activeContext.ID, msg);
		if (toFormattedLog)
		{
			activeContext.ContextLog.WriteFormatted(msg);
		}
	}

	public bool IsLevelEnabled(LogEntryType logEntryLevel)
	{
		return ServiceLocator.Instance.GetService<ILogger>()?.IsLevelEnabled(logEntryLevel) ?? false;
	}

	private LogHelper()
	{
	}

	static LogHelper()
	{
		m_instance = new LogHelper();
	}
}
