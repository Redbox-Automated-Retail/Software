using System;

namespace Redbox.HAL.Core;

public interface ILogger
{
	void Log(string message, Exception e);

	void Log(string message, LogEntryType type);

	void Log(string message, Exception e, LogEntryType type);

	bool IsLevelEnabled(LogEntryType entryLevel);
}
