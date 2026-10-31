using NAnt.Core;

namespace Redbox.NAnt.Core;

public abstract class TaskBase : Task, ITaskLogger
{
	public TaskBase()
	{
	}

	public new void Log(Level messageLevel, string message, params object[] p)
	{
		base.Log(messageLevel, message, p);
	}
}
