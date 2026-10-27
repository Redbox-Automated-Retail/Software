using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.Kernel;

public static class WorkQueueFunctions
{
	[KernelFunction(Name = "kernel.workqueuedelete")]
	internal static void DeleteFrmoQueue(string id)
	{
		ServiceLocator.Instance.GetService<IUpdateService>()?.DeleteFromWorkQueue(id);
	}

	[KernelFunction(Name = "kernel.workqueueclear")]
	internal static void ClearQueue()
	{
		ServiceLocator.Instance.GetService<IUpdateService>()?.ClearWorkQueue();
	}
}
