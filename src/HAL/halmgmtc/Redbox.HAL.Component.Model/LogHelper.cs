using System;

namespace Redbox.HAL.Component.Model;

public class LogHelper
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
