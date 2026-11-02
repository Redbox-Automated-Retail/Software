using System;
using NAnt.Core;
using NAnt.Core.Attributes;

namespace Redbox.Nant.Tasks;

[TaskName("svn.delete")]
public class SvnDelete : Task
{
	protected SvnExecTask _task;

	private string _workingDirectory = null!; // set by NAnt (required attribute)

	private string _targetsFile = null!; // set by NAnt (required attribute)

	[TaskAttribute("working-dir", Required = true)]
	public string WorkingDirectory
	{
		get
		{
			return _workingDirectory;
		}
		set
		{
			_workingDirectory = value;
		}
	}

	[TaskAttribute("targets-file", Required = true)]
	public string TargetsFile
	{
		get
		{
			return _targetsFile;
		}
		set
		{
			_targetsFile = value;
		}
	}

	public SvnDelete()
		: this(new SvnExecTask())
	{
	}

	public SvnDelete(SvnExecTask execTask)
	{
		_task = execTask;
	}

	protected override void ExecuteTask()
	{
		try
		{
			_task.AddArgument("delete");
			_task.AddArgument("--targets");
			_task.AddArgument(TargetsFile);
			_task.WorkingDirectory = WorkingDirectory;
			_task.Project = Project;
			_task.Execute();
		}
		catch (Exception ex)
		{
			Log(Level.Error, ex.ToString());
		}
	}
}
