using System;
using System.Collections;
using NAnt.Core;
using Redbox.NAnt.Core;

namespace Redbox.NAnt.CleanLogFiles.Tasks;

public class LogFileList
{
	private ArrayList _list = new ArrayList();

	private ITaskLogger _logger;

	private int _daysOld;

	public ArrayList Files => _list;

	public LogFileList(ICollection logfiles, int daysOld, ITaskLogger logger)
	{
		_daysOld = daysOld;
		_logger = logger;
		_logger.Log(Level.Info, "Today's date: {0}; removing log files older than {1} days:", DateTime.Now, _daysOld);
		AddFiles(logfiles);
	}

	public virtual void AddFiles(ICollection files)
	{
		foreach (string file in files)
		{
			Files.Add(new LogFile(file));
		}
	}

	public virtual void Delete()
	{
		TimeSpan timeSpan = new TimeSpan(_daysOld, 0, 0, 0);
		foreach (LogFile file in Files)
		{
			if (DateTime.Now - file.LastWriteTime >= timeSpan)
			{
				try
				{
					file.Delete();
					_logger.Log(Level.Info, file.ToString());
				}
				catch (Exception ex)
				{
					_logger.Log(Level.Error, ex.ToString());
				}
			}
		}
	}
}
