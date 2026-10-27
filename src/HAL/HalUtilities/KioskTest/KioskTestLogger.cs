using System;
using System.IO;
using Redbox.HAL.Component.Model;

namespace HALUtilities.KioskTest;

internal sealed class KioskTestLogger : ILogger, IDisposable
{
	private readonly StreamWriter LogFile;

	public void Log(string message, Exception e)
	{
		printAndLog(message);
		printAndLog(e.Message);
	}

	public void Log(string message, LogEntryType type)
	{
		printAndLog(message);
	}

	public void Log(string message, Exception e, LogEntryType type)
	{
		printAndLog(message);
		printAndLog(e.Message);
	}

	public bool IsLevelEnabled(LogEntryType entryLevel)
	{
		return true;
	}

	public void Dispose()
	{
		LogFile.Dispose();
	}

	internal KioskTestLogger()
	{
		IRuntimeService service = ServiceLocator.Instance.GetService<IRuntimeService>();
		string path = string.Format("kioskTest-{0}", service.GenerateUniqueFile(".log"));
		string text = "c:\\Program Files\\Redbox\\KioskLogs\\KioskTest";
		bool flag = true;
		if (!Directory.Exists(text))
		{
			try
			{
				Directory.CreateDirectory(text);
			}
			catch (Exception ex)
			{
				flag = false;
				Console.WriteLine("Failed to create kiosk logs path", ex.Message);
			}
		}
		if (flag)
		{
			string path2 = Path.Combine(text, path);
			LogFile = new StreamWriter(path2)
			{
				AutoFlush = true
			};
		}
		else
		{
			LogFile = StreamWriter.Null;
		}
	}

	private void printAndLog(string msg)
	{
		string value = $"{DateTime.Now} {msg}";
		Console.WriteLine(value);
		LogFile.WriteLine(value);
	}
}
