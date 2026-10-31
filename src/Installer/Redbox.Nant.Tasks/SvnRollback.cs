using System;
using NAnt.Core;
using NAnt.Core.Attributes;

namespace Redbox.Nant.Tasks;

[TaskName("svn.rollback")]
public class SvnRollback : Task
{
	protected SvnExecTask _task;

	private string _workingDirectory;

	private string _repositoryUrl;

	private string _tagsUrl;

	private string _targetVersion;

	private string _targetStage;

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

	[TaskAttribute("repository-url", Required = true)]
	public string RepositoryUrl
	{
		get
		{
			return _repositoryUrl;
		}
		set
		{
			_repositoryUrl = value;
		}
	}

	[TaskAttribute("tags-url", Required = true)]
	public string TagsUrl
	{
		get
		{
			return _tagsUrl;
		}
		set
		{
			_tagsUrl = value;
		}
	}

	[TaskAttribute("target-version", Required = true)]
	public string TargetVersion
	{
		get
		{
			return _targetVersion;
		}
		set
		{
			_targetVersion = value;
		}
	}

	[TaskAttribute("target-stage", Required = true)]
	public string TargetStage
	{
		get
		{
			return _targetStage;
		}
		set
		{
			_targetStage = value;
		}
	}

	public SvnRollback()
		: this(new SvnExecTask())
	{
	}

	public SvnRollback(SvnExecTask execTask)
	{
		_task = execTask;
	}

	protected override void ExecuteTask()
	{
		try
		{
			_task.AddArgument("merge");
			_task.AddArgument(RepositoryUrl);
			string argValue = $"{TagsUrl}/{TargetVersion}/{TargetStage}";
			_task.AddArgument(argValue);
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
