namespace TaskScheduler;

public class Scheduler
{
	private readonly TaskList tasks;

	public string TargetComputer
	{
		get
		{
			return tasks.TargetComputer;
		}
		set
		{
			tasks.TargetComputer = value;
		}
	}

	public TaskList Tasks => tasks;

	public Scheduler()
	{
		tasks = new TaskList();
	}

	public Scheduler(string computer)
	{
		tasks = new TaskList();
		TargetComputer = computer;
	}
}
