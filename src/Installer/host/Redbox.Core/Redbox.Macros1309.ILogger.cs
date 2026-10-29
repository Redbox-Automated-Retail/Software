using System;

namespace Redbox.Core;

internal interface Redbox_002EMacros1309_002EILogger
{
	string LoggerName { get; }

	void Configure();

	void Configure(string path);

	void ConfigureAndWatch(string path);

	void Log(string message, Exception e);

	void Log(string message, Redbox_002EMacros1309_002ELogEntryType type);

	void Log(string message, params object[] parms);

	void Log(string message, Exception e, Redbox_002EMacros1309_002ELogEntryType type);
}
