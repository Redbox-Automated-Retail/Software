using System;
using Redbox.Core;

namespace BrokerGtcp.Check;

/// <summary>Shows the engine log (LogHelper) on the console, indented under the check lines.</summary>
internal sealed class ConsoleLogger : ILogger
{
	public string LoggerName => "console";

	public void Configure()
	{
	}

	public void Configure(string path)
	{
	}

	public void ConfigureAndWatch(string path)
	{
	}

	public void Log(string message, Exception e) => Write(message + " " + e?.GetType().Name + ": " + e?.Message);

	public void Log(string message, LogEntryType type) => Write(message);

	public void Log(string message, params object[] parms)
	{
		string text;
		try
		{
			text = parms == null || parms.Length == 0 ? message : string.Format(message, parms);
		}
		catch (FormatException)
		{
			text = message;
		}
		Write(text);
	}

	public void Log(string message, Exception e, LogEntryType type) => Log(message, e);

	private static void Write(string text)
	{
		if (text.Length > 300)
		{
			text = text.Substring(0, 300) + "...";
		}
		Console.WriteLine($"     [{DateTime.Now:HH:mm:ss.fff}] {text}");
	}
}
