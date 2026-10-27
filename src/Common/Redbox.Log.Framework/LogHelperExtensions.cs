using System;
using Redbox.Core;

namespace Redbox.Log.Framework;

public static class LogHelperExtensions
{
	public static SimpleLog4NetLogger CreateLog4NetLogger(this LogHelper helper, Type type)
	{
		return new SimpleLog4NetLogger(type);
	}

	public static MultiLog4NetLogger CreateMultiLog4NetLogger(this LogHelper helper, string loggerName)
	{
		return new MultiLog4NetLogger(loggerName);
	}
}
