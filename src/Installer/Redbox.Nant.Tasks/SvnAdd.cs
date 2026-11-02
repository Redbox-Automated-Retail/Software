using System;
using NAnt.Core;
using NAnt.Core.Attributes;

namespace Redbox.Nant.Tasks;

[TaskName("svn.add")]
public class SvnAdd : Task
{
	protected SvnExecTask _task;

	private string _workingDirectory = null!; // set by NAnt (required attribute)

	private string? _targetsFile;

	private string? _filesToAdd;

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

	[TaskAttribute("targets-file", Required = false)]
	public string? TargetsFile
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

	[TaskAttribute("files-to-add", Required = false)]
	public string? FilesToAdd
	{
		get
		{
			return _filesToAdd;
		}
		set
		{
			_filesToAdd = value;
		}
	}

	public SvnAdd()
		: this(new SvnExecTask())
	{
	}

	public SvnAdd(SvnExecTask execTask)
	{
		_task = execTask;
	}

	protected override void ExecuteTask()
	{
		try
		{
			_task.AddArgument("add");
			if (FilesToAdd != null)
			{
				_task.AddArgument(FilesToAdd);
			}
			if (TargetsFile != null)
			{
				_task.AddArgument("--targets");
				_task.AddArgument(TargetsFile);
			}
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
