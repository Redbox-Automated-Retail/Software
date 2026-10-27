using System;
using System.IO;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.CameraTuner;

public sealed class TunerLog : ILogger
{
	private bool Disposed;

	private TextWriter m_logFile;

	private const string LogFileName = "CameraTuner.log";

	private const string LogDirectory = "c:\\Program Files\\Redbox\\KioskLogs\\Service";

	private TextWriter LogFile
	{
		get
		{
			if (m_logFile == null)
			{
				string path = Path.Combine("c:\\Program Files\\Redbox\\KioskLogs\\Service", "CameraTuner.log");
				m_logFile = new StreamWriter(File.Open(path, FileMode.Append, FileAccess.Write, FileShare.Read));
			}
			return m_logFile;
		}
	}

	public void Log(string message, Exception e)
	{
		WriteToLogFile(message);
		WriteToLogFile(e.Message);
		WriteToLogFile(e.StackTrace);
	}

	public void Log(string message, LogEntryType type)
	{
		WriteToLogFile(message);
	}

	public void Log(string message, Exception e, LogEntryType type)
	{
		WriteToLogFile(message);
		WriteToLogFile(e.Message);
		WriteToLogFile(e.StackTrace);
	}

	public bool IsLevelEnabled(LogEntryType entryLogLevel)
	{
		return true;
	}

	public void Dispose()
	{
		if (Disposed)
		{
			return;
		}
		Disposed = true;
		try
		{
			if (m_logFile != null)
			{
				m_logFile.Flush();
				m_logFile.Close();
			}
		}
		catch
		{
		}
		GC.SuppressFinalize(this);
	}

	private void WriteToLogFile(string msg)
	{
		try
		{
			string value = $"{DateTime.Now}: {msg}";
			LogFile.WriteLine(value);
			LogFile.Flush();
		}
		catch (Exception)
		{
			Console.WriteLine("Write to log file caught an exception.");
		}
	}
}
