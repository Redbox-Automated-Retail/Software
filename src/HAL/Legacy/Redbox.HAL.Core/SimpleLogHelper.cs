using System;
using System.IO;

namespace Redbox.HAL.Core;

public sealed class SimpleLogHelper : ILogger, IDisposable
{
	private TextWriter m_logFile;

	internal TextWriter LogFile
	{
		get
		{
			if (m_logFile == null)
			{
				string path = Path.Combine(LogPath, LogFileName);
				m_logFile = new StreamWriter(File.Open(path, FileMode.Append, FileAccess.Write, FileShare.Read))
				{
					AutoFlush = true
				};
			}
			return m_logFile;
		}
	}

	private bool Disposed { get; set; }

	private string LogFileName { get; set; }

	private string LogPath { get; set; }

	private string FullPath { get; set; }

	public SimpleLogHelper(string fileName)
	{
		LogFileName = fileName;
		LogPath = RuntimeService.Instance.AssemblyDirectory;
		FullPath = Path.Combine(LogPath, LogFileName);
	}

	public SimpleLogHelper(string fileName, bool clear)
	{
		LogFileName = fileName;
		LogPath = RuntimeService.Instance.AssemblyDirectory;
		FullPath = Path.Combine(LogPath, LogFileName);
		if (clear)
		{
			DeleteFile(FullPath);
		}
	}

	public SimpleLogHelper(string path, string fileName)
	{
		LogFileName = fileName;
		LogPath = path;
		FullPath = Path.Combine(LogPath, LogFileName);
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

	public void Log(string message, Exception e)
	{
		WriteFormatted(message);
		WriteFormatted(e.Message);
	}

	public void Log(string message, LogEntryType type)
	{
		WriteFormatted(message);
	}

	public void Log(string message, Exception e, LogEntryType type)
	{
		WriteFormatted(message);
		WriteFormatted(e.Message);
	}

	public bool IsLevelEnabled(LogEntryType entryLogLevel)
	{
		return true;
	}

	internal void Clear()
	{
		DeleteFile(FullPath);
	}

	internal void WriteLine(string msg)
	{
		LogFile.WriteLine(msg);
	}

	internal void WriteLine(string msg, params object[] stuff)
	{
		LogFile.WriteLine(string.Format(msg, stuff));
	}

	private bool DeleteFile(string file)
	{
		if (!File.Exists(file))
		{
			return false;
		}
		try
		{
			File.Delete(file);
			return true;
		}
		catch
		{
			return false;
		}
	}

	private void WriteFormatted(string msg)
	{
		LogFile.WriteLine("{0}: {1}", DateTime.Now, msg);
	}
}
