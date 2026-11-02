using System;
using log4net;
using log4net.Config;

namespace Redbox.TimeKeeper;

public class RedboxLogger
{
	public enum Log4NetSetting
	{
		PaymentService,
		KioskService,
		RedboxService
	}

	private ILog _defaultLogger;

	public RedboxLogger()
	{
		_defaultLogger = LogManager.GetLogger("DefaultLogger");
	}

	public virtual void Write(string message)
	{
		Write(message, LoggingLevel.Default);
	}

	public virtual void Write(string message, LevelWrapper level)
	{
		Write(message, level, null);
	}

	public virtual void Write(string message, Exception ex)
	{
		Write(message, LoggingLevel.Default, ex);
	}

	private void Write(string message, LevelWrapper level, Exception? ex)
	{
		_defaultLogger.Logger.Log(typeof(RedboxLogger), level.Level, message, ex);
	}

	public static void Configure()
	{
		XmlConfigurator.Configure();
	}
}
