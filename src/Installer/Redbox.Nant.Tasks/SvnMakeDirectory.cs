using System;
using NAnt.Core;
using NAnt.Core.Attributes;

namespace Redbox.Nant.Tasks;

[TaskName("svn.mkdir")]
public class SvnMakeDirectory : Task
{
	private string _remoteDirectory = null!; // set by NAnt (required attribute)

	private string _comments = null!; // set by NAnt (required attribute)

	protected SvnExecTask _task;

	[TaskAttribute("remote-dir", Required = true)]
	public string RemoteDirectory
	{
		get
		{
			return _remoteDirectory;
		}
		set
		{
			_remoteDirectory = value;
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

	public SvnMakeDirectory()
		: this(new SvnExecTask())
	{
	}

	public SvnMakeDirectory(SvnExecTask execTask)
	{
		_task = execTask;
	}

	protected override void ExecuteTask()
	{
		try
		{
			_task.AddArgument("mkdir");
			_task.AddArgument(RemoteDirectory);
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
