using System;
using Redbox.Core;

namespace Redbox.Extensibility.ComponentModel;

public interface ILoggingService
{
	void Log(string message);

	void Log(string message, Exception e);

	void Log(string message, LogEntryType type);

	void Log(string message, Exception e, LogEntryType type);

	void Log(string format, params object[] parms);

	void Log(LogEntryType type, string format, params object[] parms);
}
