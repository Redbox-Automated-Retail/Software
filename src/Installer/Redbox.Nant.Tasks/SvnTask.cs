using NAnt.Core;

namespace Redbox.Nant.Tasks;

public abstract class SvnTask : Task
{
	protected SvnExecTask _task = null!; // set by the derived task's constructor

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
