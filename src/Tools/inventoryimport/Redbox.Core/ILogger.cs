using System;

namespace Redbox.Core;

public interface ILogger
{
	void Configure();

	void Configure(string path);

	void ConfigureAndWatch(string path);

	void Log(string message, Exception e);

	void Log(string message, LogEntryType type);

	void Log(string message, params object[] parms);

	void Log(string message, Exception e, LogEntryType type);
}
