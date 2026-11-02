using System;
using NAnt.Core;
using NAnt.Core.Attributes;

namespace Redbox.Nant.Tasks;

[TaskName("svn.revert")]
public class SvnRevert : Task
{
	protected SvnExecTask _task;

	private string _workingDirectory = null!; // set by NAnt (required attribute)

	private string _ignoreList = null!; // set by NAnt (required attribute)

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

	[TaskAttribute("ignore-list", Required = true)]
	public string IgnoreList
	{
		get
		{
			return _ignoreList;
		}
		set
		{
			_ignoreList = value;
		}
	}

	public SvnRevert()
		: this(new SvnExecTask())
	{
	}

	public SvnRevert(SvnExecTask execTask)
	{
		_task = execTask;
	}

	protected override void ExecuteTask()
	{
		try
		{
			_task.AddArgument("revert");
			_task.AddArgument("-R");
			_task.AddArgument("--targets");
			_task.AddArgument(IgnoreList);
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
