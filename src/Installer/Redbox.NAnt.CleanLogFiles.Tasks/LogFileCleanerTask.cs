using NAnt.Core.Attributes;
using NAnt.Core.Types;
using Redbox.NAnt.Core;

namespace Redbox.NAnt.CleanLogFiles.Tasks;

[TaskName("logfile-cleaner")]
public class LogFileCleanerTask : TaskBase
{
	private int _daysOld;

	private FileSet _pattern;

	[BuildElement("pattern")]
	public FileSet Pattern
	{
		get
		{
			return _pattern;
		}
		set
		{
			_pattern = value;
		}
	}

	[TaskAttribute("days.old", Required = true)]
	public int DaysOld
	{
		get
		{
			return _daysOld;
		}
		set
		{
			_daysOld = value;
		}
	}

	protected override void ExecuteTask()
	{
		new LogFileList(_pattern.FileNames, _daysOld, this).Delete();
	}
}
