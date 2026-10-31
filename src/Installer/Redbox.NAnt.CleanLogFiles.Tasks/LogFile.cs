using System;
using System.IO;

namespace Redbox.NAnt.CleanLogFiles.Tasks;

public class LogFile
{
	private string _path;

	private DateTime _lastWriteTime;

	public string Path
	{
		get
		{
			return _path;
		}
		set
		{
			_path = value;
		}
	}

	public DateTime LastWriteTime
	{
		get
		{
			return _lastWriteTime;
		}
		set
		{
			_lastWriteTime = value;
		}
	}

	public LogFile(string path)
	{
		Path = path;
		LastWriteTime = new FileInfo(Path).LastWriteTime;
	}

	public override string ToString()
	{
		return Path;
	}

	public virtual void Delete()
	{
		File.Delete(_path);
	}

	public static implicit operator string(LogFile logFile)
	{
		return logFile.Path;
	}

	public static implicit operator LogFile(string logFileString)
	{
		return new LogFile(logFileString);
	}
}
