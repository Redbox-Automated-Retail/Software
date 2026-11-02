using System;
using NAnt.Core;
using NAnt.Core.Attributes;

namespace Redbox.Nant.Tasks;

[TaskName("svn.checkout")]
public class SvnCheckout : Task
{
	protected SvnExecTask _task;

	private string _remoteDirectory = null!; // set by NAnt (required attribute)

	private string _workingDirectory = null!; // set by NAnt (required attribute)

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

	public SvnCheckout()
		: this(new SvnExecTask())
	{
	}

	public SvnCheckout(SvnExecTask execTask)
	{
		_task = execTask;
	}

	protected override void ExecuteTask()
	{
		try
		{
			_task.AddArgument("checkout");
			_task.AddArgument(RemoteDirectory);
			_task.AddArgument(WorkingDirectory);
			_task.Project = Project;
			_task.Execute();
		}
		catch (Exception ex)
		{
			Log(Level.Error, ex.ToString());
		}
	}
}
