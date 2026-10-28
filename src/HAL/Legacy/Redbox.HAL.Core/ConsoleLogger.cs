using System;

namespace Redbox.HAL.Core;

public sealed class ConsoleLogger : ILogger
{
	public void Log(string message, Exception e)
	{
		WriteFormatted(message);
		Console.WriteLine(e.Message);
		Console.WriteLine(e.StackTrace);
	}

	public void Log(string message, LogEntryType type)
	{
		WriteFormatted(message);
	}

	public void Log(string message, Exception e, LogEntryType type)
	{
		WriteFormatted(message);
		Console.WriteLine(e.Message);
		Console.WriteLine(e.StackTrace);
	}

	public bool IsLevelEnabled(LogEntryType entryLogLevel)
	{
		return true;
	}

	private void WriteFormatted(string msg)
	{
		DateTime now = DateTime.Now;
		msg = $"{now.ToShortDateString()} {now.ToShortTimeString()} {msg}";
		Console.WriteLine(msg);
	}
}
