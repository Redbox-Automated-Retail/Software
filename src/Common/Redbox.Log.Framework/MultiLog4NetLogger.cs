using System;
using System.IO;
using Redbox.Core;
using log4net;
using log4net.Config;

namespace Redbox.Log.Framework;

public class MultiLog4NetLogger : ILogger
{
	private readonly string m_loggerName;

	private ILog m_log;

	public string LoggerName => m_loggerName;

	private ILog LoggerInstance
	{
		get
		{
			if (m_log == null)
			{
				m_log = LogManager.GetLogger(m_loggerName);
			}
			return m_log;
		}
	}

	public MultiLog4NetLogger(string loggerName)
	{
		m_loggerName = loggerName;
	}

	public void Configure()
	{
		XmlConfigurator.Configure();
	}

	public void Configure(string path)
	{
		XmlConfigurator.Configure(new FileInfo(path));
	}

	public void ConfigureAndWatch(string path)
	{
		XmlConfigurator.ConfigureAndWatch(new FileInfo(path));
	}

	public void Log(string message, Exception e)
	{
		if (LoggerInstance.IsErrorEnabled)
		{
			LoggerInstance.Error(message, e);
		}
	}

	public void Log(string message, LogEntryType type)
	{
		switch (type)
		{
		case LogEntryType.Info:
			if (LoggerInstance.IsInfoEnabled)
			{
				LoggerInstance.Info(message);
			}
			break;
		case LogEntryType.Debug:
			if (LoggerInstance.IsDebugEnabled)
			{
				LoggerInstance.Debug(message);
			}
			break;
		case LogEntryType.Error:
			if (LoggerInstance.IsErrorEnabled)
			{
				LoggerInstance.Error(message);
			}
			break;
		case LogEntryType.Fatal:
			if (LoggerInstance.IsFatalEnabled)
			{
				LoggerInstance.Fatal(message);
			}
			break;
		}
	}

	public void Log(string message, params object[] parms)
	{
		if (LoggerInstance.IsInfoEnabled)
		{
			if (parms.Length != 0)
			{
				LoggerInstance.Info(string.Format(message, parms));
			}
			else
			{
				LoggerInstance.Info(message);
			}
		}
	}

	public void Log(string message, Exception e, LogEntryType type)
	{
		switch (type)
		{
		case LogEntryType.Info:
			if (LoggerInstance.IsInfoEnabled)
			{
				LoggerInstance.Info(message, e);
			}
			break;
		case LogEntryType.Debug:
			if (LoggerInstance.IsDebugEnabled)
			{
				LoggerInstance.Debug(message, e);
			}
			break;
		case LogEntryType.Error:
			if (LoggerInstance.IsErrorEnabled)
			{
				LoggerInstance.Error(message, e);
			}
			break;
		case LogEntryType.Fatal:
			if (LoggerInstance.IsFatalEnabled)
			{
				LoggerInstance.Fatal(message, e);
			}
			break;
		}
	}
}
