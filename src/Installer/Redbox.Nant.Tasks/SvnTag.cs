using System;
using NAnt.Core;
using NAnt.Core.Attributes;

namespace Redbox.Nant.Tasks;

[TaskName("svn.tag")]
public class SvnTag : Task
{
	protected SvnExecTask _task;

	private string _repositoryUrl = null!; // set by NAnt (required attribute)

	private string _tagsUrl = null!; // set by NAnt (required attribute)

	private string _tagVersion = null!; // set by NAnt (required attribute)

	private string _tagStage = null!; // set by NAnt (required attribute)

	private string _comments = null!; // set by NAnt (required attribute)

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

	[TaskAttribute("tag-version", Required = true)]
	public string TagVersion
	{
		get
		{
			return _tagVersion;
		}
		set
		{
			_tagVersion = value;
		}
	}

	[TaskAttribute("tag-stage", Required = true)]
	public string TagStage
	{
		get
		{
			return _tagStage;
		}
		set
		{
			_tagStage = value;
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

	public SvnTag()
		: this(new SvnExecTask())
	{
	}

	public SvnTag(SvnExecTask execTask)
	{
		_task = execTask;
	}

	protected override void ExecuteTask()
	{
		try
		{
			string argValue = $"{TagsUrl}/{TagVersion}/{TagStage}";
			_task.AddArgument("copy");
			_task.AddArgument(RepositoryUrl);
			_task.AddArgument(argValue);
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
