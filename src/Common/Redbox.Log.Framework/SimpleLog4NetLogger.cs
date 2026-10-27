using System;
using System.IO;
using Redbox.Core;
using log4net;
using log4net.Config;

namespace Redbox.Log.Framework;

public class SimpleLog4NetLogger : ILogger
{
	private string m_loggerName;

	private ILog m_log;

	private readonly Type m_logType;

	public string LoggerName => m_loggerName;

	private ILog LoggerInstance
	{
		get
		{
			if (m_log == null)
			{
				m_log = LogManager.GetLogger(m_logType);
			}
			return m_log;
		}
	}

	public SimpleLog4NetLogger(Type type)
	{
		m_logType = type;
		m_loggerName = "SimpleLog4NetLogger";
	}

	public void Configure()
	{
		XmlConfigurator.Configure();
		LoggerInstance.Info("SimpleLog4NetLogger configured from: app.config.");
	}

	public void Configure(string path)
	{
		XmlConfigurator.Configure(new FileInfo(path));
		LoggerInstance.Info($"SimpleLog4NetLogger configured from: '{path}'.");
	}

	public void ConfigureAndWatch(string path)
	{
		XmlConfigurator.ConfigureAndWatch(new FileInfo(path));
		LoggerInstance.Info($"SimpleLog4NetLogger configured from: '{path}', file watcher is enabled.");
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
