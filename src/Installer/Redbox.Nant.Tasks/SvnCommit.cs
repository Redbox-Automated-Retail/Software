using System;
using NAnt.Core;
using NAnt.Core.Attributes;

namespace Redbox.Nant.Tasks;

[TaskName("svn.rollback")]
public class SvnCommit : Task
{
	protected SvnExecTask _task;

	private string _directory;

	private string _comments;

	[TaskAttribute("dir", Required = true)]
	public string Directory
	{
		get
		{
			return _directory;
		}
		set
		{
			_directory = value;
		}
	}

	[TaskAttribute("comments", Required = true)]
	public string Comments
	{
		get
		{
			return _comments;
		}
		set
		{
			_comments = value;
		}
	}

	public SvnCommit()
		: this(new SvnExecTask())
	{
	}

	public SvnCommit(SvnExecTask execTask)
	{
		_task = execTask;
	}

	protected override void ExecuteTask()
	{
		try
		{
			_task.AddArgument("commit");
			_task.AddArgument(Directory);
			_task.AddArgument("-m");
			_task.AddArgument(Comments);
			_task.Project = Project;
			_task.Execute();
		}
		catch (Exception ex)
		{
			Log(Level.Error, ex.ToString());
		}
	}
}
