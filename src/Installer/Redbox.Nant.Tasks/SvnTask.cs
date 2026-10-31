using NAnt.Core;

namespace Redbox.Nant.Tasks;

public abstract class SvnTask : Task
{
	protected SvnExecTask _task;

	public SvnTask()
	{
	}

	public SvnTask(SvnExecTask task)
	{
		_task = task;
	}

	protected override void ExecuteTask()
	{
		_task.Project = Project;
		_task.Execute();
	}
}
