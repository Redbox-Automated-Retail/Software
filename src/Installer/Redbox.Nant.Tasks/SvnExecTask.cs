using System.IO;
using NAnt.Core;
using NAnt.Core.Tasks;
using NAnt.Core.Types;

namespace Redbox.Nant.Tasks;

public class SvnExecTask
{
	public const string SVN_EXECUTABLE = "svn";

	private ExecTask _task;

	public virtual string FileName
	{
		get
		{
			return _task.FileName;
		}
		set
		{
			_task.FileName = value;
		}
	}

	public virtual ArgumentCollection Arguments => _task.Arguments;

	public virtual Project Project
	{
		get
		{
			return _task.Project;
		}
		set
		{
			_task.Project = value;
		}
	}

	public string WorkingDirectory
	{
		get
		{
			return _task.WorkingDirectory.FullName;
		}
		set
		{
			_task.WorkingDirectory = new DirectoryInfo(value);
		}
	}

	public SvnExecTask()
	{
		_task = new ExecTask();
		_task.FileName = "svn";
	}

	public virtual void Execute()
	{
		_task.Execute();
	}

	public void AddArgument(string argValue)
	{
		Arguments.Add(new Argument(argValue));
	}
}
